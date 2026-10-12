"""Build the football listening preview; never writes Unity assets.

Requires NumPy and imageio-ffmpeg. Run download_football_candidates.py first.
Reusable basketball clips are copied byte for byte from the approved delivery.
New designed cues use deterministic synthesis, not an AI service.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import wave

import numpy as np
import build_basketball_preview as dsp
from build_basketball_preview import Pack, SR, FF, fade, filtered, noise, tone, place, chime, whoosh

ROOT = Path(__file__).resolve().parents[2]


def stretch(x, factor):
    """Offline resampling: changes both duration and pitch; not time stretching."""
    return np.interp(np.arange(max(32, round(len(x)*factor)))/factor, np.arange(len(x)), x)


def unit(x):
    return x/max(float(np.max(np.abs(x))), 1e-9)


def modal(duration, base, ratios=(1, 2.13, 3.81, 5.7)):
    t = dsp.timebase(duration)
    x = sum(np.sin(2*np.pi*base*r*t)*np.exp(-t*(5+i*5))/(1+i*1.8)
            for i, r in enumerate(ratios))
    return fade(x+.12*noise(duration, 900, 6500)*np.exp(-t*85), .001, .04)


class FootballPack(Pack):
    def reuse(self, cue, title, category, note, loop=False):
        records = [r for r in self.shared if r['cue'] == cue]
        if not records:
            raise ValueError('Missing approved cue '+cue)
        variants = []
        for record in records:
            source = ROOT/record['file']
            if hashlib.sha256(source.read_bytes()).hexdigest() != record['sha256']:
                raise ValueError('Approved basketball hash changed: '+record['file'])
            dest = self.root/'Clips'/source.name
            shutil.copy2(source, dest)
            with wave.open(str(dest), 'rb') as w:
                assert (w.getframerate(), w.getnchannels(), w.getsampwidth()) == (SR, 1, 2)
                x = np.frombuffer(w.readframes(w.getnframes()), dtype='<i2').astype(float)/32768
            name = dest.stem
            self.samples[name] = x
            peak = float(abs(x).max())
            self.metrics.append(dict(id=name, duration=len(x)/SR,
                peak_dbfs=round(float(20*np.log10(peak+1e-12)), 2),
                rms_dbfs=round(float(20*np.log10(np.sqrt(np.mean(x*x))+1e-12)), 2),
                clipped_samples=int(np.sum(abs(x)>=1)), dc=float(x.mean()),
                endpoint_jump=float(abs(x[-1]-x[0])), bytes=dest.stat().st_size,
                sha256=record['sha256'], reuse_asset=record['file']))
            variants.append(dict(file=dest.relative_to(self.root).as_posix(), duration=len(x)/SR,
                waveform=[round(float(v),4) for v in np.max(np.abs(np.pad(x,(0,(-len(x))%80))).reshape(80,-1),axis=1)]))
        self.items.append(dict(id=cue, title=title, category=category,
            origin='Reused from approved basketball bank', note=note,
            sources=['shared-bank'], processing='Byte-identical approved WAVs; no gain, pitch or duration change. Reuse the same assets after approval.',
            variants=variants, alternative=False, loop=loop, reuse=True))


def build(output):
    dsp.RNG = np.random.default_rng(12102026)
    root = output.resolve()
    shared = json.loads((ROOT/'ArtSource/Audio/Basketball/delivery.json').read_text(encoding='utf-8'))
    (root/'Raw').mkdir(parents=True, exist_ok=True)
    (root/'Raw/shared-basketball-provenance.json').write_text(json.dumps(shared,indent=2)+'\n',encoding='utf-8')
    source_manifest = json.loads((root/'sources.json').read_text(encoding='utf-8'))
    source_manifest['sources'] = [s for s in source_manifest['sources'] if s['id']!='shared-bank']
    provenance = root/'Raw/shared-basketball-provenance.json'
    source_manifest['sources'].append(dict(id='shared-bank', author='Project synthesis + Kenney CC0 UI',
        source_page='Evidence/shared-bank.html', license='Original project synthesis / CC0 UI',
        source_quality='Previously approved basketball delivery; original source records retained in repository',
        file='Raw/shared-basketball-provenance.json', bytes=provenance.stat().st_size,
        sha256=hashlib.sha256(provenance.read_bytes()).hexdigest()))
    (root/'sources.json').write_text(json.dumps(source_manifest,indent=2)+'\n',encoding='utf-8')
    p = FootballPack(root)
    p.shared = shared['files']
    (root/'Evidence/shared-bank.html').write_text('''<!doctype html><meta charset="utf-8"><title>Shared basketball audio</title>
<style>body{font:17px/1.6 system-ui;max-width:800px;margin:40px auto;padding:20px}pre{white-space:pre-wrap;overflow-wrap:anywhere}</style>
<h1>Approved basketball sounds reused for football</h1><p>UI clips originate from <a href="https://kenney.nl/assets/interface-sounds">Kenney Interface Sounds</a> (CC0). The shared prop effects, body/fabric cues, score and coastal air are original deterministic project synthesis. These preview copies preserve the approved WAV bytes. Exact asset references and hashes are in the <a href="../Raw/shared-basketball-provenance.json">provenance manifest</a>.</p><p>Reusing a sound does not mean its football trigger is implemented. This is an approval preview.</p>''',encoding='utf-8')
    ball='Ball & goal'; movement='Movement & defense'; props='Props'; match='Match & ambience'; ui='Buttons'

    # Separate actual recordings from constructed layers and comparisons.
    kick = p.source('kick', .018, .40, hp=55, lp=7500, tail=.16)
    alt = p.source('kick_alt', .165, .49, hp=70, lp=7500, tail=.15)
    kicks = [kick, stretch(kick,1.035), stretch(kick,.97)]
    p.add('kick-light','Light kick / short pass',ball,[filtered(kick,65,5300),filtered(alt,65,5300)],'Edited CC0 recordings',
        'A quick release of the existing Kick action. Softer than the full-power shot; no new pass ability.',
        ['kick','kick_alt'],'Trims 0.018–0.400 and 0.165–0.490 s; reduced high end and tail; mono 32 kHz; peak 0.27',level=.27)
    p.add('kick-shot','Powerful kick',ball,kicks,'Recorded CC0 + derived variations',
        'Main full-power kick. Three small pitch/length variants of one recording, not three independent takes.',
        ['kick'],'Trim 0.018–0.400 s; 55 Hz high-pass / 7.5 kHz low-pass; tail damping; resampling factors 1, 1.035, 0.97',level=.51)
    p.add('kick-alt','Alternate kick character',ball,[alt,p.source('pillow',3.90,4.24,hp=60,lp=7000,tail=.13)],'Recorded CC0 / pillow foley comparison',
        'Compare with Powerful kick: variation 1 is another football; variation 2 is explicitly pillow foley.',
        ['kick_alt','pillow'],'Trims 0.165–0.490 and 3.900–4.240 s; filtered/damped; matched peak for comparison',level=.51,alternative=True)
    p.add('dribble-touch','Foot dribble touch',ball,[filtered(x,85,4500)[:round(.16*SR)] for x in kicks],'Designed from recorded kick',
        'Quiet contact at the displayed foot-to-ball touch. No stationary basketball-style bouncing.',
        ['kick'],'Main kick trimmed to 160 ms with short fade, darker filtering and reduced level',level=.14)
    p.add('receive','Trap / receive at feet',ball,[filtered(kick,80,3300)[:round(.21*SR)],filtered(alt,80,3300)[:round(.20*SR)]],'Designed from recorded kicks',
        'Soft cushioning contact when control is actually acquired, including Portal arrival.',
        ['kick','kick_alt'],'Kick source trims filtered to 3.3 kHz, shortened to 0.20–0.21 s; reduced level',level=.20)
    grass_trims=[(.015,.39),(.71,1.10),(1.405,1.815),(2.19,2.60)]
    grass=[p.source('grass',a,b,hp=110,lp=7200) for a,b in grass_trims]
    bounces=[]
    for i in range(3):
        t=dsp.timebase(.28)
        x=.32*np.sin(2*np.pi*(125*t-115*t*t))*np.exp(-t*32)+.11*noise(.28,250,3800)*np.exp(-t*45)
        bounces.append(place(.32,[(0,x,1),(.005,unit(grass[i]),.075)]))
    p.add('turf-bounce','Ball bounce on turf',ball,bounces,'Generated + grass recording layer',
        'Low, damp ball contact for the grass pitch. Designed approximation; judge against the kick and steps.',
        ['grass'],'Original damped ball resonance and noise, plus low-level grass footsteps; three deterministic variations',level=.30)
    roll=dsp.loop_air(2)*(.40+.15*np.sin(2*np.pi*9*dsp.timebase(2)))
    p.add('turf-roll','Ball rolling on turf',ball,[filtered(roll,180,1800)],'Generated',
        'Optional very quiet texture; play only while a free ball rolls. This sample previews one loop.',level=.035,loop=True)
    pipe=p.source('pipe',0,.54,hp=120,lp=8500,tail=.27)
    p.add('goal-post','Goal post / crossbar',ball,[place(.62,[(0,unit(pipe),.65),(0,unit(kick),.30)]),modal(.62,430)],'Designed foley / generated comparison',
        'Variation 1 layers metal-pipe foley with a football impact. Variation 2 is a synthesized hollow frame.',
        ['pipe','kick'],'Pipe 0–0.540 s plus selected kick; second variant is original modal synthesis. Neither is a direct goal-post recording.',level=.46)
    net=p.source('net',.018,.65,hp=140,lp=9500,tail=.25)
    p.add('goal-net','Football net impact',ball,[net],'Recorded CC0 (contact microphone)',
        'Actual soccer-net source. Contact-mic character may sound close or rigid; approve by ear.',
        ['net'],'Trim 0.018–0.650 s; 140 Hz high-pass, 9.5 kHz low-pass; damp tail and fade',level=.34)
    p.add('net-brush-football','Light net movement',ball,[filtered(net,350,6000)],'Edited CC0 recording',
        'Side/roof/back brush without a new goal. Also supports net settling.',
        ['net'],'Same football net trim with softer filtering and lower gain',level=.12)
    p.add('deflection','Ball interception / body deflection',ball,[filtered(kick,95,2700),filtered(alt,95,2700)],'Designed from recorded kicks',
        'Padded impact on an actual player or successful ball tackle; separate from the slide sound.',
        ['kick','kick_alt'],'Recorded kick transients filtered to 2.7 kHz, shorter tails, lower level',level=.26)
    p.reuse('boundary','Pitch boundary touch',ball,'Same restrained boundary accent. Only on a real containment contact; the game does not award throw-ins or corners.')

    p.add('grass-walk','Walking on grass',movement,grass,'Recorded CC0 foley',
        'Four isolated hiking-boot/grass foley steps. Intended as a turf base, not a claim of recorded football cleats.',
        ['grass'],f'Trims {grass_trims}; 110 Hz high-pass, 7.2 kHz low-pass; mono and fades',level=.20)
    running=[place(.33,[(0,stretch(x,.83),.8),(0,tone(.16,95,60,35),.10)]) for x in grass]
    p.add('grass-run','Running / sprinting on grass',movement,running,'Designed from grass foley',
        'Four firmer steps for running, dash and Turbo. Trigger with the feet rather than a fixed looping cadence.',
        ['grass'],'Walking-source trims shortened by resampling factor 0.83 and layered with a quiet low thud',level=.29)
    p.add('trail-alt','Alternate outdoor running steps',movement,[p.source('trail',a,b,hp=120,lp=6500) for a,b in [(0,.40),(.58,.97),(1.21,1.58)]],'Recorded CC0 comparison',
        'Actual trail-running recording; compare surface grit with the designed grass run before choosing.',
        ['trail'],'Trims 0–0.400, 0.580–0.970, 1.210–1.580 s; filtered and faded',level=.29,alternative=True)
    scuffs=[]
    for i in range(2):
        t=dsp.timebase(.26)
        scuffs.append(noise(.26,450,5100)*np.sin(np.pi*t/.26)**1.5*.2+place(.26,[(0,unit(grass[i]),.18)]))
    p.add('grass-cut','Turn / stop / pressure shuffle',movement,scuffs,'Generated + grass recording layer',
        'Short turf scrape for cuts and defensive footwork. No squeaking hardwood shoes on this pitch.',
        ['grass'],'Filtered friction noise with grass footstep layer and a 260 ms envelope',level=.15)
    slides=[]
    for i in range(2):
        t=dsp.timebase(.43)
        slides.append(place(.43,[(0,unit(grass[i]),.26)])+.18*noise(.43,300,5500)*np.sin(np.pi*t/.43)**.6*np.exp(-t*1.8))
    p.add('tackle-slide','Slide tackle across turf',movement,slides,'Generated + grass recording layer',
        'About 0.43 seconds to fit the current 0.425-second tackle. The slide still plays when the tackle misses.',
        ['grass'],'Short grass friction envelope + recorded footstep onset; no collision baked into slide',level=.32)
    p.reuse('body','Tackle body contact',movement,'Approved padded body sound, only on a confirmed player hit. Keep it below the kick.')
    falls=[place(.52,[(0,unit(grass[i]),.38),(.035,tone(.29,105,48,18),.4),(.08,noise(.35,220,3800)*np.exp(-dsp.timebase(.35)*16),.07)]) for i in range(2)]
    p.add('turf-fall','Fall onto turf',movement,falls,'Generated + grass recording layer',
        'Ground impact during the hit/fall animation. Play after body contact, when the body reaches the pitch.',
        ['grass'],'Grass layer, damped low tone and fabric-like noise; no human vocalization',level=.28)
    p.add('turf-land','Jump landing on turf',movement,[place(.4,[(0,unit(grass[0]),.7),(.045,unit(grass[1]),.5)]),place(.4,[(0,unit(grass[2]),.7),(.05,unit(grass[3]),.5)])],'Layered CC0 grass foley',
        'Two planted soles after a genuine landing; avoid extra run steps on the same frame.',
        ['grass'],'Paired grass footstep trims at 45/50 ms spacing with short final fade',level=.29)
    p.reuse('cloth','Charge / cancel / get-up fabric',movement,'Reuse the quiet jersey rustle on pose changes. The one-second kick charge needs no loud continuous alarm.')
    p.reuse('swipe','Missed reach / tackle air',movement,'Optional light clothing/air accent. Turf slide already communicates a miss; do not play an error buzzer for every miss.')
    p.add('dash-start','Dash begins',movement,[whoosh(.22)*.4],'Generated',
        'Short local rush when stamina-spending dash actually starts; distinct from the Turbo prop.',level=.12)
    p.add('stamina-empty','Out of stamina',movement,[chime([310,230],.28)],'Generated',
        'Soft one-time downward cue on exhaustion. Do not repeat while Dash remains held.',level=.105)
    p.add('stamina-ready','Dash available again',movement,[chime([350,440],.25)],'Generated',
        'Optional quiet cue only after release and the 25% restart threshold; omit if it feels busy.',level=.07,alternative=True)

    for cue,title,note,loop in [
        ('countdown','Prop countdown 5–4–3–2–1','Same blue hologram countdown identity. One tick per displayed number, spatially near the pickup.',False),
        ('spawn','Prop materializes','Reuse the approved appearance cue at the real spawn.',False),
        ('pickup','Prop pickup / replacement','Same collection sound when inventory changes successfully. Replacing a prop needs no second reward sound.',False),
        ('turbo-start','Team Turbo starts','Same approved activation sound for the five-second team boost.',False),
        ('turbo-loop','Team Turbo active texture','Quiet optional texture for affected players; exactly the approved loop, played once here.',True),
        ('turbo-end','Team Turbo expires','Reuse at actual expiry, once for the local affected player.',False),
        ('portal-open','Portal pair opens','Same portal identity at source and receiver.',False),
        ('portal-in','Ball enters portal','Time to the ball shrinking at the source.',False),
        ('portal-out','Ball emerges from portal','Time to the ball growing at the receiver’s feet.',False),
        ('portal-close','Portal pair closes','Use at visible closure after the 0.7-second transfer.',False),
        ('interrupt','Portal interrupted','Reuse when an accepted transfer is interrupted; cancelled Wall previews use the Back UI cue.',False),
    ]: p.reuse(cue,title,props,note,loop)
    p.add('wall-preview','Wall placement preview opens',props,[chime([330,495],.30)+whoosh(.30)*.08],'Generated',
        'Quiet hologram cue on the first Wall tap. It has not consumed the prop yet.',level=.115)
    p.add('wall-place','Wall deployed',props,[place(.72,[(0,whoosh(.40),.30),(.15,modal(.55,135,(1,1.73,2.81)),.45),(.21,tone(.27,78,48,13),.25)])],'Generated',
        'Warm, solid build-up for a successfully placed wall. Stronger than preview, shorter than the effect.',level=.37)
    p.add('wall-hit','Football rebounds from Wall',props,[place(.44,[(0,unit(kick),.5),(0,modal(.40,180),.28)]),place(.44,[(0,unit(alt),.5),(0,modal(.4,195),.28)])],'Designed recording + synthesis',
        'Ball impact plus a restrained barrier resonance, only when the swept wall collision resolves.',
        ['kick','kick_alt'],'Recorded football kick layer plus original modal barrier resonance; two takes',level=.42)
    p.add('wall-expire','Wall expires',props,[whoosh(.46,False)*.20+chime([370,250,180],.46)*.25],'Generated',
        'Soft dissolve at the 15-second lifetime. A goal/reset clearing walls should stop effects without a pile of expiry sounds.',level=.14)

    whistle=p.source('whistle',.22,.98,hp=1200,lp=7600)
    p.add('kickoff-whistle','Kickoff whistle',match,[fade(whistle[:round(.52*SR)],.015,.06)],'Edited CC0 recording',
        'One short whistle when the kickoff countdown releases both teams. Formal match only.',
        ['whistle'],'Trim 0.220–0.740 s; 1.2 kHz high-pass, 7.6 kHz low-pass; softened attack/tail',level=.25)
    short=fade(whistle[:round(.17*SR)],.012,.035)
    p.add('final-whistle','Final whistle',match,[place(1.17,[(0,short,1),(.25,short,1),(.50,whistle[:round(.58*SR)],1)])],'Edited CC0 recording',
        'Short–short–long at the actual end of the match. No whistle on the seamless regulation-to-overtime transition.',
        ['whistle'],'Same single whistle recording edited into three blasts; 1.17 s total',level=.25)
    p.reuse('score','Goal confirmed',match,'Reuse the approved score accent on an authoritative score change. Net impacts stay separate. Silent for Explore practice scores because none are awarded.')
    p.add('overtime','Golden goal begins',match,[chime([392,523.25,783.99],.75)],'Generated',
        'Brief warm alert when tied regulation flows into golden goal. Does not imply play has stopped.',level=.19)
    p.add('result-win','Match win',match,[chime([392,493.88,587.33,783.99],1.05)],'Generated',
        'A short local-team result phrase after the final whistle. No long music bed.',level=.23)
    p.add('result-loss','Match loss',match,[chime([392,349.23,293.66],.85)],'Generated',
        'Restrained result phrase for the losing local team; neutral, without ridicule.',level=.16)
    p.add('result-draw','Match draw',match,[chime([392,523.25,392],.85)],'Generated',
        'Balanced result phrase when golden goal expires without a winner.',level=.17)
    p.reuse('air','Coastal pitch ambience',match,'Reuse the existing quiet coastal-air bed. Crowd chants and music are omitted from the main proposal.',True)

    for cue,title,note in [
        ('ui-click','Action button press','Kick, Dash, Pressure, Tackle, Jump and prop buttons: one local press cue, separate from gameplay outcome.'),
        ('ui-confirm','Confirm / Start match / Place','Use only for an accepted UI confirmation; avoid duplicating the Wall deployment accent.'),
        ('ui-back','Back / Cancel','Back to play, keep playing, cancel kick and cancel Wall preview.'),
        ('ui-toggle','Toggle setting','Reuse for actual toggles; no ticking sound for every slider movement.'),
        ('ui-select','Camera / guide / team choice','One selection cue when the choice changes. Server rejection uses the error cue.'),
        ('ui-error','Rejected action','Low-key local feedback for a rejected request or failed team choice; no repeated alarm on disabled buttons.'),
        ('ui-open','Settings / guide / team panel opens','Reuse for opening the relevant panel; automatic team selection should cue once.'),
        ('ui-close','Settings / panel closes','Reuse for panel closure and returning to play.'),
    ]: p.reuse(cue,title,ui,note)

    reels=[]
    for key,title,category in [('ball-sounds','Ball & goal',ball),('movement-sounds','Grass, tackles & dash',movement),('prop-sounds','Shared props & new Wall',props),('match-sounds','Whistles & match results',match),('button-sounds','Approved shared buttons',ui)]:
        cues=[(i['id'],1) for i in p.items if i['category']==category and not i['loop']]
        reels.append(p.reel(key,title,cues,.48))
    events=[];timeline=[]
    def event(t,cue,v=1,gain=1):
        events.append((t,p.clip(cue,v),gain));timeline.append(dict(time=t,cue=cue,variation=v))
    event(.15,'kickoff-whistle')
    for i in range(6):
        event(1+i*.4,'grass-run',i%4+1,.65)
        if i%2==0:event(1.13+i*.4,'dribble-touch',i%3+1,.8)
    event(3.5,'kick-light');event(4.2,'receive');event(4.75,'grass-cut');event(5.2,'kick-shot')
    event(5.9,'goal-post',1,.8);event(6.23,'goal-net');event(6.38,'score')
    event(7.4,'pickup');event(8.1,'wall-preview');event(8.9,'wall-place')
    event(10,'kick-shot',2);event(10.65,'wall-hit');event(11.45,'tackle-slide');event(11.67,'deflection')
    event(12.7,'portal-open');event(12.84,'portal-in');event(13.09,'portal-out');event(13.4,'receive');event(13.47,'portal-close')
    event(14.5,'turbo-start');event(14.75,'grass-run',3,.7);event(15.05,'grass-run',4,.7);event(15.35,'grass-run',1,.7)
    event(16.2,'wall-expire')
    sequence=fade(place(18,events),.01,.12)
    gain=min(1,.78/max(float(abs(sequence).max()),1e-9));sequence*=gain
    scene_path=root/'Reels/pitch-scene.wav'
    with wave.open(str(scene_path),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(SR);w.writeframes((sequence*32767).round().astype('<i2').tobytes())
    subprocess.run([FF,'-v','error','-y','-i',str(scene_path),'-c:a','libmp3lame','-b:a','160k',str(scene_path.with_suffix('.mp3'))],check=True)
    reels.insert(0,dict(id='pitch-scene',title='Illustrative football sequence',file='Reels/pitch-scene.mp3',duration=18,
        timeline=sorted(timeline,key=lambda e:e['time']),mix_gain=gain,
        note='Edited audition only: elapsed time jumps between examples, including Wall expiry. Not gameplay capture or proof of synchronization.'))
    data=dict(title='Football sound preview',version=1,generated_at_utc=datetime.now(timezone.utc).isoformat(),
        status='Awaiting user approval; football audio is not integrated',
        base='All file paths relative to this manifest directory',sample_rate=SR,items=p.items,reels=reels,sources=p.sources['sources'])
    report=dict(clip_count=len(p.metrics),event_families=len(p.items),wav_bytes=sum(m['bytes'] for m in p.metrics),
        total_seconds=round(sum(m['duration'] for m in p.metrics),3),
        reused_clips=sum('reuse_asset' in m for m in p.metrics),reused_families=sum(bool(i.get('reuse')) for i in p.items),
        auditory_review='Technical signal inspection only; user listening approval required',clips=p.metrics)
    (root/'manifest.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    (root/'Evidence/clip-qa.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    template=Path(__file__).with_name('football_preview.html').read_text(encoding='utf-8')
    (root/'index.html').write_text(template.replace('__AUDIO_MANIFEST__',json.dumps(data,ensure_ascii=False).replace('</','<\\/')),encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='clips'},indent=2))


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output',type=Path,default=ROOT/'Builds/FootballAudioPreview')
    build(parser.parse_args().output)

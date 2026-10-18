"""Author a Golf/Fishing approval soundboard outside Unity.

Needs NumPy and imageio-ffmpeg. Run download_golf_fishing_candidates.py first.
Reused WAVs are verified against their delivery manifests and copied unchanged.
Missing effects use original deterministic DSP, not an AI service.
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
import build_basketball_preview as d
from build_basketball_preview import SR, FF, fade, filtered, noise, tone, place, chime, whoosh

ROOT = Path(__file__).resolve().parents[2]


def unit(x):
    return x / max(float(np.max(np.abs(x))), 1e-9)


def pitch(x, ratio):
    return np.interp(np.arange(round(len(x)/ratio))*ratio, np.arange(len(x)), x)


def loop(x, overlap=.08):
    """Overlap the tail with the beginning, then repair the final sample seam."""
    n = min(round(overlap*SR), len(x)//4)
    w = np.linspace(0, 1, n)
    y = np.concatenate((x[-n:]*(1-w)+x[:n]*w, x[n:-n]))
    y -= np.mean(y)
    m = 64
    y[-m:] -= np.linspace(0, 1, m)*(y[-1]-y[0])
    return y


def tap(duration=.18, hz=720, decay=45):
    t = d.timebase(duration)
    return fade((.6*np.sin(2*np.pi*hz*t)+.25*np.sin(2*np.pi*hz*2.21*t)+.12*noise(duration,700,6500))*np.exp(-t*decay),.001,.02)


def bubbles(duration=.6):
    events=[]
    for i in range(6):
        events.append((i*duration/8, tone(duration/3, 700+i*130, 210+i*37, decay=25), .55-i*.05))
    return place(duration, events)


class Preview(d.Pack):
    def reuse(self, bank, oldcue, cue, title, category, note, looped=False):
        records = self.banks[bank]
        if bank == 'golf':
            records = [r for r in records if ('Walkway/' in r['file']) == (oldcue == 'walkway')]
        else:
            records = [r for r in records if r['cue'] == oldcue]
        if not records:
            raise ValueError('Missing shared cue: '+bank+'/'+oldcue)
        variants=[]
        for i,r in enumerate(records):
            source=ROOT/r['file']
            if hashlib.sha256(source.read_bytes()).hexdigest().lower()!=r['sha256'].lower():
                raise ValueError('Approved shared WAV changed: '+r['file'])
            path=self.root/'Clips'/f'{cue}-{i+1:02d}.wav'
            shutil.copy2(source,path)
            with wave.open(str(path),'rb') as f:
                if (f.getframerate(),f.getnchannels(),f.getsampwidth())!=(SR,1,2):
                    raise ValueError('Unexpected shared WAV format')
                x=np.frombuffer(f.readframes(f.getnframes()),dtype='<i2').astype(float)/32768
            self.samples[path.stem]=x
            self.metrics.append(dict(id=path.stem,duration=len(x)/SR,
                peak_dbfs=round(float(20*np.log10(abs(x).max()+1e-12)),2),
                rms_dbfs=round(float(20*np.log10(np.sqrt(np.mean(x*x))+1e-12)),2),
                clipped_samples=int(np.sum(abs(x)>=1)),dc=float(x.mean()),
                endpoint_jump=float(abs(x[-1]-x[0])),bytes=path.stat().st_size,
                sha256=hashlib.sha256(path.read_bytes()).hexdigest(),reuse_asset=r['file'],bank=bank))
            variants.append(dict(file=path.relative_to(self.root).as_posix(),duration=len(x)/SR,
                waveform=[round(float(v),4) for v in np.max(np.abs(np.pad(x,(0,(-len(x))%80))).reshape(80,-1),axis=1)]))
        self.items.append(dict(id=cue,title=title,category=category,
            origin='Retained user-approved Golf recording' if bank=='golf' else 'Reused approved '+bank.title(),
            note=note,sources=['shared-'+bank],processing='Byte-identical approved WAV(s). No pitch, gain or duration changes. Future integration should reference the original asset.',
            variants=variants,loop=looped,reuse=True,alternative=False))

    def new(self,cue,title,cat,x,note,ids=None,process=None,level=.32,looped=False,alternative=False):
        self.add(cue,title,cat,x,'Edited CC0 recording' if ids else 'Generated locally',note,
                 ids,process,level=level,loop=looped,alternative=alternative)

    def recorded(self,cue,title,cat,source,trims,note,level=.32,hp=70,lp=9000,tail=None):
        clips=[self.source(source,a,b,hp,lp,tail) for a,b in trims]
        self.new(cue,title,cat,clips,note,[source],
                 f'Isolated source trims (seconds): {trims}; mono 32 kHz; DC removal; {hp} Hz high-pass / {lp} Hz low-pass; fades; peak {level}. No pitch shift.',level)
        return clips


def prepare_shared(root):
    paths={'basketball':'ArtSource/Audio/Basketball/delivery.json',
           'football':'ArtSource/Audio/Football/delivery.json',
           'golf':'ArtSource/Audio/Golf/Recordings/delivery.json'}
    sources=json.loads((root/'sources.json').read_text(encoding='utf-8'))
    sources['sources']=[r for r in sources['sources'] if not r['id'].startswith('shared-')]
    banks={}
    for bank,path in paths.items():
        data=json.loads((ROOT/path).read_text(encoding='utf-8'))
        banks[bank]=data.get('files',data.get('clips'))
        rel=f'Raw/shared-{bank}.json'
        shutil.copy2(ROOT/path,root/rel)
        sources['sources'].append(dict(id='shared-'+bank,author=bank.title()+' project bank',
            file=rel,sha256=hashlib.sha256((root/rel).read_bytes()).hexdigest(),
            source_page='Evidence/shared-sources.html',
            license='User-supplied for Golf; retained existing use' if bank=='golf' else 'CC0 recordings / original project synthesis',
            source_quality='Existing approved delivery WAV, copied unchanged'))
    (root/'sources.json').write_text(json.dumps(sources,indent=2)+'\n',encoding='utf-8')
    (root/'Evidence/shared-sources.html').write_text('''<!doctype html><meta charset="utf-8"><title>Shared audio provenance</title>
<style>body{font:17px/1.6 system-ui;max-width:800px;margin:40px auto;padding:20px}</style>
<h1>Approved sounds retained and reused</h1><p>Basketball UI comes from <a href="https://kenney.nl/assets/interface-sounds">Kenney Interface Sounds</a> (CC0). Its prop, fabric, score and outdoor-air effects use original project synthesis. Football grass footsteps and results retain their existing delivery and source records.</p>
<p>Golf GO and walkway steps were supplied and approved by the user on 17 October 2026. They are retained for their existing Golf use, not described as public-domain downloads. No evidence here grants redistribution of those recordings outside this project.</p>
<p>Exact repository-relative references and hashes: <a href="../Raw/shared-basketball.json">Basketball</a>, <a href="../Raw/shared-football.json">Football</a>, <a href="../Raw/shared-golf.json">Golf</a>. New preview copies do not change any game files.</p>''',encoding='utf-8')
    return banks


def build(output):
    d.RNG=np.random.default_rng(18102026)
    root=output.resolve()
    for folder in ['Raw','Evidence']:(root/folder).mkdir(parents=True,exist_ok=True)
    banks=prepare_shared(root)
    p=Preview(root);p.banks=banks
    g='Golf · Shots';cart='Golf · Buggy';f='Fishing · Cast & fight';gear='Fishing · Skills & treasure'
    move='Shared · Movement';match='Shared · Match & rewards';ui='Shared · Buttons';amb='Shared · Ambience';keep='Golf · Kept recordings'

    # Golf's recorded GO and walkway selection remains exactly as approved.
    p.reuse('golf','go','golf-go','Golf GO — keep current recording',keep,'Retain your existing GO sound exactly. Once when the opening countdown releases.')
    p.reuse('golf','walkway','golf-walkway','Golf walkway — keep your 3 takes',keep,'Retain the supplied Path / Approach / Paving foot contacts, original pitch and level. Golf only.')

    p.recorded('golf-swing','Club strikes ball — full swing',g,'golf_drive',[(.655,.96)],'Actual golf driver contact. Trigger at club/ball impact, after the downswing; strength controls playback gain.',.48,80,11000)
    p.recorded('golf-putt','Putter contacts — 3 takes',g,'putt',[(1.165,1.40),(3.815,4.06),(6.085,6.34)],'Shorter, quieter than a full swing. No large-ball basketball bounce or football thump.',.31,100,10000)
    p.recorded('golf-swish','Club downswing through air',g,'rod',[(2.62,2.91),(5.21,5.53)],'A rod swish edited as club foley. Play just before actual contact; a putt uses little or none.',.20,380,7400)
    t=d.timebase(2.6)
    charge=loop((.6*np.sin(2*np.pi*180*t)+.12*np.sin(2*np.pi*360*t))*(.72+.18*np.cos(2*np.pi*t/2.6)))
    p.new('golf-charge','Soft aim / power charge',g,[charge],'Quiet low-pitch bed while holding power; fade on release/cancel. No full-charge beep: that cue was removed in your current Golf.',level=.065,looped=True)
    # Small, dry contact designs suit a golf ball rather than a hollow sports ball.
    p.new('golf-turf','Small ball lands on turf',g,[tap(.20,160,35)+.14*noise(.20,100,2600)*np.exp(-d.timebase(.20)*25),tap(.17,180,43)],'First touchdown and energetic secondary bounces only. Scale with impact; no stationary chatter.',level=.26)
    sand=p.recorded('golf-sand','Ball lands in sand',g,'sand',[(7.38,7.83),(8.35,8.78)],'Granular sand foley; source contains some coastal background. Shortened to avoid a wave on every contact.',.24,550,7800,.14)
    p.new('golf-stone','Ball strikes stone / hard path',g,[tap(.18,1450,48),tap(.16,1820,55)],'Dry, bright small-ball tick for real stone/path collisions, gated by impact speed.',level=.32)
    wood=p.recorded('golf-wood','Ball strikes timber / palm trunk',g,'wood',[(5.85,6.07),(9.66,9.89)],'Short hardwood foley, used only when the actual obstacle is timber. No metal-rim clang.',.26,100,6500,.07)
    p.new('golf-ball-hit','Golf ball hits another ball',g,[tap(.12,2250,70),tap(.11,2100,78)],'One cue for a resolved ball pair. Do not emit once from each collider.',level=.28)
    rolling=loop(d.loop_air(2)*(.8+.12*np.sin(2*np.pi*9*d.timebase(2))))
    p.new('golf-roll','Close golf-ball rolling',g,[rolling],'Very quiet near the ball, linked to actual ground speed; stop at rest or while airborne. Rough/sand may darken the same loop.',level=.055,looped=True)
    splash=p.source('splash',.50,1.04,100,9000)
    p.new('water-entry','Ball / object enters water',g,[splash,pitch(splash,.85)],'Golf pond/sea entry; also a shared larger object splash. Trigger once at the actual water plane, before recovery.', ['splash'],'Trim .50–1.04 seconds; second take resampled to 85% pitch/speed; fades and peak scaling.',.38)
    p.recorded('golf-cup','Golf ball drops into cup',g,'golf_cup',[(.38,1.84)],'Actual cup rattle. Play only for an accepted target cup; the owner gets the separate quiet success cue.',.36,90,9000)
    p.new('golf-hole','Hole completed',g,[chime([660,880],.55)],'Short local acknowledgement after the cup rattle. A helper shot credits the ball owner.',level=.23)
    p.new('golf-finish','All five holes completed',g,[chime([523.25,659.25,784,1046.5],.95)],'Personal completion cue, not a winner announcement. Match ranks settle separately.',level=.29)
    p.reuse('basketball','interrupt','recovery','Recovery / interrupted action',g,'Quiet shared reset cue for wrong cup, off-course recovery or a fight timeout. No penalty-stroke implication.')

    p.reuse('basketball','spawn','cart-summon','Buggy appears / supply becomes ready',cart,'On successful buggy summon; also a nearby shore supply becoming available. Never on rejected requests.')
    p.new('cart-recall','Buggy recalled',cart,[whoosh(.35,False)*.5+tone(.35,260,140,16)],'Recall only when a visible buggy actually disappears, including accepted Golf aim auto-recall.',level=.19)
    p.new('cart-seat','Sit in / leave buggy',cart,[place(.30,[(0,tap(.12,170,30),.8),(.04,whoosh(.2,False),.14)])],'Soft seat/body and fabric foley. No door slam: the cart has open sides.',level=.22)
    t=d.timebase(2)
    motor=loop((np.sin(2*np.pi*110*t)+.3*np.sin(2*np.pi*220*t)+.12*np.sin(2*np.pi*440*t))*(.85+.08*np.cos(2*np.pi*12*t)))
    p.new('cart-motor','Quiet electric-style buggy motor',cart,[motor],'Designed motor bed. Gain/pitch follow actual moving speed; parked/blocked carts stay quiet. Listen with the tyre layer.',level=.11,looped=True)
    p.new('cart-tyres','Tyres on grass / sand',cart,[loop(d.loop_air(2)*(.7+.2*np.sin(2*np.pi*11*t))),loop(d.loop_air(2)*(.7+.2*np.sin(2*np.pi*7*t)))],'Two ground textures for supported wheel travel; blend by surface. No wheel noise in the air.',level=.10,looped=True)
    p.new('cart-brake','Buggy slows to a stop',cart,[fade(noise(.32,160,3500)*np.exp(-d.timebase(.32)*8),.01,.07)],'Restrained tyre scrub only on a meaningful moving-to-stop transition, not every released frame.',level=.18)
    p.new('cart-bump','Buggy obstacle / hard landing',cart,[place(.34,[(0,tap(.25,90,20),1),(.01,tap(.17,340,32),.25)])],'Soft chassis contact for a real obstacle or energetic suspension landing. Pair deduplication prevents chatter.',level=.33)
    p.new('cart-reverse','Reverse engaged — optional',cart,[chime([660,523],.28)],'Optional quiet one-time reverse confirmation. No repeating reversing alarm.',level=.14,alternative=True)

    p.recorded('fish-cast','Fishing rod cast — 2 takes',f,'rod',[(2.60,2.96),(5.19,5.56)],'On accepted cast release, with the rod motion. Water entry is a separate event about 0.65 seconds later.',.29,280,8500)
    p.new('float-splash','Float touches water',f,[pitch(splash,1.35),pitch(splash,1.5)],'Small float splash, lower than a golf ball or fish landing. Match the float reaching water.', ['splash'],'Trim .50–1.04; resample to 135% / 150% pitch and speed; reduced peak.',.23)
    p.new('fish-bite','Bite alert — gentle / clearer',f,[chime([740,988],.28),place(.30,[(0,tone(.13,880,decay=18),.7),(.15,tone(.14,1175,decay=18),.7)])],'Local, once at the actionable ! / 1.8-second Hook window. Compare softer mallet and clearer two-note option.',level=.33)
    p.new('fish-hook','Accepted hook set',f,[place(.25,[(0,tap(.1,520,45),.4),(.02,whoosh(.21),.3),(.04,tone(.16,350,650,20),.12)])],'Short line-tightening accent after the host accepts Hook. Button click alone does not confirm a hook.',level=.28)
    reel=loop(p.source('reel',8.05,10.45,170,6200),.12)
    p.new('fish-reel','Reel winding loop',f,[reel],'Continuous accepted correct-side winding; also the six-second treasure recovery. Stop on release/cool/cancel/finish, with short fades.', ['reel'],'Trim 8.05–10.45; mono; 170 Hz high-pass / 6.2 kHz low-pass; 120 ms loop overlap; seam repair.',.20,True)
    drag=loop(p.source('drag',1.1,3.25,200,7200),.10)
    p.new('fish-drag','Fish pulls line / reel drag',f,[drag],'Separate from winding: only while line actually slips under fish load. Blend quietly with the fight; no constant high-tension siren.', ['drag'],'Trim 1.10–3.25; 200 Hz high-pass / 7.2 kHz low-pass; 100 ms loop overlap; seam repair.',.18,True)
    p.recorded('fish-line','Line tension creak',f,'line',[(.20,.65),(.75,1.18)],'Intermittent nylon tension texture, only under load; never a constant squeal.',.18,200,6500)
    p.new('fish-switch','Switch Left / Right',f,[tap(.11,620,45),tap(.11,820,45)],'Low local tick when the required side changes. Left/right use different pitches, so mono phone playback remains readable.',level=.14)
    p.new('fish-warning','Release to cool',f,[place(.35,[(0,tone(.15,520,decay=20),1),(.17,tone(.15,440,decay=20),.8)])],'Soft falling warning on entering the cooling state. Hysteresis and a cooldown prevent repeated alarm spam.',level=.23)
    p.new('fish-cooled','Line cooled — resume',f,[chime([440,660],.24)],'One restrained recovery note when the cooling hint clears below 58%; do not tick every tension change.',level=.14)
    p.new('fish-surge','Fish surface thrash — 3 sizes',f,[pitch(splash,1.15),splash,pitch(splash,.72)],'Actual fish surface splash only. Larger fish are deeper, not simply much louder; rate-limit repeated visible splashes.', ['splash'],'One small water source at .50–1.04; speed/pitch 115%, 100%, 72%; peak balanced.',.31)
    p.new('fish-land','Landed fish — water / body',f,[place(.62,[(0,splash,.75),(.12,tap(.24,160,26),.3),(.25,bubbles(.30),.08)])],'Physical landing once on confirmed catch. Follow with the shared score cue and a separate Wanted bonus only if earned.', ['splash'],'Water trim .50–1.04 plus original soft body tap and bubble synthesis.',.34)
    p.new('fish-missed','Missed bite / fish escapes',f,[place(.45,[(0,bubbles(.37),.5),(.03,tone(.29,440,230,12),.15)])],'Small downward bubble cue after confirmed missed bite. Timeout can reuse Recovery; cancellation uses Back, not failure.',level=.20)
    p.new('fish-snap','Line snaps',f,[place(.28,[(0,fade(noise(.06,950,10000)*np.exp(-d.timebase(.06)*75),.001,.01),.25),(.015,tone(.23,1250,200,22),.16)])],'Dry, short twang at confirmed Snapped state. No gunshot-like bang.',level=.30)
    p.new('fish-bubbles','Fish respawn bubbles — optional',f,[bubbles(.45)],'Optional quiet nearby visible respawn, after the actual seven-second respawn. Distant or submerged fish remain silent.',level=.10,alternative=True)

    p.reuse('basketball','pickup','gear-pickup','Accepted gear pickup',gear,'Shore bag selection or completed treasure recovery grants an item. No sound on merely opening the selection panel.')
    p.new('treasure-start','Treasure recovery begins',gear,[place(.38,[(0,tap(.22,210,25),.6),(.06,whoosh(.28),.2)])],'Accepted treasure reservation starts the six-second rod recovery. Reuse Reel winding while recovering.',level=.23)
    p.new('treasure-rise','Treasure comes out of water',gear,[place(.85,[(0,pitch(splash,.85),.8),(.25,tap(.2,250,30),.3),(.40,tap(.17,480,35),.2)])],'Water draining and a small wooden lid clack at completion; shared Pickup confirms the awarded gear.', ['splash'],'Water trim .50–1.04 resampled to 85%; original wooden modal lid taps.',.33)
    p.new('sonar','Sonar ping',gear,[place(.75,[(0,tone(.65,720,decay=8),.65),(.12,tone(.60,1080,decay=9),.12)])],'Soft watery ping on acceptance, then at most one quiet repeat every two seconds during the 8-second reveal; only recipients hear it.',level=.24)
    p.new('cool-spool','Cool Spool activates',gear,[place(.55,[(0,whoosh(.45,False),.35),(.05,bubbles(.48),.18)])],'Airy cooling gesture for four seconds of reduced strain. The reel stays mechanically audible.',level=.23)
    p.reuse('basketball','turbo-start','flippers','Flippers activate',gear,'Reuse the approved speed-boost accent for the five-second speed buff; surface footsteps follow the new pace.')
    p.new('power-reel','Power Reel engages',gear,[place(.42,[(0,tap(.09,420,50),.7),(.07,tap(.09,640,55),.45),(.08,tone(.31,180,500,14),.25)])],'Mechanical clutch/whirr on acceptance. Reuse the reel loop with restrained speed change during the four-second buff.',level=.27)
    p.new('bubble-guard','Bubble Guard appears',gear,[place(.65,[(0,bubbles(.55),.7),(.10,chime([523,784],.45),.16)])],'Watery shield forms once; a separate pop confirms consumption at a strong surge. No always-on hum.',level=.26)
    p.new('bubble-block','Bubble Guard absorbs surge',gear,[place(.37,[(0,bubbles(.25),.8),(.025,tone(.28,280,620,18),.15)])],'One protected-surge pop when the guard is consumed; no generic failure or damage cue.',level=.28)
    p.new('quick-lure','Quick Lure prepared',gear,[place(.40,[(0,tap(.10,1900,55),.25),(.06,chime([880,1175],.30),.4)])],'Small tackle jingle on activation. The next cast uses normal cast/splash audio; do not play an extra reward when the lure is consumed.',level=.23)
    p.reuse('basketball','turbo-end','gear-expire','Skill naturally expires',gear,'Reuse the soft expiry cue for the local active effect only. Consumed Bubble Guard uses its pop; reset/travel ends silently.')

    p.reuse('football','grass-walk','grass-walk','Grass walking — 4 takes',move,'Reuse Football grass plants for both islands. Actual planted foot and surface contact, never idle shuffling.')
    p.reuse('football','grass-run','grass-run','Grass running — 4 takes',move,'Reuse Football running contacts. Foot cadence follows movement; avoid court sneaker squeaks outdoors.')
    p.recorded('wood-steps','Wooden pier / bridge steps',move,'wood',[(2.61,2.97),(4.16,4.52),(5.85,6.22),(9.66,10.03)],'Short hardwood contacts as pier foley. Listen for indoor resonance; shorten further if it feels too roomy.',.29,100,7200,.13)
    p.recorded('sand-steps','Sand footsteps — 3 takes',move,'sand',[(3.00,3.37),(7.42,7.81),(8.38,8.78)],'Soft granular shoreline steps. Some sea noise exists in the original; no single long walking recording on each footfall.',.24,650,7000,.15)
    p.new('stone-steps','Stone / hard path — Fishing option',move,[tap(.16,710,44),tap(.15,830,49),tap(.16,640,42)],'New small dry contacts for Fishing hard surfaces. Golf keeps your supplied walkway clips.',level=.25)
    p.new('water-steps','Shallow-water wading',move,[pitch(splash,1.2),pitch(splash,1.05)],'Individual shallow splashes when grounded in water. Do not play footsteps while swimming or airborne.', ['splash'],'Water .50–1.04; resampled to 120% / 105% speed/pitch; softer than object entry.',.25)
    p.reuse('basketball','cloth','cloth','Jump / rod / club clothing',move,'Shared quiet fabric cue for a jump or equipment lift. Do not retrigger continuously while holding aim.')
    p.reuse('football','turf-land','grass-land','Jump lands on grass',move,'Reuse Football grounded landing. Other surfaces select their matching step/splash layer; no sound without landing.')

    p.reuse('basketball','countdown','countdown','Fishing countdown / final seconds',match,'Shared quiet tick for each displayed countdown number; Golf keeps its current opening beep below. Final warning only in the last few seconds, not every timer update.')
    t=d.timebase(.16)
    p.new('golf-countdown','Golf opening countdown — current design',match,[fade((np.sin(2*np.pi*880*t)*.30+np.sin(2*np.pi*1760*t)*.08),.008,.06)],'Retain Golf’s five-number countdown design; recreated from its current oscillator formula for audition. Your recorded GO follows it.',level=.30)
    p.new('round-start','Fishing round starts',match,[chime([523,784,1046.5],.65)],'Bright but short local start after the countdown. Free Fishing has no match start/end alarms.',level=.27)
    p.new('round-end','Fishing time is up',match,[chime([784,659,523],.7)],'Once at confirmed round end. Allow catch/reward tails to finish before a result sting.',level=.25)
    p.new('first-finisher','Golf first finisher / 30 seconds left',match,[chime([660,880,660],.55)],'A neutral notice when the first golfer completes Hole 5. It does not claim they won; no duplicate start of the deadline.',level=.23)
    p.reuse('basketball','score','catch-score','Catch / points confirmed',match,'Shared score accent for your confirmed catch; do not fire again as score numbers animate. Small/medium/large use the same success language.')
    p.new('wanted-bonus','Wanted +3 bonus',match,[chime([1046.5,1318.5],.40)],'One light extra sparkle when the actual +3 bonus is awarded; not for every fish of that size.',level=.19)
    p.new('wanted-change','Wanted size changes',match,[chime([659,784],.32)],'Subtle once-per-minute change cue while a competitive round is live. Never interrupts a bite alert.',level=.14)
    for cue,title in [('result-win','Win / Crew success'),('result-loss','Result / Crew failed'),('result-draw','Tied result')]:
        p.reuse('football',cue,cue,title,match,'Reuse the approved result palette after settled Golf rankings or Fishing Round/Cup/Crew outcomes. No premature winner cue for a personal finish.')
    p.new('cup-champion','Fishing Cup champion',match,[chime([523.25,659.25,784,1046.5,1318.5],1.1)],'A slightly fuller finale after all three Cup rounds; ordinary rounds use the shared result cues.',level=.30)

    ui_notes={
        'click':'Action press, Swing/Putt, Cast/Hook, Jump and Left/Right press edges. Holding never repeats click each frame.',
        'confirm':'Ready, Start, save the two-skill kit, rematch or confirmed Leave. Gameplay success still waits for authority.',
        'back':'Cancel aim/charge/fishing/treasure, Back and Keep playing. A cancel must not sound like a failed catch.',
        'toggle':'Sound/vibration/reduced-motion switches and Ready/Unready toggles; one cue per actual change.',
        'select':'Camera choice, Round/Cup/Crew, skill cards, rod choice and tutorial pages. No tick for pointer hover.',
        'error':'Rejected action, invalid target, full inventory, occupied buggy or unavailable gear. Rate-limited; no continuous disabled-button alarm.',
        'open':'Settings, guide, scoreboard, skill picker and exit dialog opening.',
        'close':'Closing panels. Sliders remain silent during drag; decorative/disabled elements have no click.'}
    for cue,note in ui_notes.items():
        p.reuse('basketball','ui-'+cue,'ui-'+cue,'Button · '+cue.title(),ui,note)
    p.reuse('basketball','air','air','Shared coastal air',amb,'Same quiet breeze already approved for both sports. Use once per island listener; no stacked duplicate wind beds.',True)
    waves=loop(p.source('waves',.0,21.76,90,6000),.45)
    p.new('shore-waves','Gentle shoreline / lagoon wash',amb,[waves],'Quiet ambient layer near shore. Duck for bite warnings, results and important contacts. Do not put a loud wave behind every cast.', ['waves'],'Full public preview trimmed .00–21.76; mono; 90 Hz high-pass / 6 kHz low-pass; 450 ms overlap and loop seam repair.',.10,True)

    # Two illustrative montages. Each has explicit time jumps; no gameplay claim.
    reels=[]
    def scene(key,title,duration,spec):
        events=[];timeline=[]
        for t,cue,variant,gain in spec:
            events.append((t,p.clip(cue,variant),gain))
            timeline.append(dict(time=t,cue=cue,title=next(i['title'] for i in p.items if i['id']==cue),variation=variant))
        x=fade(place(duration,events),.01,.10);gain=min(1,.78/max(abs(x).max(),1e-9));x*=gain
        path=root/'Reels'/f'{key}.wav'
        with wave.open(str(path),'wb') as w:
            w.setparams((1,2,SR,0,'NONE','not compressed'));w.writeframes((x*32767).round().astype('<i2').tobytes())
        subprocess.run([FF,'-v','error','-y','-i',str(path),'-c:a','libmp3lame','-b:a','160k',str(path.with_suffix('.mp3'))],check=True)
        reels.append(dict(id=key,title=title,file=f'Reels/{key}.mp3',duration=duration,timeline=timeline,mix_gain=float(gain),scene=True,
            note='Illustrative edit with time jumps. Not gameplay capture or synchronization evidence.'))
    scene('golf-scene','Golf · a short round',23,[
        (.2,'golf-countdown',1,.7),(.75,'golf-countdown',1,.7),(1.3,'golf-go',1,.65),
        (2.5,'golf-walkway',1,.6),(3.0,'golf-walkway',2,.6),(3.55,'ui-select',1,.7),
        (4,'golf-charge',1,1),(6.5,'golf-swish',1,.8),(6.72,'golf-swing',1,1),
        (8.5,'golf-turf',1,1),(9,'golf-roll',1,.8),(11.3,'golf-putt',1,1),
        (11.6,'golf-roll',1,.8),(13.0,'golf-cup',1,1),(14.7,'golf-hole',1,1),
        (16,'cart-summon',1,.6),(16.6,'cart-seat',1,1),(17.2,'cart-motor',1,1),
        (17.2,'cart-tyres',1,.8),(19.2,'cart-brake',1,1),(20.5,'golf-finish',1,1)])
    scene('fishing-scene','Fishing · cast to catch',24,[
        (0,'shore-waves',1,.45),(.3,'wood-steps',1,.6),(.8,'wood-steps',2,.6),
        (1.5,'fish-cast',1,1),(2.15,'float-splash',1,1),(4.0,'fish-bite',1,1),
        (4.65,'fish-hook',1,1),(5.0,'fish-reel',1,1),(7.3,'fish-switch',2,1),
        (7.5,'fish-drag',1,1),(8.0,'fish-surge',2,.8),(9.7,'fish-warning',1,1),
        (11.2,'fish-cooled',1,1),(11.65,'power-reel',1,1),(12.1,'fish-reel',1,1),
        (14.8,'fish-land',1,1),(15.6,'catch-score',1,.7),(16.2,'wanted-bonus',1,1),
        (18,'treasure-start',1,1),(18.5,'fish-reel',1,.75),(20.9,'treasure-rise',1,1),
        (21.7,'gear-pickup',1,.7),(22.2,'sonar',1,1)])
    for cat,key,title in [(g,'golf-contacts','Golf contacts'),(cart,'buggy','Buggy cues'),
                          (f,'fishing-actions','Fishing actions'),(gear,'skills','Six skills & treasure'),
                          (move,'surfaces','Footsteps & surfaces'),(match,'rewards','Match & rewards'),(ui,'buttons','Buttons')]:
        cues=[(i['id'],1) for i in p.items if i['category']==cat and not i.get('loop')]
        reels.append(p.reel(key,title,cues,.35))

    # Sample-contiguous three-cycle auditions avoid HTML ended/restart gaps.
    (root/'LoopAuditions').mkdir(exist_ok=True)
    for item in p.items:
        if not item.get('loop'):continue
        for v in item['variants']:
            original=root/v['file'];dest=root/'LoopAuditions'/original.name
            with wave.open(str(original),'rb') as w:
                params=w.getparams();frames=w.readframes(w.getnframes())
            with wave.open(str(dest),'wb') as w:
                w.setparams(params);w.writeframes(frames*3)
            v['preview_file']=dest.relative_to(root).as_posix()
            v['preview_cycles']=3
    data=dict(title='Golf & Fishing sound preview',version=1,generated_at_utc=datetime.now(timezone.utc).isoformat(),
        status='Awaiting approval; new candidates are not integrated. Existing game sounds remain unchanged.',
        base='All file paths relative to this manifest directory',sample_rate=SR,items=p.items,reels=reels,sources=p.sources['sources'])
    report=dict(clip_count=len(p.metrics),event_families=len(p.items),wav_bytes=sum(m['bytes'] for m in p.metrics),
        total_seconds=round(sum(m['duration'] for m in p.metrics),3),reused_clips=sum('reuse_asset' in m for m in p.metrics),
        shared_sports_clips=sum(m.get('bank') in ('basketball','football') for m in p.metrics),
        retained_golf_recordings=sum(m.get('bank')=='golf' for m in p.metrics),
        auditory_review='Technical signal checks only; user listening approval pending',clips=p.metrics)
    (root/'manifest.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    (root/'Evidence/clip-qa.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    template=Path(__file__).with_name('golf_fishing_preview.html').read_text(encoding='utf-8')
    (root/'index.html').write_text(template.replace('__AUDIO_MANIFEST__',json.dumps(data,ensure_ascii=False).replace('</','<\\/')),encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='clips'},indent=2))


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output',type=Path,default=ROOT/'Builds/GolfFishingAudioPreview')
    build(parser.parse_args().output)

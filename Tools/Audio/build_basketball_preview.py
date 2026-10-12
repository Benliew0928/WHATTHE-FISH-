"""Build a local, pre-integration basketball listening pack.

Requires numpy and imageio-ffmpeg. Run the candidate downloader first.
All paths in generated records are relative to the output directory.
Original procedural cues use deterministic synthesis, not an AI model.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import subprocess
import wave

import imageio_ffmpeg
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
SR = 32000
FF = imageio_ffmpeg.get_ffmpeg_exe()
RNG = np.random.default_rng(1102026)


def timebase(duration):
    return np.arange(round(duration * SR), dtype=np.float64) / SR


def fade(a, attack=.003, release=.028):
    a = np.asarray(a, dtype=np.float64).copy()
    n = min(round(attack * SR), len(a) // 2)
    m = min(round(release * SR), len(a) // 2)
    if n: a[:n] *= np.linspace(0, 1, n)**1.5
    if m: a[-m:] *= np.linspace(1, 0, m)**1.5
    return a


def filtered(a, low=80, high=10000):
    # Padded frequency-domain filter, smooth knees. Used for offline authoring.
    n = 1 << (len(a) + 1023).bit_length()
    f = np.fft.rfftfreq(n, 1 / SR)
    hp = (f / np.maximum(f + low, 1))**2 if low else np.ones_like(f)
    lp = 1 / (1 + (f / high)**8)
    return np.fft.irfft(np.fft.rfft(a, n) * hp * lp, n)[:len(a)]


def noise(duration, low=200, high=7000):
    a = filtered(RNG.normal(0, 1, len(timebase(duration))), low, high)
    return a / max(np.std(a), 1e-8)


def tone(duration, start, end=None, decay=8, attack=.004):
    t = timebase(duration)
    end = start if end is None else end
    phase = 2 * np.pi * (start*t + (end-start)*t*t/(2*duration))
    return fade(np.sin(phase) * np.exp(-t*decay), attack, .025)


def place(duration, events):
    out = np.zeros(round(duration*SR))
    for when, clip, gain in events:
        offset = round(when*SR)
        count = min(len(clip), len(out)-offset)
        if count > 0: out[offset:offset+count] += clip[:count] * gain
    return out


def chime(notes, duration=.5):
    events = []
    for i, freq in enumerate(notes):
        d = max(.08, duration-i*.075)
        x = .6*tone(d, freq, decay=14) + .13*tone(d, freq*2.01, decay=24)
        events.append((i*.075, x, 1))
    return place(duration, events)


def whoosh(duration=.35, upward=True):
    t = timebase(duration)
    env = np.sin(np.pi*t/duration)**1.7
    f1, f2 = ((420, 1500) if upward else (1500, 360))
    phase = 2*np.pi*(f1*t+(f2-f1)*t*t/(2*duration))
    x = .16*np.sin(phase) + .50*noise(duration, 450, 6500)
    return fade(x*env*(.85+.15*np.sin(2*np.pi*23*t)), .008, .04)


def palm(duration=.19, weight=1):
    t = timebase(duration)
    x = .6*np.sin(2*np.pi*(150*t-100*t*t))*np.exp(-t*55)
    x += .22*noise(duration, 600, 6500)*np.exp(-t*90)
    x += .10*noise(duration, 150, 1900)*np.exp(-t*25)
    return fade(x*weight, .001, .04)


def squeal(duration=.24, base=950):
    t = timebase(duration)
    # Friction-inspired synthetic alternative, explicitly labelled as such.
    f = base+base*.27*np.sin(np.pi*t/duration)+38*np.sin(2*np.pi*43*t)
    phase = 2*np.pi*np.cumsum(f)/SR
    env = np.sin(np.pi*t/duration)**.9
    harmonic = np.sin(phase)+.35*np.sin(2*phase)+.12*np.sin(3*phase)
    return fade((.22*harmonic+.075*noise(duration, 1200, 8500))*env, .006, .045)


def loop_air(duration=2, ocean=False):
    n = round(duration*SR)
    f = np.fft.rfftfreq(n, 1/SR)
    spectrum = RNG.normal(size=len(f))+1j*RNG.normal(size=len(f))
    spectrum *= np.where(f>30, 1/np.maximum(f, 30)**.6, 0)
    spectrum *= 1/(1+(f/(2100 if ocean else 4300))**6)
    spectrum[0] = 0
    x = np.fft.irfft(spectrum, n)
    t = np.arange(n)/n
    x *= .6+.22*np.sin(2*np.pi*t)+.1*np.sin(2*np.pi*3*t+.4)
    return x/max(abs(x).max(), 1e-9)


class Pack:
    def __init__(self, root):
        self.root = root.resolve()
        self.root.mkdir(parents=True, exist_ok=True)
        self.sources = json.loads((self.root/'sources.json').read_text(encoding='utf-8'))
        self.source_by_id = {s['id']: s for s in self.sources['sources']}
        for record in self.sources['sources']:
            source_path = (self.root / record['file']).resolve()
            if not source_path.is_relative_to(self.root):
                raise ValueError('Source path escapes preview directory')
            if hashlib.sha256(source_path.read_bytes()).hexdigest() != record['sha256']:
                raise ValueError(f"Source integrity mismatch: {record['id']}")
        self.cache = {}
        self.items = []
        self.samples = {}
        self.metrics = []
        for folder in ['Clips', 'Reels', 'Evidence']:
            (self.root/folder).mkdir(exist_ok=True)

    def read(self, filename):
        filename = str(filename)
        if filename not in self.cache:
            data = subprocess.check_output([FF, '-v', 'error', '-i', str(self.root/filename),
                '-ac', '1', '-ar', str(SR), '-f', 'f32le', 'pipe:1'])
            self.cache[filename] = np.frombuffer(data, dtype='<f4').astype(np.float64)
        return self.cache[filename].copy()

    def source(self, source_id, start=0, end=None, hp=65, lp=10000, tail=None):
        record = self.source_by_id[source_id]
        x = self.read(record['file'])
        end = len(x)/SR if end is None else end
        x = x[round(start*SR):round(end*SR)]
        if not len(x): raise ValueError(f'Empty source trim {source_id} {start} {end}')
        x = filtered(x-np.mean(x), hp, lp)
        if tail:
            t = timebase(len(x)/SR)
            x *= np.exp(-np.maximum(0, t-.11)/tail)
        return fade(x)

    def kenney(self, pack, filename):
        candidates = list((self.root/'Raw'/f'kenney-{pack}').rglob(filename))
        if len(candidates) != 1: raise ValueError(f'Expected one {filename}')
        rel = candidates[0].relative_to(self.root).as_posix()
        return fade(self.read(rel)), rel

    def add(self, cue_id, title, category, clips, origin, note, source_ids=None, processing=None,
            level=.38, alternative=False, loop=False):
        variants = []
        for index, clip in enumerate(clips):
            x = np.asarray(clip, dtype=np.float64)
            if not np.isfinite(x).all() or len(x)<32: raise ValueError(f'Invalid {cue_id}')
            peak = max(abs(x).max(), 1e-9)
            x = x / peak * level
            if not loop: x=fade(x, .0015, .018)
            name = f'{cue_id}-{index+1:02d}'
            wav = self.root/'Clips'/f'{name}.wav'
            with wave.open(str(wav), 'wb') as w:
                w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
                w.writeframes((x*32767).round().astype('<i2').tobytes())
            self.samples[name] = x
            metric=dict(id=name, duration=round(len(x)/SR,4), peak_dbfs=round(float(20*np.log10(abs(x).max()+1e-12)),2),
                        rms_dbfs=round(float(20*np.log10(np.sqrt(np.mean(x*x))+1e-12)),2),
                        clipped_samples=int(np.sum(abs(x)>=1)), dc=float(np.mean(x)),
                        endpoint_jump=float(abs(x[-1]-x[0])), bytes=wav.stat().st_size,
                        sha256=hashlib.sha256(wav.read_bytes()).hexdigest())
            self.metrics.append(metric)
            variants.append(dict(file=wav.relative_to(self.root).as_posix(), duration=metric['duration'],
                                 waveform=[round(float(v),4) for v in np.max(np.abs(np.pad(x,(0,(-len(x))%80))).reshape(80,-1),axis=1)]))
        item=dict(id=cue_id,title=title,category=category,origin=origin,note=note,
                  sources=source_ids or [],processing=processing or 'Original deterministic procedural synthesis; no AI service',
                  variants=variants,alternative=alternative,loop=loop)
        self.items.append(item)

    def clip(self, cue_id, variation=1):
        return self.samples[f'{cue_id}-{variation:02d}']

    def reel(self, name, title, cues, gaps=.5):
        length = .3
        events=[]; timeline=[]
        for cue, variant in cues:
            x=self.clip(cue,variant)
            events.append((length,x,1))
            item=next(i for i in self.items if i['id']==cue)
            timeline.append(dict(time=round(length,3),title=item['title'],cue=cue,variation=variant))
            length += len(x)/SR+gaps
        x=place(length,events)
        path=self.root/'Reels'/f'{name}.wav'
        with wave.open(str(path),'wb') as w:
            w.setnchannels(1);w.setsampwidth(2);w.setframerate(SR);w.writeframes((x*32767).round().astype('<i2').tobytes())
        mp3=path.with_suffix('.mp3')
        subprocess.run([FF,'-v','error','-y','-i',str(path),'-c:a','libmp3lame','-b:a','128k',str(mp3)],check=True)
        return dict(id=name,title=title,file=mp3.relative_to(self.root).as_posix(),duration=round(length,2),timeline=timeline)


def build(output):
    p=Pack(output)
    # Isolated transients selected from waveform evidence, not an auditory review.
    bounce_trims=[(33.29,33.81),(34.84,35.37),(36.35,36.95),(38.02,38.65)]
    bounces=[p.source('dribble',a,b,tail=.20) for a,b in bounce_trims]
    p.add('ball-bounce','Court bounce','Ball & basket',bounces,'Recorded CC0',
          'First choice for dribbles, bounce passes and rebounds. Shortened gym tails; listen for residual echo.',
          ['dribble'],f'Trims in seconds: {bounce_trims}; mono 32 kHz; 65 Hz high-pass, 10 kHz low-pass; tail damping, fades, peak normalization',level=.48)
    p.add('ball-soft','Soft bounce','Ball & basket',[b*.65 for b in bounces[:2]],'Edited recording',
          'Quieter version of the same ball for low-energy contacts.', ['dribble'],
          'Same source trims as bounce 1/2; reduced playback level',level=.22)
    p.add('ball-hard','Hard bounce / bounce pass','Ball & basket',[bounces[2],bounces[3]],'Edited recording',
          'Stronger version of the same ball; final game strength should follow the collision.', ['dribble'],
          'Same source trims as bounce 3/4; higher playback level',level=.60)
    p.add('board','Backboard strike','Ball & basket',[p.source('backboard',.012,.53)],'Recorded CC0',
          'Dedicated backboard recording, kept distinct from the metallic rim.', ['backboard'],
          'Trim 0.012-0.530 s; mono 32 kHz, 65 Hz high-pass, fades, peak normalization',level=.47)
    rim_trims=[(.13,.89),(1.60,2.41),(3.12,3.95)]
    rims=[p.source('rim',a,b,tail=.35) for a,b in rim_trims]
    p.add('rim','Rim strike','Ball & basket',rims,'Recorded CC0',
          'Three separate actual rim hits. Check the metallic ring in headphones.', ['rim'],
          f'Trims: {rim_trims}; mono 32 kHz, 65 Hz high-pass, shortened tail, fades, peak normalization',level=.48)
    swish=p.source('swish',.10,.44,hp=180,lp=11000)
    p.add('net','Net swish','Ball & basket',[swish],'Recorded CC0',
          'Direct standalone swish candidate. Please confirm its nylon-net character.', ['swish'],
          'Trim 0.100-0.440 s; mono 32 kHz, 180 Hz high-pass, fades, peak normalization',level=.40)
    p.add('net-brush','Light net brush','Ball & basket',[filtered(swish,450,6500)],'Edited recording',
          'Softer net texture for side contact without a score.', ['swish'],
          'Derived from selected swish; filtered and quieter',level=.14)
    p.add('dunk','Dunk contact','Ball & basket',[place(.7,[(0,rims[0],.7),(.13,swish,.8)])],'Layered recording',
          'Preview of rim plus net. In the game these layers should follow separate contacts.', ['rim','swish'],
          'Selected rim 1 plus selected swish at +0.13 s; normalized',level=.54)
    p.add('catch','Catch / ball pickup','Ball & basket',[palm(.21),palm(.18)],'Generated',
          'Original palm-and-rubber approximation; replace if you prefer a recorded catch.',level=.36)
    p.add('hand','Dribble hand contact','Ball & basket',[palm(.12)],'Generated',
          'Small, quiet slap for the hand returning to the ball.',level=.13)
    p.add('release','Pass / shot release','Ball & basket',[whoosh(.18)*.35+palm(.18)*.2],'Generated',
          'Light cloth and hand movement. Should remain quieter than ball impacts.',level=.16)
    p.add('block','Block / successful steal','Ball & basket',[palm(.16)+.25*tone(.16,210,110,30)],'Generated',
          'Shorter, firmer contact for an actual deflection; not for a missed swipe.',level=.41)
    p.add('score','Basket confirmed','Ball & basket',[chime([784,1046],.40)],'Generated',
          'Friendly score confirmation, kept separate from the real net sound.',level=.22)
    p.add('roll','Ball rolling texture','Ball & basket',[loop_air(1.5)*(.65+.35*np.cos(2*np.pi*timebase(1.5)*4))],'Generated',
          'Optional quiet rubber texture. Low-priority polish, not a constant dribble layer.',level=.055,loop=True)

    walk_trims=[(1.64,2.04),(2.70,3.14),(3.68,4.13),(4.68,5.09)]
    walks=[p.source('sneakers',a,b,hp=90,lp=8000,tail=.16) for a,b in walk_trims]
    p.add('walk','Sneakers walking on wood','Shoes & body',walks,'Recorded CC0',
          'Actual sneakers on wood; four alternating contacts. Quiet source raised to the preview level.', ['sneakers'],
          f'Trims: {walk_trims}; mono 32 kHz, high-pass 90 Hz, low-pass 8 kHz, fades, peak normalization',level=.23)
    steps=[];step_files=[]
    for i in range(4):
        x,file=p.kenney('impact-sounds',f'footstep_wood_{i:03}.ogg');steps.append(x);step_files.append(file)
    p.add('wood-alt','Wood footstep alternative','Shoes & body',steps,'Recorded CC0',
          'Kenney original pack alternative. Compare its weight with the sneaker recording.', ['impact-sounds'],
          'Original pack files '+', '.join(step_files)+'; mono conversion, fades, normalization',level=.23,alternative=True)
    running=[]
    for i,x in enumerate(walks):
        x=np.interp(np.arange(0,len(x),1.10),np.arange(len(x)),x)
        running.append(place(max(len(x)/SR,.28),[(0,x,1),(.012,palm(.14),.035)]))
    p.add('run','Running / sprinting footstep','Shoes & body',running,'Layered / generated',
          'Designed from real sneaker steps with a firmer attack; not a separate sprint recording.', ['sneakers'],
          'Selected walk takes at 1.10x playback rate plus low original palm transient',level=.32)
    qtrims=[(2.17,2.56),(3.21,3.60),(4.26,4.63)]
    sq=[p.source('squeak',a,b,hp=400,lp=11000) for a,b in qtrims]
    p.add('squeak-real','Recorded shoe squeak','Shoes & body',sq,'Recorded CC0',
          'Walking-shoe recording. Its suitability for an athletic pivot needs your listening choice.', ['squeak'],
          f'Trims: {qtrims}; mono 32 kHz, high-pass 400 Hz, fades, normalization',level=.24)
    p.add('squeak-designed','Sharp pivot squeak alternative','Shoes & body',[squeal(.20,1050),squeal(.26,860),squeal(.17,1250)],
          'Generated','Friction-inspired chirps for a sharper cartoon sports feel; compare with the recorded option.',level=.23,alternative=True)
    p.add('scuff','Defensive shuffle / sole drag','Shoes & body',
          [noise(.28,350,4800)*np.sin(np.pi*timebase(.28)/.28)**1.4 for _ in range(2)],'Generated',
          'Low sole friction; layer under occasional squeaks during a lateral plant.',level=.11)
    p.add('land','Jump / dunk landing','Shoes & body',
          [place(.65,[(0,walks[0],1),(.028,walks[2],.8),(.009,palm(.24),.025)])],'Layered / generated',
          'Two-foot landing using recorded soles and a soft original body transient.', ['sneakers'],
          'Two walk contacts offset by 28 ms with original low transient; normalization',level=.35)
    p.add('cloth','Gather / jersey movement','Shoes & body',
          [noise(.25,280,4200)*np.sin(np.pi*timebase(.25)/.25)**1.5],'Generated',
          'Very restrained cloth-like rustle for gathers and cancellation.',level=.065)
    p.add('swipe','Missed steal / reach','Shoes & body',[whoosh(.20)],'Generated',
          'Quiet arm movement without the successful ball slap.',level=.12)
    p.add('body','Padded / body impact','Shoes & body',[filtered(palm(.23),40,1400)],'Generated',
          'Dull contact for padding or a body reaction.',level=.22)

    p.add('countdown','Prop countdown tick','Props & court',[tone(.13,610,570,30)],'Generated',
          'One quiet tick per countdown second, not a repeating alarm.',level=.17)
    p.add('spawn','Prop materializes','Props & court',[chime([523,784,1046],.55)+.13*whoosh(.55)],'Generated',
          'A short holographic shimmer at the actual spawn.',level=.26)
    p.add('pickup','Prop collected','Props & court',[chime([659,880,1318],.40)],'Generated',
          'Compact rising pickup chime, distinct from the basket confirmation.',level=.27)
    t=timebase(2)
    focus=(.32*np.sin(2*np.pi*(220*t+35*t*t))+.07*np.sin(2*np.pi*(440*t+70*t*t)))
    focus*=np.minimum(t/.18,1)*(.42+.58*(t/2))
    focus+=place(2,[(0,chime([462],.12),.08),(.5,chime([518],.12),.08),(1,chime([581],.12),.08),(1.5,chime([651],.12),.08)])
    p.add('lock-aim','Lock-on: two-second aim','Props & court',[fade(focus,.04,.09)],'Generated',
          'Softer revision: lower pitch and 8 dB lower peak. Exactly two seconds; stops if interrupted.',level=.064)
    p.add('lock-fire','Lock-on: release','Props & court',[chime([1046,1568],.25)+whoosh(.25)*.15],'Generated',
          'Release accent; does not announce a made basket.',level=.23)
    p.add('interrupt','Ability interrupted','Props & court',[place(.32,[(0,tone(.18,550,350,15),1),(.12,tone(.2,330,220,18),.5)])],'Generated',
          'A descending broken tone for a cancelled lock-on or failed transfer.',level=.20)
    p.add('turbo-start','Team Turbo: activate','Props & court',[whoosh(.48)+.2*chime([440,660,880],.48)],'Generated',
          'Airy acceleration burst for the collector and affected teammates.',level=.30)
    p.add('turbo-loop','Team Turbo: quiet active texture','Props & court',[loop_air(2)],'Generated',
          'Optional loop, intentionally much quieter than footsteps. The effect still lasts five seconds.',level=.055,loop=True)
    p.add('turbo-end','Team Turbo: expire','Props & court',[whoosh(.30,False)+.18*tone(.30,440,220,10)],'Generated',
          'Short downward air tail at expiry.',level=.17)
    p.add('portal-open','Portal Steal: open','Props & court',[whoosh(.42)+.25*tone(.42,210,460,3)],'Generated',
          'Rounded opening; play spatially at both portal positions.',level=.30)
    p.add('portal-in','Portal Steal: suction','Props & court',[whoosh(.25,False)[::-1]+.14*tone(.25,260,100,3)],'Generated',
          'Brief intake as the ball shrinks into the source portal.',level=.24)
    p.add('portal-out','Portal Steal: emerge','Props & court',[.65*palm(.28)+.25*chime([520,780],.28)],'Generated',
          'Rounded arrival pop; the receiver catch remains a separate layer.',level=.28)
    p.add('portal-close','Portal Steal: close','Props & court',[whoosh(.22,False)*.4+tone(.22,380,140,12)*.25],'Generated',
          'Short closure or softened abort tail.',level=.15)
    p.add('boundary','Energy boundary ripple','Props & court',[tone(.30,360,220,12)+.08*whoosh(.30)],'Generated',
          'Subtle energy contact. The ball passes through the wall, so there is no hard impact.',level=.12)
    p.add('inbound','Ball recovered / inbound ready','Props & court',[chime([440,587],.34)],'Generated',
          'Gentle status notice, once when recovery completes.',level=.19)
    p.add('air','Coastal air bed','Props & court',[loop_air(12,True)],'Generated',
          'Optional subtle looping air/surf approximation; no crowd, whistles or unrelated dribbles.',level=.042,loop=True)

    ui=[('click','Button click','click_003.ogg'),('confirm','Confirm','confirmation_001.ogg'),
        ('back','Back / cancel','back_001.ogg'),('toggle','Toggle','switch_002.ogg'),
        ('select','Selection / gesture mode','select_001.ogg'),('error','Unavailable action','error_002.ogg'),
        ('open','Open panel','open_001.ogg'),('close','Close panel','close_001.ogg')]
    for key,title,file in ui:
        x,rel=p.kenney('interface-sounds',file)
        p.add('ui-'+key,title,'Buttons', [x],'Recorded CC0',
              'Kenney original interface pack. One cue per accepted action; no repeating clicks while holding.',
              ['interface-sounds'],f'{rel}; mono 32 kHz, fades and preview gain',level=.15 if key!='error' else .13)
    t=timebase(.06)
    cove=np.sin(2*np.pi*(620*t-900*t*t))*np.sin(np.pi*t/.06)*np.exp(-50*t)
    p.add('ui-soft','Existing menu-style tap alternative','Buttons',[cove],'Generated',
          'Same synthesis formula as the current Cove menu tap, rendered for comparison.',
          processing='Existing original CoveSound formula resampled at 32 kHz; preview gain',level=.14,alternative=True)

    reels=[p.reel('court-sounds','Ball, basket and shoes',[(key,1) for key in
             ['ball-bounce','ball-soft','ball-hard','board','rim','net','catch','release','block','score','walk','run','squeak-real','squeak-designed','scuff','land']],.60),
           p.reel('prop-sounds','Props and court feedback',[(key,1) for key in
             ['countdown','spawn','pickup','lock-aim','lock-fire','interrupt','turbo-start','turbo-end','portal-open','portal-in','portal-out','portal-close','boundary','inbound']],.6),
           p.reel('button-sounds','Button palette',[(i['id'],1) for i in p.items if i['category']=='Buttons'],.6)]

    # A clearly labelled illustrative edit, not captured gameplay or a runtime test.
    scene_events=[];timeline=[]
    def event(t,key,v=1,gain=1):
        scene_events.append((t,p.clip(key,v),gain));timeline.append(dict(time=t,cue=key,variation=v))
    for i in range(5):
        event(.4+i*.54,'run',i%4+1,.65)
        event(.52+i*.54,'ball-bounce',i%4+1,.85)
    event(2.96,'squeak-real',1,.6);event(3.3,'release');event(4.0,'board');event(4.2,'rim',2,.7)
    event(4.43,'net');event(4.57,'score');event(4.88,'ball-bounce',2,.7);event(5.4,'catch')
    event(6.2,'pickup');event(7.0,'lock-aim');event(9.0,'lock-fire');event(9.8,'net');event(10.02,'score')
    event(11.2,'portal-open');event(11.40,'portal-in');event(11.90,'portal-out');event(12.02,'catch',1,.6);event(12.17,'portal-close')
    scene=fade(place(13.2,scene_events),.01,.10)
    peak=abs(scene).max()
    if peak>.8:scene*=.8/peak
    scene_path=p.root/'Reels/court-scene.wav'
    with wave.open(str(scene_path),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(SR);w.writeframes((scene*32767).round().astype('<i2').tobytes())
    subprocess.run([FF,'-v','error','-y','-i',str(scene_path),'-c:a','libmp3lame','-b:a','160k',str(scene_path.with_suffix('.mp3'))],check=True)
    reels.insert(0,dict(id='court-scene',title='Illustrative court sequence',file='Reels/court-scene.mp3',duration=13.2,timeline=sorted(timeline,key=lambda x:x['time'])))
    data=dict(title='Basketball sound preview',version=1,generated_at_utc=datetime.now(timezone.utc).isoformat(),
              status='Pre-integration audition candidates. Technical checks only; not heard by the assistant.',
              base='All file paths relative to this manifest directory',sample_rate=SR,items=p.items,reels=reels,
              sources=p.sources['sources'])
    (p.root/'manifest.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    report=dict(clip_count=len(p.metrics),event_families=len(p.items),wav_bytes=sum(m['bytes'] for m in p.metrics),
                total_seconds=round(sum(m['duration'] for m in p.metrics),3),
                auditory_review='Not available in this tool session; user audition required',clips=p.metrics)
    (p.root/'Evidence/clip-qa.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    template=(Path(__file__).with_name('basketball_preview.html')).read_text(encoding='utf-8')
    payload=json.dumps(data,ensure_ascii=False).replace('</','<\\/')
    (p.root/'index.html').write_text(template.replace('__AUDIO_MANIFEST__',payload),encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='clips'},indent=2))


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output',type=Path,default=ROOT/'Builds/BasketballAudioPreview')
    build(parser.parse_args().output)

"""Original 48 kHz stereo sound design; seamless eight-bar loops, no external samples."""
import math, wave, json, shutil, sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[1]; OUT=ROOT/'assets/audio/unity'; OUT.mkdir(parents=True,exist_ok=True)
SR=48000; rng=np.random.default_rng(732); report={}
def stereo(x,pan=0):
 return np.column_stack((x*np.sqrt((1-pan)/2),x*np.sqrt((1+pan)/2)))
def tone(t,f): return np.sin(2*np.pi*f*t)+.16*np.sin(2*np.pi*f*2*t)
def bell(t,f): return tone(t,f)*np.exp(-7*t)*(1-np.exp(-t*240))
def save(name,x,peak):
 if x.ndim==1:x=stereo(x)
 x=np.tanh(x); x*=peak/max(np.max(np.abs(x)),1e-8)
 pcm=np.round(x*32767).astype('<i2')
 with wave.open(str(OUT/f'mektra_{name}.wav'),'wb') as w:
  w.setparams((2,2,SR,0,'NONE','not compressed'));w.writeframes(pcm.tobytes())
 report[name]={'seconds':len(x)/SR,'peak_dbfs':round(20*np.log10(np.max(np.abs(x))),2),'rms_dbfs':round(20*np.log10(np.sqrt(np.mean(x*x))),2),'boundary_delta':float(np.max(np.abs(x[0]-x[-1])))}
 shutil.copy2(OUT/f'mektra_{name}.wav',ROOT/'Unity/Assets/Resources/Audio'/f'mektra_{name}.wav')
def effect(name,duration,fn,peak=.65):
 t=np.arange(round(duration*SR))/SR;x=fn(t);env=np.minimum(t/.008,1)*np.minimum((duration-t)/.06,1)
 save(name,x*env[:,None] if x.ndim==2 else x*env,peak)
def whoosh(t):
 # A fast rod/whip pass: low air pressure, accelerating swish, restrained tip snap.
 noise=rng.normal(0,1,len(t))
 low=np.convolve(noise,np.ones(96)/96,mode='same')
 mid=np.convolve(noise,np.ones(14)/14,mode='same')-low
 tip=np.convolve(noise,np.ones(4)/4,mode='same')-np.convolve(noise,np.ones(20)/20,mode='same')
 sweep=np.exp(-.5*((t-.045)/.023)**2)
 pressure=np.sin(2*np.pi*(105*t+70*(1-np.exp(-24*t))/24))*np.exp(-21*t)*(1-np.exp(-t*500))
 snap=np.exp(-.5*((t-.072)/.006)**2)
 body=(low*1.9+mid*.65)*sweep+pressure*.10+tip*.09*snap
 return stereo(body,-.04)
effect('jump',.20,whoosh,.62)
if '--jump-only' in sys.argv:
 existing=json.loads((OUT/'quality-report.json').read_text());existing.update(report)
 (OUT/'quality-report.json').write_text(json.dumps(existing,indent=2)+'\n')
 print('Updated jump: 200 ms bass-led whip swish, short tip snap, stereo PCM with headroom.')
 sys.exit(0)
def stars(t):
 out=np.zeros((len(t),2))
 for delay,f,pan in [(0,587.33,-.22),(.045,880.0,.22),(.095,1174.66,0)]:
  u=np.maximum(0,t-delay);out+=stereo(bell(u,f)*.18*(t>=delay),pan)
 # Quiet stereo delay gives sparkle without piercing high frequencies.
 for delay,gain in [(.15,.18),(.23,.09)]:
  n=round(delay*SR);out[n:]+=out[:-n]*gain
 return out
effect('star',.66,stars,.58)
effect('land',.16,lambda t:.3*np.sin(2*np.pi*(115*t-110*t*t))*np.exp(-30*t),.28)
effect('hello',.52,lambda t:bell(t,293.665)*.22+bell(np.maximum(0,t-.09),440.0)*.16*(t>=.09),.42)
def crash(t):
 noise=np.convolve(rng.normal(0,1,len(t)),np.ones(18)/18,mode='same')
 phase=2*np.pi*(95*t+55*(1-np.exp(-t*10))/10)
 return .48*np.sin(phase)*np.exp(-8*t)+noise*.65*np.exp(-12*t)+.06*np.sin(2*np.pi*220*t)*np.exp(-5*t)
effect('crash',.95,crash,.72)
def music(name,menu=False):
 bpm=96;beat=60/bpm;duration=32*beat;N=round(duration*SR);out=np.zeros((N,2));chords=[(146.832,174.614,220),(130.813,164.814,195.998),(116.541,146.832,174.614),(130.813,174.614,195.998)]
 def add(x,start,pan=0):
  # Circular placement preserves delay/reverb tails through the loop boundary.
  if x.ndim==1:x=stereo(x,pan)
  offset=round(start*SR)%N; end=min(len(x),N-offset);out[offset:offset+end]+=x[:end]
  if end<len(x):out[:len(x)-end]+=x[end:]
 for bar in range(8):
  chord=chords[(bar//2)%4];t=np.arange(round(4*beat*SR))/SR;env=np.sin(np.pi*t/(4*beat))**2
  for f in chord:
   pad=.032*(np.sin(2*np.pi*f*t)+.3*np.sin(2*np.pi*f*2*t))*env
   add(pad,bar*4*beat,(-.35 if f==chord[0] else .35))
  for b in range(4):
   start=(bar*4+b)*beat;t=np.arange(round(.35*SR))/SR
   if not menu or b in (0,2):
    kick=.23*np.sin(2*np.pi*(48*t+55*(1-np.exp(-24*t))/24))*np.exp(-14*t)*(1-np.exp(-t*800));add(kick,start)
   if b in (1,3):
    noise=np.convolve(rng.normal(0,1,len(t)),np.ones(5)/5,mode='same');add(noise*.055*np.exp(-26*t)*(1-np.exp(-t*500)),start,.1)
   bt=np.arange(round(.48*SR))/SR;add(tone(bt,chord[0]/2)*np.exp(-7*bt)*(1-np.exp(-bt*300))*.11,start+.02)
  for step in range(8):
   start=(bar*4+step*.5)*beat;t=np.arange(round(.12*SR))/SR;noise=rng.normal(0,1,len(t));noise=noise-np.convolve(noise,np.ones(8)/8,mode='same');add(noise*.015*np.exp(-48*t)*(1-np.exp(-t*500)),start,(-.3 if step%2 else .3))
   if step%2==0 or not menu:
    f=chord[[0,1,2,1,0,2,1,2][step]]*(2 if menu else 4);t=np.arange(round(1.1*SR))/SR;mel=bell(t,f)*(.035 if menu else .045);add(mel,start,(-.18 if step%2 else .18));add(mel*.22,start+beat*.75,.25)
 # Ensure a continuous seam while retaining circular musical tails.
 length=256;delta=out[0]-out[-1];out[-length:]+=np.linspace(0,1,length)[:,None]*delta
 save(name,out,.50 if menu else .57)
music('music_game');music('music_menu',True)
assert all(v['peak_dbfs']<-2 for v in report.values())
assert all(report[n]['boundary_delta']<1e-8 for n in ['music_game','music_menu'])
(OUT/'quality-report.json').write_text(json.dumps(report,indent=2)+'\n')
print('Generated seven original stereo sounds. Music loops: 20 seconds, 96 BPM, matched harmony. Peak/seam checks passed.')

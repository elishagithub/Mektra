"""Original, short PCM sound effects: no downloaded or licensed samples."""
import math
import random
import shutil
import struct
import wave
from pathlib import Path

RATE = 44100
ROOT = Path(__file__).resolve().parent.parent / 'assets' / 'audio'
ROOT.mkdir(parents=True, exist_ok=True)
rng = random.Random(42)

def write(name, duration, sample):
    values = []
    for i in range(int(duration * RATE)):
        t = i / RATE
        fade = min(1, t / .004) * min(1, (duration - t) / .025)
        v = max(-.8, min(.8, sample(t) * fade))
        values.append(struct.pack('<h', round(v * 32767)))
    with wave.open(str(ROOT / f'{name}.wav'), 'wb') as out:
        out.setparams((1, 2, RATE, 0, 'NONE', 'not compressed'))
        out.writeframes(b''.join(values))

def bell(t, frequency, decay, volume):
    return volume * math.exp(-decay * t) * (
        math.sin(2 * math.pi * frequency * t)
        + .24 * math.sin(2 * math.pi * frequency * 2.003 * t)
        + .08 * math.sin(2 * math.pi * frequency * 3.01 * t))

def chime(t):
    return sum(bell(t-delay, f, 14, .18) for delay,f in [(0,1046.5),(.045,1318.5),(.09,1568)] if t >= delay)

write('mektra_star', .38, chime)
write('mektra_jump', .16, lambda t: .22 * math.sin(2*math.pi*(430*t+1600*t*t)) * math.exp(-17*t) + .035*rng.uniform(-1,1)*math.exp(-30*t))
write('mektra_land', .10, lambda t: .25*math.sin(2*math.pi*(180*t-350*t*t))*math.exp(-40*t) + .04*rng.uniform(-1,1)*math.exp(-70*t))
write('mektra_hello', .24, lambda t: bell(t,830,18,.17) + (bell(t-.075,1245,20,.15) if t>.075 else 0))
write('mektra_crash', .48, lambda t: .23*math.sin(2*math.pi*(260*t-210*t*t))*math.exp(-7*t) + .15*rng.uniform(-1,1)*math.exp(-15*t))
for destination in [ROOT.parent.parent / 'ios' / 'Mektra', ROOT.parent.parent / 'android' / 'app' / 'src' / 'main' / 'res' / 'raw']:
    destination.mkdir(parents=True, exist_ok=True)
    for clip in ROOT.glob('mektra_*.wav'):
        shutil.copy2(clip, destination / clip.name)
print('Generated and bundled five original, short arcade sounds.')

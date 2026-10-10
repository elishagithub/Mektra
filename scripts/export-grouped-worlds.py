from pathlib import Path
import re,json,math
r=Path(__file__).resolve().parents[1]; f=r/'figma';pack=json.loads((r/'assets/levels/endless/pack.json').read_text())
files=sorted((f/'world-boards').glob('*.svg'))+[f/'Mektra_hollow_passages.svg',f/'Mektra_hexagon_worlds.svg'];assert len(files)==16
out=['<svg xmlns="http://www.w3.org/2000/svg" width="3920" height="13100" viewBox="0 0 3920 13100">','<rect width="3920" height="13100" fill="#202632"/>','<text x="48" y="70" font-family="Arial" font-size="40" fill="#FFFFFF">MEKTRA / ALL WORLDS IN LEVEL ORDER</text>','<text x="48" y="120" font-family="Arial" font-size="22" fill="#BCC9DD">Four levels per world. Read left to right, then down. Orange rings show platforms that disappear.</text>']
for i,p in enumerate(files):
 s=re.sub(r'^<svg[^>]+>|</svg>$','',p.read_text())
 if i<14:
  for col,c in enumerate(pack['levels'][i*4:i*4+4]):
   rings=[]
   for plat in c['level']['platforms']:
    seconds=plat.get('crumbleSeconds',0)
    if not seconds:continue
    b=plat['bounds'];cx=(b['minX']+b['maxX'])/2;cy=(b['minY']+b['maxY'])/2;radius=max(b['maxX']-b['minX'],b['maxY']-b['minY'])/2+7
    rings.append(f'<circle id="crumble-ring-{plat["id"]}" cx="{cx}" cy="{cy}" r="{radius}" fill="none" stroke="#FF9F43" stroke-width="3"/>')
   # Add rings inside the level group, preserving exact layout size.
   marker=f'<g id="Mektra_level_{c["number"]}" transform="translate({48+445*col} 215)">'
   s=re.sub(re.escape(marker)+r'(<rect[^>]+/>)',lambda m:marker+m.group(1)+''.join(rings),s)
 name=f'World_{i+1:02d}__Levels_{i*4+1:02d}_{i*4+4:02d}'
 out.append(f'<g id="{name}" transform="translate({48+(i%2)*1960} {200+(i//2)*1550})">{s}</g>')
out.append('<g id="Crumble_countdown_states" transform="translate(48 12700)"><text x="0" y="30" font-size="30" font-family="Arial" fill="#FFFFFF">Disappearing platform / visual states</text>')
for i,label in enumerate(['Land: countdown starts','Ring counts down','Almost gone','Platform disappears']):
 x=200+i*850
 out.append(f'<text x="{x-120}" y="250" font-family="Arial" font-size="24" fill="#FFFFFF">{label}</text>')
 if i<3:
  out.append(f'<circle id="state-{i}-platform" cx="{x}" cy="135" r="55" fill="#05090E"/>')
  out.append(f'<circle id="state-{i}-ring" cx="{x}" cy="135" r="65" fill="none" stroke="#FF9F43" stroke-width="5" stroke-dasharray="{408*(1-i*.4)} 408" transform="rotate(-90 {x} 135)"/>')
 else:
  for j in range(8):
   a=j*math.pi/4;out.append(f'<circle cx="{x+80*math.cos(a)}" cy="{135+80*math.sin(a)}" r="3" fill="#FF9F43"/>')
out.append('</g></svg>');(f/'Mektra_all_worlds_grouped.svg').write_text(''.join(out));print('16 groups / 64 layouts + crumble visual states')

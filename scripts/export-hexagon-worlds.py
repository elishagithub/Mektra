"""Editable connected black hexagon concepts for Figma."""
from pathlib import Path
import math
root=Path(__file__).resolve().parents[1]
out=['<svg xmlns="http://www.w3.org/2000/svg" width="1860" height="1400" viewBox="0 0 1860 1400">','<rect width="1860" height="1400" fill="#10151E"/>','<text x="48" y="65" fill="#EDF2FA" font-family="Arial" font-size="32">MEKTRA / CONNECTED HEXAGON WORLDS</text>','<text x="48" y="108" fill="#BCC9DD" font-family="Arial" font-size="17">Black honeycomb clusters, clear travel gaps, and stars between receiving shapes. Editable concepts.</text>']
# Point-up hexagons share full edges on this axial lattice.
R=30; dx=math.sqrt(3)*R
patterns=[[(0,0),(1,0),(1,-1)],[(0,0),(1,0),(0,1)],[(0,0),(1,0),(2,0),(1,-1)],[(0,0),(1,0),(1,-1),(0,1)]]
names=['Honeycomb crossing','Staggered chains','Hexagon narrows','Branching lattice']
for v in range(4):
 x=48+445*v
 out.append(f'<g id="Mektra_level_{61+v}" transform="translate({x} 215)"><rect id="playfield-background" width="390" height="1080" fill="#9CA3AF"/>')
 centres=[]
 for row in range(6):
  cy=970-row*175
  cx=(95 if row%2==0 else 215) if v!=2 else (80 if row%2==0 else 195)
  pattern=patterns[(v+row)%4] if v!=2 else patterns[2]
  if row in (0,5):pattern=[(0,0),(1,0)]
  centres.append((cx+25,cy))
  out.append(f'<g id="hex-cluster-{row}">')
  for j,(q,r) in enumerate(pattern):
   hx=cx+dx*(q+r/2);hy=cy+45*r
   pts=' '.join(f'{hx+R*math.cos(math.pi/6+k*math.pi/3):.2f},{hy+R*math.sin(math.pi/6+k*math.pi/3):.2f}' for k in range(6))
   # point-up orientation with lattice adjusted below
   pts=' '.join(f'{hx+R*math.cos(-math.pi/2+k*math.pi/3):.2f},{hy+R*math.sin(-math.pi/2+k*math.pi/3):.2f}' for k in range(6))
   out.append(f'<polygon id="platform-{row}-{j}" points="{pts}" fill="#05090E" stroke="#34404D" stroke-width="0.8"/>')
  out.append('</g>')
 for j,((ax,ay),(bx,by)) in enumerate(zip(centres,centres[1:])):
  for k,t in enumerate((.38,.65)):
   sx=ax+(bx-ax)*t;sy=ay+(by-ay)*t
   pts=' '.join(f'{sx+(7 if n%2==0 else 3)*math.cos(-math.pi/2+n*math.pi/5):.2f},{sy+(7 if n%2==0 else 3)*math.sin(-math.pi/2+n*math.pi/5):.2f}' for n in range(10))
   out.append(f'<polygon id="star-{j*2+k+1}" points="{pts}" fill="#FFFFFF"/>')
 out.append('</g>')
 out.append(f'<text x="{x}" y="1332" fill="#EDF2FA" font-family="Arial" font-size="18">level_{61+v} / {names[v]}</text>')
out.append('</svg>');p=root/'figma/Mektra_hexagon_worlds.svg';p.write_text(''.join(out));print(p)

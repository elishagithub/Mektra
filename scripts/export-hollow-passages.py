"""Four editable Figma concept layouts; not added to the live game pack."""
from pathlib import Path
import math
root=Path(__file__).resolve().parents[1]
out=['<svg xmlns="http://www.w3.org/2000/svg" width="1860" height="1400" viewBox="0 0 1860 1400">','<rect width="1860" height="1400" fill="#10151E"/>','<text x="48" y="65" fill="#EDF2FA" font-size="32" font-family="Arial">MEKTRA / HOLLOW PASSAGES</text>','<text x="48" y="105" fill="#BCC9DD" font-size="17" font-family="Arial">Four later-world concepts: travel upward through the hollow middle, jumping between shapes.</text>','<text x="48" y="140" fill="#BCC9DD" font-size="16" font-family="Arial">Editable walls, platforms and stars. Concepts for review; not yet part of the running game.</text>']
# (y, left wall edge, right wall edge), top to bottom. Wide pockets alternate with constrictions.
profiles=[[(0,70,320),(160,70,320),(190,155,320),(330,155,320),(360,65,320),(510,65,235),(540,65,235),(680,65,320),(710,155,320),(870,155,320),(900,65,320),(1080,65,320)],[(0,70,320),(150,70,320),(190,70,235),(320,70,235),(360,150,320),(500,150,320),(540,60,235),(690,60,235),(730,150,320),(880,150,320),(920,70,320),(1080,70,320)],[(0,65,325),(130,65,325),(250,155,325),(380,155,325),(510,65,235),(640,65,235),(770,155,325),(900,155,325),(1010,65,325),(1080,65,325)],[(0,70,320),(150,70,320),(180,145,320),(300,145,320),(330,60,320),(440,60,225),(560,60,225),(590,150,320),(710,150,320),(740,60,320),(880,60,235),(910,60,320),(1080,60,320)]]
centres=[[(195,985),(235,810),(135,610),(200,435),(240,265),(195,80)],[(195,985),(235,820),(120,640),(235,455),(140,255),(195,75)],[(195,995),(235,835),(145,640),(225,420),(235,250),(190,70)],[(190,985),(140,825),(230,660),(140,495),(235,300),(195,75)]]
names=['Stepped pockets','Alternating chambers','Diagonal canyon','Mixed shape weave']
for v,profile in enumerate(profiles):
 x=48+445*v; y=215
 out.append(f'<g id="Mektra_level_{57+v}" transform="translate({x} {y})"><rect id="playfield-background" width="390" height="1080" fill="#9CA3AF"/>')
 left='M 0 0 L '+' L '.join(f'{l} {yy}' for yy,l,r in profile)+' L 0 1080 Z'
 right='M 390 0 L '+' L '.join(f'{r} {yy}' for yy,l,r in profile)+' L 390 1080 Z'
 for name,path in [('left',left),('right',right)]:out.append(f'<path id="wall-{name}" d="{path}" fill="#05090E" stroke="#EDF2FA" stroke-width="2.5" stroke-linejoin="round"/>')
 for j,(cx,cy) in enumerate(centres[v]):
  kind=(j+v)%5; radius=29 if j not in (0,5) else 34
  if j in (0,5) or kind==0:out.append(f'<circle id="platform-{j}" cx="{cx}" cy="{cy}" r="{radius}" fill="#05090E"/>')
  elif kind==1:out.append(f'<rect id="platform-{j}" x="{cx-28}" y="{cy-28}" width="56" height="56" rx="14" fill="#05090E"/>')
  else:
   n=6 if kind==2 else 4 if kind==3 else 64
   pts=[]
   for k in range(n):
    a=2*math.pi*k/n-math.pi/2; rr=radius if kind!=4 else radius*(1+.17*math.cos(3*a))
    pts.append(f'{cx+rr*math.cos(a):.2f},{cy+rr*math.sin(a):.2f}')
   out.append(f'<polygon id="platform-{j}" points="{" ".join(pts)}" fill="#05090E"/>')
 # Stars sit between receiving shapes; avoid wall intrusions.
 for j,((ax,ay),(bx,by)) in enumerate(zip(centres[v],centres[v][1:])):
  cy=(ay+by)/2; cx=(ax+bx)/2
  # Interpolated playable interval at star height.
  for (y0,l0,r0),(y1,l1,r1) in zip(profile,profile[1:]):
   if y0<=cy<=y1:
    t=(cy-y0)/(y1-y0); l=l0+(l1-l0)*t; r=r0+(r1-r0)*t;cx=max(l+18,min(r-18,cx));break
  pts=[]
  for k in range(10):
   a=-math.pi/2+k*math.pi/5;rr=7 if k%2==0 else 3
   pts.append(f'{cx+rr*math.cos(a):.2f},{cy+rr*math.sin(a):.2f}')
  out.append(f'<polygon id="star-{j+1}" points="{" ".join(pts)}" fill="#FFFFFF"/>')
 out.append('</g>')
 out.append(f'<text x="{x}" y="1332" fill="#EDF2FA" font-size="18" font-family="Arial">{57+v} / {names[v]}</text>')
out.append('</svg>');p=root/'figma/Mektra_hollow_passages.svg';p.write_text(''.join(out));print(p)

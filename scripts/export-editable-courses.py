"""Exact playfield frames for editable Figma ↔ Unity SVG round trips."""
import json,math,html,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
pack_name=sys.argv[1] if len(sys.argv)>1 else 'endless'
pack=json.loads((root/'assets/levels'/pack_name/'pack.json').read_text())
folder=root/'assets/levels'/pack_name/'figma-editable';folder.mkdir(exist_ok=True)
for c in pack['levels']:
 level=c['level'];height=int(level['height']);parts=[f'<svg xmlns="http://www.w3.org/2000/svg" width="390" height="{height}" viewBox="0 0 390 {height}">',f'<rect width="390" height="{height}" fill="#9CA3AF"/>']
 for p in level['platforms']:
  name=p['id'];m=p.get('motion')
  if m and m['type']!='static':name+='__'+'__'.join(str(m.get(k,0)) for k in ['type','amplitudeX','amplitudeY','periodSeconds','phase','speedVariation'])
  parts.append(f'<path id="{html.escape(name)}" d="{p["d"]}" fill="#03060B" stroke="{"#66FFD9" if p.get("sourceShapeId")=="wedge" else "#03060B"}" stroke-width="2"/>')
 for hazard in level.get('hazards') or []:
  name=hazard['id'];m=hazard.get('motion')
  if m and m['type']!='static':name+='__'+'__'.join(str(m.get(k,0)) for k in ['type','amplitudeX','amplitudeY','periodSeconds','phase','speedVariation'])
  parts.append(f'<path id="{html.escape(name)}" d="{hazard["d"]}" fill="#03060B" stroke="#FF8547" stroke-width="1.5"/>')
 for hole in level.get('blackHoles') or []:
  parts.append(f'<circle id="{hole["id"]}" cx="{hole["x"]}" cy="{hole["y"]}" r="{hole["radius"]}" fill="#000000"/>')
  parts.append(f'<circle cx="{hole["x"]}" cy="{hole["y"]}" r="{hole["pullRadius"]}" fill="none" stroke="#BB80FF" stroke-opacity=".4"/>')
  for arm in range(3):
   pts=[]
   for i in range(41):
    t=i/40;a=t*7+arm*2*math.pi/3;r=hole['radius']*(1-t)*.92
    pts.append(f'{hole["x"]+math.cos(a)*r:.4f},{hole["y"]+math.sin(a)*r:.4f}')
   parts.append(f'<polyline points="{" ".join(pts)}" fill="none" stroke="#BFA0FF" stroke-width="1.2"/>')
 for run_index,run in enumerate(level.get('bounceRuns') or []):
  for i,p in enumerate(run['points']):
   parts.append(f'<circle id="bounce-{run_index}-point-{i}" cx="{p["x"]}" cy="{p["y"]}" r="2" fill="#66FFD9"/>')
 for s in level['stars']:
  name=s['id']+('__follow__'+s['parentPlatformId'] if s.get('parentPlatformId') else '')
  points=[]
  for i in range(10):
   angle=-math.pi/2+i*math.pi/5;r=7.7 if i%2==0 else 3.465
   points.append(f'{s["x"]+math.cos(angle)*r:.5f},{s["y"]+math.sin(angle)*r:.5f}')
  parts.append(f'<polygon id="{name}" points="{" ".join(points)}" fill="#FFFFFF"/>')
 parts.append('</svg>');(folder/f'Mektra_level_{c["number"]}.svg').write_text(''.join(parts))
print(f'Exported {len(pack["levels"])} exact editable course frames.')

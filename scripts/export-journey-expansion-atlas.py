from pathlib import Path
import xml.etree.ElementTree as E, json, math, copy, re
root=Path(__file__).resolve().parents[1];N='{http://www.w3.org/2000/svg}';E.register_namespace('',N[1:-1]);r=E.parse(root/'figma/Mektra_worlds_smooth_sine.svg').getroot()
pack=json.loads((root/'assets/levels/endless/expansion-preview.json').read_text());courses=pack['levels'];by={c['number']:c for c in courses}
def stars(g,s,offset):
 pts=[]
 for j in range(10):
  a=-math.pi/2+j*math.pi/5;radius=5 if j%2==0 else 2.2;pts.append(f'{s["x"]+radius*math.cos(a):.4f},{s["y"]+offset+radius*math.sin(a):.4f}')
 E.SubElement(g,N+'polygon',{'id':s['id'],'points':' '.join(pts),'fill':'#FFD166'})
def level(number,local_y):
 c=by[number];lv=c['level'];g=E.Element(N+'g',{'id':f'Mektra_level_{number}','transform':f'translate(48 {local_y})'});E.SubElement(g,N+'rect',{'width':'390','height':'5400','fill':'#9CA3AF'})
 clip=E.SubElement(g,N+'clipPath',{'id':f'level-clip-{number}'});E.SubElement(clip,N+'rect',{'width':'390','height':'5400'})
 content=E.SubElement(g,N+'g',{'clip-path':f'url(#level-clip-{number})'})
 for tile in range(5):
  section=E.SubElement(content,N+'g',{'id':f'Screen_run_{tile+1}','transform':f'translate(0 {tile*1080})'})
  for p in lv['platforms']:
   E.SubElement(section,N+'path',{'id':p['id'],'d':p['d'],'fill':'#03060B','stroke':'#66FFD9' if p.get('sourceShapeId')=='wedge' else '#03060B','stroke-width':'1.5' if p.get('sourceShapeId')=='wedge' else '1'})
  for p in lv.get('hazards') or []:E.SubElement(section,N+'path',{'id':p['id'],'d':p['d'],'fill':'#03060B','stroke':'#FF8547','stroke-width':'1'})
  for s in lv['stars']:stars(section,s,0)
 return g
# Update the revised rain and the three formerly concept-only world families in place.
for w in r:
 m=re.match(r'World_(\d+)',w.get('id',''))
 if not m:continue
 wi=int(m[1]);first=(wi-1)*4+1
 if wi not in (10,15,16,17):continue
 for child in list(w):
  lm=re.fullmatch(r'Mektra_level_(\d+)',child.get('id',''))
  if lm:
   number=int(lm[1]);t=child.get('transform','');nums=re.findall(r'-?\d+(?:\.\d+)?',t);local_y=float(nums[1]);idx=list(w).index(child);w.remove(child);w.insert(idx,level(number,local_y))
for wi in (18,19,20):
 first=(wi-1)*4+1;name=by[first]['title'];w=E.SubElement(r,N+'g',{'id':f'World_{wi:02d}__Levels_{first}_{first+3}','transform':f'translate({48+(wi-1)*800} 200)'})
 E.SubElement(w,N+'rect',{'width':'680','height':'22140','rx':'18','fill':'#111827'})
 E.SubElement(w,N+'text',{'x':'48','y':'56','font-family':'Arial','font-size':'26','fill':'#EAF2FF'}).text=f'{wi:02d} / {name}'
 for v in range(4):
  y=215+v*5460;E.SubElement(w,N+'text',{'x':'48','y':str(y-28),'font-family':'Arial','font-size':'18','fill':'#EAF2FF'}).text=f'level_{first+v}';w.append(level(first+v,y))
r.set('width','16100');r.set('viewBox','0 0 16100 23000')
for child in r:
 if child.tag==N+'rect' and child.get('width')=='13700':child.set('width','16100')
E.ElementTree(r).write(root/'figma/Mektra_worlds_expanded_journey.svg',encoding='unicode')
print('Updated rain and concept worlds in place; added 12 layouts in three columns. 80 layouts in 20 worlds.')

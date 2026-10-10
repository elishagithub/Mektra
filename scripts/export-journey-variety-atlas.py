"""Five-screen journey windows: actual consecutive courses, never five copies."""
from pathlib import Path
import json,math,sys,copy,xml.etree.ElementTree as E
root=Path(__file__).resolve().parents[1];N='{http://www.w3.org/2000/svg}';E.register_namespace('',N[1:-1])
p=json.loads((root/(sys.argv[1] if len(sys.argv)>1 else 'assets/levels/endless/pack.json')).read_text());by={c['number']:c for c in p['levels']};order=[c['designLevel'] for c in json.loads((root/'assets/levels/endless/journey-order.json').read_text())]
r=E.Element(N+'svg',{'width':'16900','height':'23000','viewBox':'0 0 16900 23000'});E.SubElement(r,N+'rect',{'width':'16900','height':'23000','fill':'#0B1220'})
E.SubElement(r,N+'text',{'x':'48','y':'74','font-family':'Arial','font-size':'36','fill':'#EAF2FF'}).text='Mektra / Varied endless journey'
E.SubElement(r,N+'text',{'x':'48','y':'118','font-family':'Arial','font-size':'20','fill':'#B8C2D6'}).text='Four frames per world. Each frame shows five consecutive runtime screens, climbing bottom to top. Individual courses: 390 × 1080.'
for wi in range(21):
 first=wi*4+1;w=E.SubElement(r,N+'g',{'id':f'World_{wi+1:02d}__Levels_{first}_{first+3}','transform':f'translate({48+wi*800} 200)'});E.SubElement(w,N+'rect',{'width':'680','height':'22140','rx':'18','fill':'#111827'});E.SubElement(w,N+'text',{'x':'48','y':'56','font-family':'Arial','font-size':'26','fill':'#EAF2FF'}).text=f'{wi+1:02d} / {by[first]["title"]}'
 for v in range(4):
  number=first+v;y=215+v*5460;at=order.index(number);sequence=[order[(at+t)%len(order)] for t in range(5)]
  E.SubElement(w,N+'text',{'x':'48','y':str(y-28),'font-family':'Arial','font-size':'18','fill':'#EAF2FF'}).text=f'level_{number} / journey '+ ' → '.join(map(str,sequence))
  g=E.SubElement(w,N+'g',{'id':f'Mektra_level_{number}__Five_screen_journey','transform':f'translate(48 {y})'});E.SubElement(g,N+'rect',{'width':'390','height':'5400','fill':'#9CA3AF'});clip=E.SubElement(g,N+'clipPath',{'id':f'clip-{number}'});E.SubElement(clip,N+'rect',{'width':'390','height':'5400'});content=E.SubElement(g,N+'g',{'clip-path':f'url(#clip-{number})'})
  for tile,n in enumerate(sequence):
   lv=by[n]['level'];sg=E.SubElement(content,N+'g',{'id':f'Screen_{tile+1}__Actual_level_{n}','transform':f'translate(0 {(4-tile)*1080})'})
   for shape in lv['platforms']:E.SubElement(sg,N+'path',{'id':shape['id']+'__'+shape['sourceShapeId'],'d':shape['d'],'fill':'#03060B','stroke':'#66FFD9' if shape['sourceShapeId']=='wedge' else '#03060B','stroke-width':'1.5'})
   for shape in lv.get('hazards') or []:E.SubElement(sg,N+'path',{'id':shape['id'],'d':shape['d'],'fill':'#03060B','stroke':'#FF8547','stroke-width':'1'})
   for hole in lv.get('blackHoles') or []:
    E.SubElement(sg,N+'circle',{'id':hole['id'],'cx':str(hole['x']),'cy':str(hole['y']),'r':str(hole['radius']),'fill':'#000000'});E.SubElement(sg,N+'circle',{'cx':str(hole['x']),'cy':str(hole['y']),'r':str(hole['pullRadius']),'fill':'none','stroke':'#BFA0FF','stroke-opacity':'.4'})
   for star in lv['stars']:
    pts=[]
    for j in range(10):
     a=-math.pi/2+j*math.pi/5;rad=5 if j%2==0 else 2.2;pts.append(f'{star["x"]+rad*math.cos(a):.4f},{star["y"]+rad*math.sin(a):.4f}')
    E.SubElement(sg,N+'polygon',{'id':star['id'],'points':' '.join(pts),'fill':'#FFD166'})
old=E.parse(root/'figma/Mektra_worlds_expanded_journey.svg').getroot()
for child in old:
 if child.get('id')=='Crumble_countdown_states':r.append(copy.deepcopy(child))
E.ElementTree(r).write(root/'figma/Mektra_worlds_varied_journey.svg',encoding='unicode')
print('21 columns, 84 frames, 420 actual journey screens; positions preserved.')

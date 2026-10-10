from pathlib import Path
import json,math,xml.etree.ElementTree as E
root=Path(__file__).resolve().parents[1];p=json.loads((root/'assets/levels/endless/giant-curves-preview.json').read_text());N='{http://www.w3.org/2000/svg}';E.register_namespace('',N[1:-1])
r=E.Element(N+'svg',{'width':'680','height':'22140','viewBox':'0 0 680 22140'});w=E.SubElement(r,N+'g',{'id':'World_21__Levels_81_84__Giant_curves'});E.SubElement(w,N+'rect',{'width':'680','height':'22140','rx':'18','fill':'#111827'});E.SubElement(w,N+'text',{'x':'48','y':'56','font-family':'Arial','font-size':'26','fill':'#EAF2FF'}).text='21 / Giant lobes'
for v,c in enumerate(p['levels'][80:84]):
 y=215+v*5460;lv=c['level'];E.SubElement(w,N+'text',{'x':'48','y':str(y-28),'font-family':'Arial','font-size':'18','fill':'#EAF2FF'}).text=f'level_{81+v}';g=E.SubElement(w,N+'g',{'id':f'Mektra_level_{81+v}','transform':f'translate(48 {y})'});E.SubElement(g,N+'rect',{'width':'390','height':'5400','fill':'#9CA3AF'});clip=E.SubElement(g,N+'clipPath',{'id':f'clip-{v}'});E.SubElement(clip,N+'rect',{'width':'390','height':'5400'});content=E.SubElement(g,N+'g',{'clip-path':f'url(#clip-{v})'})
 for tile in range(5):
  sg=E.SubElement(content,N+'g',{'id':f'Screen_run_{tile+1}','transform':f'translate(0 {tile*1080})'})
  for shape in lv['platforms']:E.SubElement(sg,N+'path',{'id':shape['id']+'__'+shape['sourceShapeId'],'d':shape['d'],'fill':'#03060B'})
  for star in lv['stars']:
   pts=[]
   for j in range(10):
    a=-math.pi/2+j*math.pi/5;radius=5 if j%2==0 else 2.2;pts.append(f'{star["x"]+radius*math.cos(a):.4f},{star["y"]+radius*math.sin(a):.4f}')
   E.SubElement(sg,N+'polygon',{'id':star['id'],'points':' '.join(pts),'fill':'#FFD166'})
E.ElementTree(r).write(root/'figma/Mektra_giant_lobes_no_four_81_84.svg',encoding='unicode')

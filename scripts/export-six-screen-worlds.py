from pathlib import Path
import xml.etree.ElementTree as E
import copy,re
root=Path(__file__).resolve().parents[1]
n='{http://www.w3.org/2000/svg}'
r=E.fromstring((root/'figma/Mektra_all_worlds_vertical.svg').read_text())
height=4896; gap=60; world_height=215+4*(height+gap)+85; stride=world_height+150
for wi,w in enumerate([c for c in r if c.get('id','').startswith('World_')]):
 w.set('transform',f'translate(48 {200+wi*stride})')
 for c in list(w):
  if c.tag==n+'rect':c.set('height',str(world_height))
  elif c.tag==n+'text' and float(c.get('y','0'))>1000:
   j=round((float(c.get('y'))-1320)/1140);c.set('y',str(215+j*(height+gap)+height+25))
  elif c.tag==n+'g' and c.get('id','').startswith('Mektra_level_'):
   number=int(c.get('id').split('_')[-1]);j=(number-1)%4;c.set('transform',f'translate(48 {215+j*(height+gap)})')
   children=list(c);c.clear();c.set('id',f'Mektra_level_{number}');c.set('transform',f'translate(48 {215+j*(height+gap)})')
   E.SubElement(c,n+'rect',{'width':'390','height':str(height),'fill':'#9CA3AF'})
   for tile in range(5):
    section=E.SubElement(c,n+'g',{'id':f'Screen_run_{tile+1}','transform':f'translate(0 {tile*height/5})'})
    for original in children:
     if original.tag==n+'rect' and original.get('width')=='390' and original.get('height')=='1080':continue
     s=copy.deepcopy(original);role=s.get('id','');s.set('id',f'{role}_run_{tile+1}' if role else f'element_run_{tile+1}_{len(section)}')
     ratio=height/5/1080
     # Corridors retain a continuous wall; discrete shapes retain their geometry.
     if any(k in role.lower() for k in ('wall','corridor','boundary')):s.set('transform',f'scale(1 {ratio}) '+s.get('transform',''))
     else:
      if s.get('cy') is not None:cy=float(s.get('cy'))
      elif s.get('y') is not None:cy=float(s.get('y'))+float(s.get('height','0'))/2
      else:
       nums=[float(v) for v in re.findall(r'-?\d+(?:\.\d+)?',s.get('d',s.get('points','')))];ys=nums[1::2];cy=(min(ys)+max(ys))/2 if ys else 540
      s.set('transform',f'translate(0 {cy*(ratio-1)}) '+s.get('transform',''))
     section.append(s)
total=200+16*stride+1000
r.set('height',str(total));r.set('viewBox',f'0 0 776 {total}')
for c in r:
 if c.tag==n+'rect':c.set('height',str(total))
 if c.tag==n+'text' and c.get('y')=='120':c.text='Each level: 390 × 4896 / six 816-high mobile screens';c.set('font-size','16')
 if c.get('id')=='Crumble_countdown_states':c.set('transform',f'translate(48 {200+16*stride}) scale(0.18)')
s=E.tostring(r,encoding='unicode').replace('ns0:','').replace('xmlns:ns0=','xmlns=')
(root/'figma/Mektra_all_worlds_six_screens.svg').write_text(s)
assert len([g for g in r.iter(n+'g') if g.get('id','').startswith('Mektra_level_')])==64
print(f'64 editable levels, 390 × {height} each; total height {total}.')

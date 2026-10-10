import json,math
from pathlib import Path
root=Path(__file__).resolve().parents[1];shapes=json.loads((root/'assets/shapes/custom/catalog.json').read_text())['shapes']
def pts(s):return [(p['x'],p['y']) for p in s['outline']]
def inside(p,v):
 x,y=p;hit=False
 for a,b in zip(v,v[1:]+v[:1]):
  if (a[1]>y)!=(b[1]>y) and x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0]:hit=not hit
 return hit
def cross(a,b):return a[0]*b[1]-a[1]*b[0]
def union(a,b):
 edges=[]
 for v,other in [(a,b),(b,a)]:
  for p,q in zip(v,v[1:]+v[:1]):
   d=(q[0]-p[0],q[1]-p[1]);cuts=[0.,1.]
   for r,s in zip(other,other[1:]+other[:1]):
    e=(s[0]-r[0],s[1]-r[1]);den=cross(d,e)
    if abs(den)<1e-10:continue
    rp=(r[0]-p[0],r[1]-p[1]);t=cross(rp,e)/den;u=cross(rp,d)/den
    if 0<t<1 and 0<=u<=1:cuts.append(t)
   cuts=sorted(set(cuts))
   for t,u in zip(cuts,cuts[1:]):
    m=(t+u)/2
    if inside((p[0]+d[0]*m,p[1]+d[1]*m),other):continue
    edges.append(((round(p[0]+d[0]*t,7),round(p[1]+d[1]*t,7)),(round(p[0]+d[0]*u,7),round(p[1]+d[1]*u,7))))
 lookup={x:y for x,y in edges};out=[edges[0][0]]
 while len(out)<len(edges)+2:
  q=lookup[out[-1]]
  if q==out[0]:return out
  out.append(q)
 raise ValueError('Union did not close')
entries=[]
for n,num in [(2,5),(3,6),(4,25),(5,24)]:
 s=shapes[num-1];entries.append({'lobes':n,'sourceId':s['id'],'outline':s['outline']})
base=pts(shapes[21]);five=union(base,[(x+73.5,y+.17) for x,y in base])
for n,shift in [(6,110.3),(7,147.0)]:
 v=union(five,[(x+shift,y+.34) for x,y in base]);entries.append({'lobes':n,'sourceId':'joined-custom-shape-22','outline':[{'x':x,'y':y} for x,y in v]})
(root/'assets/shapes/custom/giant-lobe-library.json').write_text(json.dumps({'shapes':entries},indent=2)+'\n')
print([(s['lobes'],len(s['outline'])) for s in entries])

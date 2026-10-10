from pathlib import Path
import xml.etree.ElementTree as E, math, re, json
N='{http://www.w3.org/2000/svg}'; E.register_namespace('',N[1:-1]); root=E.parse('figma/Mektra_worlds_star_jumps.svg').getroot(); changed=0
for e in root.iter(N+'path'):
 if not e.get('data-curve-variant'):continue
 nums=list(map(float,re.findall(r'-?\d+(?:\.\d+)?',e.get('d','')))); pts=list(zip(nums[::2],nums[1::2])); cx=(min(x for x,y in pts)+max(x for x,y in pts))/2; cy=(min(y for x,y in pts)+max(y for x,y in pts))/2; rx=(max(x for x,y in pts)-min(x for x,y in pts))/2; ry=(max(y for x,y in pts)-min(y for x,y in pts))/2
 phase=(changed*37%360)*math.pi/180; count=96; amp=.065; points=[]; deriv=[]
 for k in range(count):
  t=k*math.tau/count; r=1+amp*math.cos(3*t+phase); dr=-3*amp*math.sin(3*t+phase)
  points.append((r*math.cos(t),r*math.sin(t))); deriv.append((dr*math.cos(t)-r*math.sin(t),dr*math.sin(t)+r*math.cos(t)))
 sx=2*rx/(max(x for x,y in points)-min(x for x,y in points)); sy=2*ry/(max(y for x,y in points)-min(y for x,y in points)); ox=(min(x for x,y in points)+max(x for x,y in points))/2; oy=(min(y for x,y in points)+max(y for x,y in points))/2
 points=[(cx+(x-ox)*sx,cy+(y-oy)*sy) for x,y in points]; deriv=[(x*sx,y*sy) for x,y in deriv]; h=math.tau/count/3; d=[f'M{points[0][0]:.5f} {points[0][1]:.5f}']
 for k in range(count):
  j=(k+1)%count; a,b=points[k],points[j]; u,v=deriv[k],deriv[j]; d.append(f'C{a[0]+u[0]*h:.5f} {a[1]+u[1]*h:.5f} {b[0]-v[0]*h:.5f} {b[1]-v[1]*h:.5f} {b[0]:.5f} {b[1]:.5f}')
 e.set('d',' '.join(d)+' Z'); e.set('data-curve-variant','broad_sine_smooth'); changed+=1
E.ElementTree(root).write('figma/Mektra_worlds_smooth_sine.svg',encoding='unicode'); print({'smoothed':changed,'amplitude':amp,'harmonics':3,'segments':count})

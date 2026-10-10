"""Describe the spatial and mechanical differences between the four authored variants."""
import json, math, sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
source=root/(sys.argv[1] if len(sys.argv)>1 else 'assets/levels/endless/pack.json')
pack=json.loads(source.read_text());worlds=[]
for start in range(0,len(pack['levels']),4):
 courses=pack['levels'][start:start+4];variants=[]
 for c in courses:
  lv=c['level'];ps=lv['platforms'];centers=[((p['bounds']['minX']+p['bounds']['maxX'])/2,(p['bounds']['minY']+p['bounds']['maxY'])/2) for p in ps]
  gaps=[round(math.dist(centers[r['from']],centers[r['to']])) for r in c['routes']]
  variants.append({'level':c['number'],'platforms':len(ps),'jumps':len(c['routes']),'stars':len(lv['stars']),'routeGaps':gaps,'receivingShape':ps[-1]['sourceShapeId'],'movingSurfaces':sum(bool(p.get('motion') and p['motion']['type']!='static') for p in ps)})
 # Shape coordinates, hazards, flow and rewards together define the authored playfield.
 signatures=[json.dumps({k:c['level'].get(k) for k in ('platforms','connections','hazards','stars','blackHoles')},sort_keys=True) for c in courses]
 worlds.append({'world':courses[0]['title'],'kind':courses[0]['level']['segmentKind'],'distinctPlayfields':len(set(signatures)),'variants':variants})
report={'layouts':len(pack['levels']),'families':len(worlds),'worlds':worlds,'note':'Structural differences are reviewed alongside physical jump, collection and continuous-run checks. Distinct geometry alone does not measure enjoyment.'}
(root/'assets/levels/endless/layout-variety-review.json').write_text(json.dumps(report,indent=2)+'\n')
print(f"Reviewed {report['layouts']} layouts in {report['families']} families; {sum(w['distinctPlayfields']==4 for w in worlds)} families contain four distinct playfields.")

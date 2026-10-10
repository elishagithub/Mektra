"""Preserve every current custom SVG shape as an independent, origin-normalized asset."""
import json,html
from pathlib import Path
root=Path(__file__).resolve().parents[1]
source=json.loads((root/'Unity/Assets/Resources/level01.json').read_text())
folder=root/'assets/shapes/custom';folder.mkdir(parents=True,exist_ok=True)
shapes=[];parts=['<svg xmlns="http://www.w3.org/2000/svg" width="1450" height="1680" viewBox="0 0 1450 1680"><rect width="1450" height="1680" fill="#101D30"/><text x="40" y="65" font-family="Inter" font-size="30" fill="white">MEKTRA / YOUR CUSTOM SHAPES</text><text x="40" y="100" font-family="Inter" font-size="15" fill="#9AB0CB">29 original objects · exact current vector geometry · reusable in every course</text>']
for index,p in enumerate(source['platforms']):
 q=json.loads(json.dumps(p));b=q['bounds'];dx=b['minX'];dy=b['minY'];w=b['maxX']-dx;h=b['maxY']-dy
 for pt in q['outline']:pt['x']-=dx;pt['y']-=dy
 for c in q['paths']:
  for i in range(0,len(c['v']),2):c['v'][i]-=dx;c['v'][i+1]-=dy
 q['bounds']={'minX':0,'maxX':w,'minY':0,'maxY':h};q['id']=f'custom-shape-{index+1:02d}';q['sourceShapeId']=p['id'];q['motion']=None;q['d']=' '.join(c['op']+' '.join(str(v) for v in c['v']) for c in q['paths']);shapes.append(q)
 (folder/(q['id']+'.json')).write_text(json.dumps(q,indent=2)+'\n')
 (folder/(q['id']+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}"><path id="{q["id"]}" d="{q["d"]}" fill="#03060B"/></svg>')
 x=40+index%5*280;y=140+index//5*250
 parts.append(f'<g id="{q["id"]}" transform="translate({x} {y})"><rect width="250" height="220" rx="16" fill="#DEE8F3"/><path id="shape-vector-{index+1}" transform="translate({125-w/2} {95-h/2})" d="{q["d"]}" fill="#03060B"/><text x="16" y="178" font-family="Inter" font-size="16" fill="#10213A">{q["id"]}</text><text x="16" y="200" font-family="Inter" font-size="12" fill="#53657A">Original {p["id"]} · {w:.1f} × {h:.1f}</text></g>')
parts.append('</svg>');(folder/'Mektra-custom-shape-library.svg').write_text(''.join(parts));(folder/'catalog.json').write_text(json.dumps({'source':'current level01.json','shapes':shapes},indent=2)+'\n')
print('Preserved all 29 custom shapes as individual SVG + JSON objects.')

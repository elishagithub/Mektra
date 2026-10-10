from pathlib import Path
import json,html,re
root=Path(__file__).resolve().parents[1]; pack=json.loads((root/'assets/levels/endless/pack.json').read_text())
for start in range(0,56,4):
 courses=pack['levels'][start:start+4]; title=courses[0]['title']
 name=f'{start//4+1:02d}_Levels_{start+1:02d}_{start+4:02d}'
 out=['<svg xmlns="http://www.w3.org/2000/svg" width="1860" height="1400" viewBox="0 0 1860 1400">','<rect width="1860" height="1400" fill="#10151E"/>',f'<text x="48" y="65" fill="#EDF2FA" font-size="32" font-family="Arial">{name} / {html.escape(title)}</text>',f'<text x="48" y="110" fill="#BCC9DD" font-size="17" font-family="Arial">{html.escape(courses[0]["level"]["instruction"])}</text>']
 for i,c in enumerate(courses):
  x=48+i*445;svg=(root/f'assets/levels/endless/figma-editable/Mektra_level_{c["number"]}.svg').read_text();contents=re.sub(r'^<svg[^>]+>|</svg>$','',svg)
  out.append(f'<g id="Mektra_level_{c["number"]}" transform="translate({x} 215)">{contents}</g>')
  out.append(f'<text x="{x}" y="1332" fill="#EDF2FA" font-size="18" font-family="Arial">level_{c["number"]}</text>')
 out.append('</svg>');(root/'figma/world-boards'/f'{name}.svg').write_text(''.join(out))
print('Created 14 editable four-level boards.')

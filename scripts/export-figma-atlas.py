"""Editable vector boards grouped into exact-size playfield groups for Figma import."""
from pathlib import Path
import json,html,re
root=Path(__file__).resolve().parents[1]
pack=json.loads((root/'assets/levels/endless/pack.json').read_text())
for name,courses,title,subtitle in [('Mektra_later_worlds',pack['levels'][28:],'MEKTRA / LATER WORLDS','Starts after section 16 (~1.7 km). Eight-section world chapters: five encounters + three recovery stretches.'),('Mektra_rain_wedge_revisions',[c for c in pack['levels'] if c['level']['segmentKind'] in (9,11)],'MEKTRA / RAIN & WEDGE REVISIONS','Updated from your paper sketches. Black drops carry the player left. Wedge face angles produce physical rebounds into an upper catcher.'),('Mektra_opening_library',pack['levels'][:28],'MEKTRA / OPENING LIBRARY','Current opening mechanics. Later chapters remix these encounters. Endless play has no level picker.')]:
 rows=len(courses)//4;height=260+rows*1310
 out=[f'<svg xmlns="http://www.w3.org/2000/svg" width="1860" height="{height}" viewBox="0 0 1860 {height}">',f'<rect width="1860" height="{height}" fill="#10151E"/>',f'<text x="48" y="65" fill="#EDF2FA" font-size="32" font-family="Arial">{title}</text>',f'<text x="48" y="105" fill="#BCC9DD" font-size="17" font-family="Arial">{html.escape(subtitle)}</text>','<text x="48" y="140" fill="#BCC9DD" font-size="16" font-family="Arial">Orange: lethal hazards. Mint: bouncy wedges. Purple: vortex pull boundary. White: collectible stars.</text>','<text x="48" y="170" fill="#BCC9DD" font-size="16" font-family="Arial">Select a Mektra_level_N group to inspect/edit; its background preserves the exact 390 x 1080 export size.</text>']
 for i,c in enumerate(courses):
  row,col=divmod(i,4);x=48+col*445;y=260+row*1310
  if col==0:
   out.append(f'<text x="48" y="{y-55}" fill="#EDF2FA" font-size="25" font-family="Arial">{html.escape(c["title"])} / {"first appears at section "+str(16+row*8) if name.endswith("worlds") else "mechanic "+str(row+1)}</text>')
   rule=c['level']['instruction'];out.append(f'<text x="48" y="{y-25}" fill="#BCC9DD" font-size="15" font-family="Arial">{html.escape(rule)}</text>')
  out.append(f'<text x="{x}" y="{y+1109}" fill="#EDF2FA" font-size="18" font-family="Arial">Variant {c["level"]["variant"]+1} / Mektra_level_{c["number"]}</text>')
  svg=(root/f'assets/levels/endless/figma-editable/Mektra_level_{c["number"]}.svg').read_text();contents=re.sub(r'^<svg[^>]+>|</svg>$','',svg)
  out.append(f'<g id="Mektra_level_{c["number"]}" transform="translate({x} {y})">{contents}</g>')
 out.append('</svg>');(root/'figma'/f'{name}.svg').write_text(''.join(out))
print('Exported editable Figma boards: opening, later worlds and eight rain/wedge revisions.')

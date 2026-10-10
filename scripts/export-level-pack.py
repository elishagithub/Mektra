"""Export the validated four-screen pack as editable SVG art for Figma."""
import json, math, html
from pathlib import Path
root=Path(__file__).resolve().parents[1]/'assets/levels/pack-25'
pack=json.loads((root/'pack.json').read_text())
esc=html.escape
palette={'EMBER FOREST':('#674627','#06242A','#69D8E8'),'ALPINE DAWN':('#A6747B','#294F66','#B2ECFF'),'VIOLET DUSK':('#593F78','#152A49','#C0A0FF'),'AURORA NIGHT':('#112849','#073E44','#77FFE0'),'STORMWOOD':('#263743','#101C2D','#8CBFFF'),'GOLDEN DUNES':('#A16852','#433342','#FFD8A5')}
colors={'Easy':'#87E0B2','Intermediate':'#79BEFF','Tough':'#FFBF72','Critical':'#FF7F9D','Fun':'#D6A1FF','Tension':'#FFAF7D'}
def text(x,y,value,size=14,color='#EAF2FF',weight=400):
 return f'<text x="{x}" y="{y}" font-family="Inter" font-size="{size}" font-weight="{weight}" fill="{color}">{esc(str(value))}</text>'
def star(x,y,size=10):
 p=[]
 for i in range(10):
  a=-math.pi/2+i*math.pi/5;r=size if i%2==0 else size*.45;p.append(f'{x+math.cos(a)*r:.2f},{y+math.sin(a)*r:.2f}')
 return f'<polygon points="{" ".join(p)}" fill="#FFFFFF"/>'
def level_svg(design,header=True):
 n=design['number'];level=design['level'];top,bottom,accent=palette[design['theme']]
 parts=[f'<g id="Mektra_level_{n}">']
 if header:
  parts += [text(0,-66,f'{n:02d} / {design["title"]}',22,weight=700),text(0,-39,f'{design["mood"].upper()} · {design["theme"]}',12,colors[design['mood']],600),text(0,-15,f'4 screens · {design["points"]} stars · {len(level["platforms"])} platforms',12,'#9AB0CB')]
 parts += [f'<defs><linearGradient id="sky{n}" x1="0" y1="0" x2="0" y2="1"><stop stop-color="{top}"/><stop offset="1" stop-color="{bottom}"/></linearGradient></defs>',f'<rect id="playfield-390x3264" width="390" height="3264" fill="url(#sky{n})"/>']
 for screen in range(1,4):
  y=screen*816
  parts += [f'<path id="screen-boundary-{screen}" d="M0 {y}H390" stroke="#FFFFFF" opacity=".18" stroke-dasharray="5 8"/>',text(400,y+5,f'S{4-screen}',11,'#9AB0CB')]
 parts += [f'<path id="wall-hazards" d="M1 0V3264M389 0V3264" stroke="#FF7391" stroke-width="2" opacity=".65"/>']
 for p in level['platforms']:
  parts += [f'<path id="{p["id"]}" d="{p["d"]}" fill="#03060B" stroke="{accent}" stroke-opacity=".45" stroke-width="1"/>']
 for s in level['stars']:
  parts += [f'<g id="{s["id"]}">{star(s["x"],s["y"])}</g>']
 start=level['platforms'][0];x=(start['bounds']['minX']+start['bounds']['maxX'])/2;y=start['bounds']['minY']-13.2
 parts += [f'<g id="Player-start"><circle cx="{x}" cy="{y}" r="10.5" fill="white"/><rect x="{x-6}" y="{y-6}" width="12" height="12" rx="4" fill="#10213A"/><circle cx="{x-2.5}" cy="{y-1}" r="1.6" fill="#2585FF"/><circle cx="{x+2.5}" cy="{y-1}" r="1.6" fill="#2585FF"/></g>',text(20,3237,'START · move upward',11,accent,600),text(20,32,'EXIT / NEXT LEVEL',11,accent,600),f'<path d="M155 52H235" stroke="{accent}" stroke-width="2"/>','</g>']
 return ''.join(parts)
for d in pack['levels']:
 svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="430" height="3384" viewBox="-20 -100 430 3384">{level_svg(d)}</svg>'
 (root/f'Mektra_level_{d["number"]}.svg').write_text(svg)
# One import creates the entire editable board. Each named group has exact world coordinates.
parts=['<svg xmlns="http://www.w3.org/2000/svg" width="2500" height="19200" viewBox="0 0 2500 19200">','<rect width="2500" height="19200" fill="#081222"/>',text(70,90,'MEKTRA / 25 SHORT WORLDS',42,weight=700),text(70,132,'Four-screen courses · readable routes · deliberate difficulty waves',20,'#9AB0CB')]
for index,d in enumerate(pack['levels']):
 col=index%5;row=index//5;x=70+col*480;y=205+row*245
 parts += [f'<g id="overview-level-{d["number"]}" transform="translate({x} {y})"><rect width="430" height="215" rx="20" fill="#14243A"/>',text(22,42,f'{d["number"]:02d} / {d["title"]}',20,weight=700),text(22,73,d['mood'].upper(),12,colors[d['mood']],600),text(22,104,d['theme'],11,'#9AB0CB'),text(22,135,f'{d["points"]} stars · {len(d["level"]["platforms"])} platforms',13),text(22,170,'390 × 3264 · 4 screens',12,'#9AB0CB'),'</g>']
for index,d in enumerate(pack['levels']):
 col=index%5;row=index//5
 parts += [f'<g transform="translate({70+col*480} {1700+row*3480})">{level_svg(d)}</g>']
parts += ['</svg>'];(root/'Mektra-25-levels.svg').write_text(''.join(parts))
(root/'README.md').write_text('''# Mektra: 25 four-screen levels\n\nEach playfield is 390 × 3264 units (4 × 816). Start at the bottom and move upward. Named black platform paths and white star paths are editable in Figma. Screen dividers, player markers, wall highlights, and exit labels are authoring guides.\n\nImport `Mektra-25-levels.svg` into Figma for the complete board, or import individual `Mektra_level_N.svg` files. SVG imports contain native editable vectors and text. JSON files contain the matching outlines, paths, stars, and tested launch distances. The existing long level is preserved.\n\nEasy levels teach broad landing windows. Intermediate levels introduce shape changes and zigzags. Tough and critical levels shrink targets. Tension levels lean toward walls while preserving body clearance. Fun levels provide three stars per jump and recovery platforms. Every fourth platform and the start/end platforms are wider recovery zones.\n\n`validation.txt` records actual Unity simulation checks across all eight characters. Difficulty tuning still requires human playtesting; route reachability does not establish subjective fairness. The pack is connected to the Unity level scheduler and cycles after level 25.\n\nRegenerate geometry: Unity editor method `Mektra.Editor.LevelPackBuilder.Build`. Regenerate SVG: `python3 scripts/export-level-pack.py`.\n''')
print(f'Exported 25 SVGs and complete board: {len("".join(parts))} bytes')

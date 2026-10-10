"""Extract named platform/star geometry, including nested Figma transforms."""
import json
import math
import re
import sys
import xml.etree.ElementTree as ET

IDENTITY = [1, 0, 0, 1, 0, 0]

def multiply(a, b):
    return [a[0]*b[0]+a[2]*b[1], a[1]*b[0]+a[3]*b[1],
            a[0]*b[2]+a[2]*b[3], a[1]*b[2]+a[3]*b[3],
            a[0]*b[4]+a[2]*b[5]+a[4], a[1]*b[4]+a[3]*b[5]+a[5]]

def transform(value):
    result = IDENTITY
    for name, raw in re.findall(r'(\w+)\(([^)]*)\)', value):
        v = [float(x) for x in re.findall(r'[-+]?(?:\d*\.\d+|\d+\.?\d*)(?:[eE][-+]?\d+)?', raw)]
        if name == 'matrix': m = v
        elif name == 'translate': m = [1, 0, 0, 1, v[0], v[1] if len(v)>1 else 0]
        elif name == 'scale': m = [v[0], 0, 0, v[1] if len(v)>1 else v[0], 0, 0]
        elif name == 'rotate':
            angle = math.radians(v[0]); c, s = math.cos(angle), math.sin(angle)
            m = [c, s, -s, c, 0, 0]
            if len(v) == 3:
                m = multiply(multiply([1,0,0,1,v[1],v[2]], m), [1,0,0,1,-v[1],-v[2]])
        elif name == 'skewX': m = [1,0,math.tan(math.radians(v[0])),1,0,0]
        elif name == 'skewY': m = [1,math.tan(math.radians(v[0])),0,1,0,0]
        else: raise ValueError(f'Unsupported SVG transform: {name}')
        result = multiply(result, m)
    return result

def geometry(node, tag):
    a = node.attrib
    if tag == 'path': return a.get('d', '')
    if tag in ('polygon', 'polyline'):
        return 'M' + a.get('points', '').strip() + ('Z' if tag == 'polygon' else '')
    if tag in ('circle', 'ellipse'):
        x,y = float(a.get('cx',0)),float(a.get('cy',0))
        rx,ry = float(a.get('r',a.get('rx',0))),float(a.get('r',a.get('ry',0)))
        return f'M{x-rx} {y}A{rx} {ry} 0 1 0 {x+rx} {y}A{rx} {ry} 0 1 0 {x-rx} {y}Z'
    if tag == 'rect':
        x,y,w,h = [float(a.get(k,0)) for k in ('x','y','width','height')]
        rx = min(float(a.get('rx',a.get('ry',0))),w/2)
        ry = min(float(a.get('ry',a.get('rx',0))),h/2)
        if not rx or not ry: return f'M{x} {y}H{x+w}V{y+h}H{x}Z'
        return f'M{x+rx} {y}H{x+w-rx}A{rx} {ry} 0 0 1 {x+w} {y+ry}V{y+h-ry}A{rx} {ry} 0 0 1 {x+w-rx} {y+h}H{x+rx}A{rx} {ry} 0 0 1 {x} {y+h-ry}V{y+ry}A{rx} {ry} 0 0 1 {x+rx} {y}Z'
    return ''

def extract_svg(source, named_only=False):
    root = ET.parse(source).getroot()
    view = [float(x) for x in root.attrib.get('viewBox','').split()]
    if len(view) != 4 or view[:2] != [0,0]:
        raise ValueError('Export the level frame as SVG with a viewBox starting at 0,0.')
    items = []
    def visit(node, parent_matrix=IDENTITY, fill='black', role=None, owner=''):
        tag = node.tag.split('}')[-1]
        if tag in ('defs','clipPath','mask','symbol'): return
        style = dict(pair.split(':',1) for pair in node.attrib.get('style','').split(';') if ':' in pair)
        fill = node.attrib.get('fill',style.get('fill',fill)).lower().replace(' ','')
        name = node.attrib.get('id','')
        if name.startswith('platform-'): role,owner = 'platform',name
        elif name.startswith('star-'): role,owner = 'star',name
        elif name.startswith('hazard-'): role,owner = 'hazard',name
        elif name.startswith('black-hole-'): role,owner = 'hole',name
        elif name.startswith('bounce-'): role,owner = 'bounce',name
        matrix = multiply(parent_matrix, transform(node.attrib.get('transform','')))
        d = geometry(node,tag)
        if d:
            kind = role or (None if named_only else ('platform' if fill in ('white','#fff','#ffffff') else 'star' if fill in ('black','#000','#000000','#303236') else None))
            if kind and fill != 'none': items.append({'role':kind,'id':owner or name,'d':d,'matrix':matrix})
        for child in node: visit(child,matrix,fill,role,owner)
    visit(root)
    return {'width':view[2],'height':view[3],'items':items}

if __name__ == '__main__':
    print(json.dumps(extract_svg(sys.argv[1], '--named-only' in sys.argv)))

import fs from 'node:fs';
import { svgPathProperties } from 'svg-path-properties';
import svgpath from 'svgpath';
const svg = fs.readFileSync('assets/levels/level-01.svg', 'utf8');
const tags = [...svg.matchAll(/<path\b[^>]*>/g)]
  .map(m => m[0])
  .filter(t => /fill="white"/.test(t));
const area = points =>
  points.reduce((a, p, i) => {
    const q = points[(i + 1) % points.length];
    return a + p.x * q.y - q.x * p.y;
  }, 0) / 2;
let bottom = 6600;
const platforms = tags.map((tag, index) => {
  let d = tag.match(/\bd="([^"]+)"/)[1];
  const before = new svgPathProperties(d),
    samples = Array.from({ length: 600 }, (_, i) =>
      before.getPointAtLength((i / 600) * before.getTotalLength()),
    );
  const old = {
    minX: Math.min(...samples.map(p => p.x)),
    maxX: Math.max(...samples.map(p => p.x)),
    minY: Math.min(...samples.map(p => p.y)),
    maxY: Math.max(...samples.map(p => p.y)),
  };
  const factor = Math.max(
    1,
    96 / Math.max(old.maxX - old.minX, old.maxY - old.minY),
  );
  const cx =
    index === 0 ? 195 : Math.max(135, Math.min(255, (old.minX + old.maxX) / 2));
  const tx = cx - ((old.minX + old.maxX) / 2) * factor,
    ty = bottom - old.maxY * factor;
  d = svgpath(d).scale(factor).translate(tx, ty).round(3).toString();
  bottom = old.minY * factor + ty - 86;
  const contours = (d.match(/[Mm][^Mm]*/g) || []).map(part => {
    const path = new svgPathProperties(part),
      length = path.getTotalLength();
    const count = Math.max(96, Math.ceil(length / 2));
    return Array.from({ length: count }, (_, i) => {
      const p = path.getPointAtLength((i / count) * length);
      return { x: +p.x.toFixed(3), y: +p.y.toFixed(3) };
    });
  });
  const outer = [...contours].sort(
    (a, b) => Math.abs(area(b)) - Math.abs(area(a)),
  )[0];
  if (area(outer) < 0) outer.reverse();
  const bounds = {
    minX: Math.min(...outer.map(p => p.x)),
    maxX: Math.max(...outer.map(p => p.x)),
    minY: Math.min(...outer.map(p => p.y)),
    maxY: Math.max(...outer.map(p => p.y)),
  };
  return { id: `platform-${index + 1}`, d, contours, outline: outer, bounds };
});
platforms.sort((a, b) => b.bounds.maxY - a.bounds.maxY);
const first = platforms[0],
  last = platforms.at(-1);
const stride = first.bounds.maxY - last.bounds.minY + 86;
fs.writeFileSync(
  'src/game/levels/level01.json',
  JSON.stringify(
    {
      id: 'level-01',
      name: 'First orbit',
      width: 390,
      height: 6800,
      stride,
      source:
        'https://www.figma.com/design/3GMPeJH9DL1N4h7rQpF4TN?node-id=2025-66',
      platforms,
      stars: [],
    },
    null,
    2,
  ) + '\n',
);
console.log(
  `Imported ${
    platforms.length
  } exact Figma paths; repeat stride ${stride.toFixed(1)}.`,
);

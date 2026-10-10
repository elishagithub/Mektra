import fs from 'node:fs';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { svgPathProperties } from 'svg-path-properties';
import svgpath from 'svgpath';

const source = process.argv[2] || 'assets/levels/Mektra_level_1.svg';
const previous = JSON.parse(
  fs.readFileSync('src/game/levels/level01.json', 'utf8'),
);
const extracted = JSON.parse(
  execFileSync(
    'python3',
    [fileURLToPath(new URL('./read-level-svg.py', import.meta.url)), source],
    { encoding: 'utf8' },
  ),
);
if (extracted.width !== 390)
  throw new Error('Keep the Mektra_level_1 frame width at 390.');
const area = points =>
  points.reduce((sum, p, i) => {
    const q = points[(i + 1) % points.length];
    return sum + p.x * q.y - q.x * p.y;
  }, 0) / 2;
const platforms = [],
  stars = [];
for (const item of extracted.items) {
  const d = svgpath(item.d).matrix(item.matrix).round(4).toString();
  const contours = (d.match(/[Mm][^Mm]*/g) || []).map(part => {
    const path = new svgPathProperties(part),
      length = path.getTotalLength();
    const count = Math.max(100, Math.ceil(length / 2));
    return Array.from({ length: count }, (_, i) => {
      const p = path.getPointAtLength((i / count) * length);
      return { x: +p.x.toFixed(4), y: +p.y.toFixed(4) };
    });
  });
  const outline = [...contours].sort(
    (a, b) => Math.abs(area(b)) - Math.abs(area(a)),
  )[0];
  if (!outline?.length) continue;
  if (area(outline) < 0) outline.reverse();
  const bounds = {
    minX: Math.min(...outline.map(p => p.x)),
    maxX: Math.max(...outline.map(p => p.x)),
    minY: Math.min(...outline.map(p => p.y)),
    maxY: Math.max(...outline.map(p => p.y)),
  };
  if (item.role === 'platform') {
    // Figma can include a white frame background; that is not a walkable platform.
    if (
      bounds.maxX - bounds.minX >= extracted.width - 1 &&
      bounds.maxY - bounds.minY >= extracted.height - 1
    )
      continue;
    platforms.push({
      id: item.id || `platform-${platforms.length + 1}`,
      d,
      contours,
      outline,
      bounds,
    });
  } else {
    // Regular stars are rotationally symmetric; equal arc-length samples recover their center.
    const path = new svgPathProperties(d),
      length = path.getTotalLength();
    const points = Array.from({ length: 500 }, (_, i) =>
      path.getPointAtLength((i / 500) * length),
    );
    stars.push({
      id: item.id || `star-${stars.length + 1}`,
      x: +(points.reduce((sum, p) => sum + p.x, 0) / 500).toFixed(4),
      y: +(points.reduce((sum, p) => sum + p.y, 0) / 500).toFixed(4),
    });
  }
}
if (platforms.length < 2 || !stars.length)
  throw new Error(
    'The frame must contain white platforms and black stars. Keep platform-* and star-* layer names.',
  );
platforms.sort((a, b) => b.bounds.maxY - a.bounds.maxY);
const gaps = platforms
  .slice(1)
  .map((p, i) => platforms[i].bounds.minY - p.bounds.maxY)
  .filter(g => g > 0)
  .sort((a, b) => a - b);
const seamGap = gaps[Math.floor(gaps.length / 2)] ?? 107.5;
const stride =
  platforms[0].bounds.maxY - platforms.at(-1).bounds.minY + seamGap;
const data = {
  ...previous,
  name: 'Mektra_level_1',
  width: extracted.width,
  height: extracted.height,
  stride,
  platforms,
  stars,
};
fs.writeFileSync(
  'src/game/levels/level01.json',
  JSON.stringify(data, null, 2) + '\n',
);
console.log(
  `Imported ${platforms.length} platforms and ${
    stars.length
  } authored stars. Positions preserved; repeat gap ${seamGap.toFixed(2)}.`,
);

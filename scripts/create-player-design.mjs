import fs from 'node:fs';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { ROBOT_LEGS } = require('../src/game/render/robotGeometry.ts');
const states = [
  'Idle',
  'Blink',
  'Focused jump',
  'Happy landing',
  'Surprised',
  'Sleep',
];
function robot(state) {
  let eyes =
    '<rect x="-8" y="-4" width="5" height="7" rx="2"/><rect x="3" y="-4" width="5" height="7" rx="2"/>';
  if (state === 'Blink' || state === 'Sleep')
    eyes =
      '<path d="M-8 0h5M3 0h5" fill="none" stroke="#2585FF" stroke-width="2" stroke-linecap="round"/>';
  if (state === 'Focused jump')
    eyes =
      '<rect x="-8" y="-2" width="5" height="4" rx="1.5"/><rect x="3" y="-2" width="5" height="4" rx="1.5"/>';
  if (state === 'Happy landing')
    eyes =
      '<path d="M-8 0Q-5.5 -5 -3 0M3 0Q5.5 -5 8 0" fill="none" stroke="#2585FF" stroke-width="2" stroke-linecap="round"/>';
  if (state === 'Surprised')
    eyes = '<circle cx="-5.5" cy="-1" r="3"/><circle cx="5.5" cy="-1" r="3"/>';
  return `<g id="robot-${state.replaceAll(
    ' ',
    '-',
  )}"><path id="triangle-legs" d="${ROBOT_LEGS}" fill="#FFFFFF" stroke="#8DA0B8" stroke-width=".5" stroke-linejoin="round"/><circle id="body" r="14" fill="#FFFFFF" stroke="#8DA0B8" stroke-width="1"/><path d="M-8 -10Q0 -14 8 -10" fill="none" stroke="#DAE8F8" stroke-width=".8"/><rect id="rounded-face" x="-10" y="-9" width="20" height="18" rx="5" fill="#152236" stroke="#8DA0B8" stroke-width=".7"/><g id="eyes-${state}" fill="#2585FF">${eyes}</g><path d="M-2 5h4" stroke="#2585FF" stroke-width="1" stroke-linecap="round"/><circle cx="0" cy="11" r="1" fill="#2585FF"/></g>`;
}
const cards = states
  .map((state, i) => {
    const x = 40 + (i % 3) * 290,
      y = 140 + Math.floor(i / 3) * 300;
    return `<g id="state-${i}" transform="translate(${x} ${y})"><rect width="270" height="280" rx="20" fill="#F8FAFC"/><g transform="translate(135 120) scale(5)">${robot(
      state,
    )}</g><text x="135" y="236" text-anchor="middle" font-family="Arial" font-size="18" fill="#303236">${state}</text></g>`;
  })
  .join('');
const board = `<svg xmlns="http://www.w3.org/2000/svg" width="950" height="800" viewBox="0 0 950 800"><rect width="950" height="800" fill="#E2E6EB"/><text x="40" y="60" font-family="Arial" font-size="32" font-weight="bold" fill="#303236">MEKTRA / MINI ROBO</text><text x="40" y="94" font-family="Arial" font-size="16" fill="#536678">Circular shell · 15 rotating triangle legs · rounded face · expressive LED eyes</text>${cards}<text x="40" y="768" font-family="Arial" font-size="14" fill="#536678">Game body: 21 units · leg ring: 26.4 units · upright face</text></svg>`;
fs.writeFileSync('assets/player/Mektra-Mini-Robo.svg', board);
fs.writeFileSync(
  'assets/player/mini-robo-idle.svg',
  `<svg xmlns="http://www.w3.org/2000/svg" viewBox="-18 -18 36 36" width="144" height="144">${robot(
    'Idle',
  )}</svg>`,
);

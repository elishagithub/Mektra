export type Vec = { x: number; y: number };
export type Bounds = { minX: number; maxX: number; minY: number; maxY: number };
export type PlatformDefinition = {
  id: string;
  d: string;
  contours: Vec[][];
  outline: Vec[];
  bounds: Bounds;
};
export type StarDefinition = Vec & { id: string };
export type LevelDefinition = {
  id: string;
  name: string;
  width: number;
  height: number;
  stride: number;
  source: string;
  platforms: PlatformDefinition[];
  stars: StarDefinition[];
};
export type Platform = {
  key: string;
  index: number;
  section: number;
  offset: number;
};
export type Star = StarDefinition & {
  key: string;
  section: number;
  collected: boolean;
};
export type Particle = Vec & {
  kind: 'spark' | 'star';
  vx: number;
  vy: number;
  age: number;
  life: number;
  radius: number;
};
export type Phase = 'ready' | 'playing' | 'paused' | 'exploding' | 'over';
export type Player = Vec & {
  mode: 'orbit' | 'flight';
  platformKey: string;
  distance: number;
  nx: number;
  ny: number;
  vx: number;
  vy: number;
  flightTime: number;
};
export type GameFrame = {
  player: Vec & {
    nx: number;
    ny: number;
    visible: boolean;
    flying: boolean;
    expression: 'idle' | 'focused' | 'happy' | 'surprised';
    gearAngle: number;
  };
  cameraY: number;
  platforms: { index: number; offset: number; active: boolean }[];
  stars: Vec[];
  particles: (Vec & {
    radius: number;
    opacity: number;
    kind: 'spark' | 'star';
    rotation: number;
  })[];
  bursts: (Vec & { radius: number; opacity: number })[];
  trail: Vec[];
  time: number;
};
export type GameEvent = {
  type: 'jump' | 'star' | 'land' | 'crash';
  score: number;
};
export type GameSummary = {
  phase: Phase;
  stars: number;
  distance: number;
  sections: number;
  reason: 'wall' | 'lost' | null;
};

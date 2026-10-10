import type { LegStyle } from '../characters/catalog';
import { GAME } from './config';
import { assistedDirection } from './aim';
import { collectedOnStep, touchesPlatform, touchesWall } from './collision';
import {
  createOrbit,
  nearestOrbit,
  orbitPose,
  type OrbitPath,
  groundedPose,
} from './orbit';
import { spawnSection } from './spawner';
import type {
  GameFrame,
  GameSummary,
  GameEvent,
  LevelDefinition,
  Particle,
  Phase,
  Platform,
  Player,
  Star,
  Vec,
} from './types';

/** Plain simulation data: owned by the UI runtime during gameplay. */
export type SimulationState = {
  phase: Phase;
  player: Player;
  platforms: Platform[];
  stars: Star[];
  particles: Particle[];
  cameraY: number;
  score: number;
  highestSection: number;
  reason: GameSummary['reason'];
  orbits: OrbitPath[];
  accumulator: number;
  time: number;
  deathAge: number;
  nextSection: number;
  viewport: number;
  bestY: number;
  startY: number;
  pausedFrom: Phase;
  trail: Vec[];
  trailTime: number;
  happyUntil: number;
  gearAngle: number;
  legCount: number;
  legStyle: LegStyle;
  events: GameEvent[];
  bursts: (Vec & { age: number })[];
  level: LevelDefinition;
  previous: {
    x: number;
    y: number;
    nx: number;
    ny: number;
    cameraY: number;
    gearAngle: number;
  };
};

// Worklet dependencies precede callers: captured functions must already exist.
export function simulationAddSection(state: SimulationState) {
  'worklet';
  const section = spawnSection(state.level, state.nextSection++);
  state.platforms.push(...section.platforms);
  state.stars.push(...section.stars);
}

export function simulationCapturePose(state: SimulationState) {
  'worklet';
  state.previous = {
    x: state.player.x,
    y: state.player.y,
    nx: state.player.nx,
    ny: state.player.ny,
    cameraY: state.cameraY,
    gearAngle: state.gearAngle,
  };
}

export function createSimulation(level: LevelDefinition): SimulationState {
  'worklet';
  const state: SimulationState = {
    phase: 'ready',
    player: {} as Player,
    platforms: [],
    stars: [],
    particles: [],
    cameraY: 0,
    score: 0,
    highestSection: 0,
    reason: null,
    orbits: level.platforms.map(p => createOrbit(p.outline)),
    accumulator: 0,
    time: 0,
    deathAge: 0,
    nextSection: 0,
    viewport: 700,
    bestY: Infinity,
    startY: 0,
    pausedFrom: 'playing',
    trail: [],
    trailTime: 0,
    happyUntil: 0,
    gearAngle: 0,
    legCount: GAME.legCount,
    legStyle: 'triangle',
    events: [],
    bursts: [],
    level,
    previous: { x: 0, y: 0, nx: 0, ny: -1, cameraY: 0, gearAngle: 0 },
  };

  simulationAddSection(state);
  simulationAddSection(state);
  const path = state.orbits[0];
  const top = path.points.reduce(
    (best, p, i) => (p.y < path.points[best].y ? i : best),
    0,
  );
  const distance = path.cumulative[top],
    pose = groundedPose(
      orbitPose(path, distance),
      state.gearAngle,
      state.legCount,
      state.legStyle,
    );
  state.player = {
    ...pose,
    mode: 'orbit',
    platformKey: state.platforms[0].key,
    distance,
    vx: 0,
    vy: 0,
    flightTime: 0,
  };
  state.startY = state.bestY = state.player.y;
  state.cameraY = state.player.y - state.viewport * GAME.cameraAnchor;

  simulationCapturePose(state);
  return state;
}

export function simulationSetViewport(state: SimulationState, height: number) {
  'worklet';
  state.viewport = height;
  state.cameraY = state.player.y - height * GAME.cameraAnchor;
  simulationCapturePose(state);
}

export function simulationStart(state: SimulationState) {
  'worklet';
  if (state.phase === 'ready') state.phase = 'playing';
}

export function simulationPause(state: SimulationState) {
  'worklet';
  if (state.phase === 'playing' || state.phase === 'exploding') {
    state.pausedFrom = state.phase;
    state.phase = 'paused';
    simulationCapturePose(state);
  }
}

export function simulationResume(state: SimulationState) {
  'worklet';
  if (state.phase === 'paused') state.phase = state.pausedFrom;
}

export function simulationJump(state: SimulationState) {
  'worklet';
  if (state.phase !== 'playing' || state.player.mode !== 'orbit') return;
  const platform = state.platforms.find(
    p => p.key === state.player.platformKey,
  );
  const dir = platform
    ? assistedDirection(
        state.level,
        platform.index,
        state.player,
        { x: state.player.nx, y: state.player.ny },
        platform.offset,
      )
    : { x: state.player.nx, y: state.player.ny };
  state.player.mode = 'flight';
  state.player.vx = dir.x * GAME.jumpSpeed;
  state.player.vy = dir.y * GAME.jumpSpeed;
  state.player.flightTime = 0;
  state.events.push({ type: 'jump', score: state.score });
}

export function simulationUpdateParticles(state: SimulationState, dt: number) {
  'worklet';
  for (const p of state.particles) {
    p.age += dt;
    p.x += p.vx * dt;
    p.y += p.vy * dt;
    p.vx *= Math.exp(-3 * dt);
    p.vy *= Math.exp(-3 * dt);
  }
  state.particles = state.particles.filter(p => p.age < p.life);
}

export function simulationEmit(
  state: SimulationState,
  position: Vec,
  count: number,
  speed: number,
  kind: Particle['kind'] = 'spark',
) {
  'worklet';
  for (let i = 0; i < count; i++) {
    const a = (i / count) * Math.PI * 2,
      v = speed * (0.45 + (i % 5) * 0.14);
    state.particles.push({
      ...position,
      kind,
      vx: Math.cos(a) * v,
      vy: Math.sin(a) * v,
      age: 0,
      life: 0.6 + (i % 4) * 0.1,
      radius: (kind === 'star' ? 4 : 2) + (i % 3),
    });
  }
  state.particles = state.particles.slice(-GAME.maxParticles);
}

export function simulationDie(
  state: SimulationState,
  reason: GameSummary['reason'],
) {
  'worklet';
  if (state.phase !== 'playing') return;
  state.reason = reason;
  state.phase = 'exploding';
  state.deathAge = 0;
  state.trail = [];
  state.events.push({ type: 'crash', score: state.score });
  simulationEmit(state, state.player, 32, 180);
}

export function simulationStep(state: SimulationState, dt: number) {
  'worklet';
  simulationCapturePose(state);
  state.time += dt;
  state.bursts = state.bursts.filter(b => (b.age += dt) < 0.45);
  simulationUpdateParticles(state, dt);
  if (state.phase === 'exploding') {
    state.deathAge += dt;
    if (state.deathAge >= GAME.deathDuration) state.phase = 'over';
    return;
  }
  if (state.phase !== 'playing') return;
  // Rolling teeth turn with travel speed; using simulation time freezes them on pause.
  state.gearAngle =
    (state.gearAngle + (GAME.orbitSpeed / GAME.playerRadius) * dt) %
    (Math.PI * 2);
  const before = { x: state.player.x, y: state.player.y };
  if (state.player.mode === 'orbit') {
    const platform = state.platforms.find(
      p => p.key === state.player.platformKey,
    );
    if (!platform) {
      simulationDie(state, 'lost');
      return;
    }
    const path = state.orbits[platform.index];
    state.player.distance =
      (state.player.distance + GAME.orbitSpeed * dt) % path.length;
    const pose = groundedPose(
      orbitPose(path, state.player.distance),
      state.gearAngle,
      state.legCount,
      state.legStyle,
    );
    Object.assign(state.player, pose, { y: pose.y + platform.offset });
  } else {
    state.player.x += state.player.vx * dt;
    state.player.y += state.player.vy * dt;
    state.player.flightTime += dt;
    // Fixed 120 Hz steps travel 7.2 units: narrower than the player's 26.4-unit diameter.
    for (const platform of state.platforms) {
      if (
        platform.key === state.player.platformKey &&
        state.player.flightTime < 0.16
      )
        continue;
      if (
        !touchesPlatform(
          state.player,
          state.level.platforms[platform.index],
          platform.offset,
        )
      )
        continue;
      const path = state.orbits[platform.index];
      const nearest = nearestOrbit(path, {
        x: state.player.x,
        y: state.player.y - platform.offset,
      });
      const pose = groundedPose(
        orbitPose(path, nearest.along),
        state.gearAngle,
        state.legCount,
        state.legStyle,
      );
      Object.assign(state.player, pose, {
        y: pose.y + platform.offset,
        distance: nearest.along,
        platformKey: platform.key,
        mode: 'orbit',
        vx: 0,
        vy: 0,
      });
      state.highestSection = Math.max(state.highestSection, platform.section);
      state.happyUntil = state.time + 0.55;
      state.events.push({ type: 'land', score: state.score });
      simulationEmit(state, state.player, 8, 70);
      break;
    }
  }
  if (touchesWall(state.player)) {
    simulationDie(state, 'wall');
    return;
  }
  if (
    state.player.y > state.cameraY + state.viewport + 50 ||
    (state.player.flightTime > GAME.maxFlightSeconds &&
      state.player.mode === 'flight')
  ) {
    simulationDie(state, 'lost');
    return;
  }
  for (const star of state.stars) {
    if (!star.collected && collectedOnStep(star, before, state.player)) {
      star.collected = true;
      state.score++;
      state.happyUntil = state.time + 0.55;
      state.events.push({ type: 'star', score: state.score });
      state.bursts.push({ x: star.x, y: star.y, age: 0 });
      state.bursts = state.bursts.slice(-4);
      simulationEmit(state, star, 12, 170, 'star');
    }
  }
  state.bestY = Math.min(state.bestY, state.player.y);
  const target = state.bestY - state.viewport * GAME.cameraAnchor;
  state.cameraY += (target - state.cameraY) * (1 - Math.exp(-8 * dt));
  // Keep two authored sections ahead, then discard old sections as a unit.
  const current =
    state.platforms.find(p => p.key === state.player.platformKey)?.section ??
    state.highestSection;
  if (state.nextSection <= current + 2) simulationAddSection(state);
  const oldest = Math.max(0, current - 1);
  state.platforms = state.platforms.filter(p => p.section >= oldest);
  state.stars = state.stars.filter(s => s.section >= oldest);
  state.trailTime += dt;
  if (state.trailTime >= GAME.trailInterval) {
    state.trailTime -= GAME.trailInterval;
    state.trail.push({ x: state.player.x, y: state.player.y });
    if (state.trail.length > GAME.trailSamples) state.trail.shift();
  }
}

export function simulationAdvance(state: SimulationState, delta: number) {
  'worklet';
  if (state.phase !== 'playing' && state.phase !== 'exploding') return;
  // Limit accumulated work after dropped frames; backgrounding never advances the run.
  state.accumulator += Math.max(0, Math.min(delta, GAME.maxFrameDelta));
  while (state.accumulator >= GAME.fixedStep) {
    simulationStep(state, GAME.fixedStep);
    state.accumulator -= GAME.fixedStep;
  }
}

export function simulationSummary(state: SimulationState): GameSummary {
  'worklet';
  return {
    phase: state.phase,
    stars: state.score,
    distance: Math.floor((state.startY - state.bestY) / 10),
    sections: state.highestSection + 1,
    reason: state.reason,
  };
}

export function simulationDrainEvents(state: SimulationState): GameEvent[] {
  'worklet';
  const events = state.events;
  state.events = [];
  return events;
}

export function simulationFrame(
  state: SimulationState,
  interpolate = false,
): GameFrame {
  'worklet';
  const top = state.cameraY - 160,
    bottom = state.cameraY + state.viewport + 160;
  const platform = state.platforms.find(
    p => p.key === state.player.platformKey,
  );
  const aim =
    platform && state.player.mode === 'orbit'
      ? assistedDirection(
          state.level,
          platform.index,
          state.player,
          { x: state.player.nx, y: state.player.ny },
          platform.offset,
        )
      : { x: state.player.nx, y: state.player.ny };
  // Render between fixed physics samples; camera and player share the same clock.
  const alpha =
    interpolate && (state.phase === 'playing' || state.phase === 'exploding')
      ? Math.max(0, Math.min(1, state.accumulator / GAME.fixedStep))
      : 1;
  const prior = state.previous;
  const cameraY = prior.cameraY + (state.cameraY - prior.cameraY) * alpha;
  const gearDelta =
    ((state.gearAngle - prior.gearAngle + Math.PI * 3) % (Math.PI * 2)) -
    Math.PI;
  return {
    player: {
      x: prior.x + (state.player.x - prior.x) * alpha,
      y: prior.y + (state.player.y - prior.y) * alpha,
      nx: aim.x,
      ny: aim.y,
      visible: state.phase !== 'exploding' && state.phase !== 'over',
      flying: state.player.mode === 'flight',
      expression:
        state.time < state.happyUntil
          ? 'happy'
          : state.player.mode === 'flight' &&
            (state.player.x < 45 || state.player.x > GAME.width - 45)
          ? 'surprised'
          : state.player.mode === 'flight'
          ? 'focused'
          : 'idle',
      gearAngle: prior.gearAngle + gearDelta * alpha,
    },
    cameraY,
    time: state.time,
    platforms: state.platforms
      .filter(p => {
        const b = state.level.platforms[p.index].bounds;
        return b.maxY + p.offset >= top && b.minY + p.offset <= bottom;
      })
      .slice(0, GAME.maxVisiblePlatforms)
      .map(p => ({
        index: p.index,
        offset: p.offset,
        active: p.key === state.player.platformKey,
      })),
    stars: state.stars
      .filter(s => !s.collected && s.y >= top && s.y <= bottom)
      .slice(0, GAME.maxVisibleStars)
      .map(s => ({ x: s.x, y: s.y })),
    particles: state.particles.map(p => ({
      x: p.x,
      y: p.y,
      radius: p.radius,
      opacity: Math.max(0, 1 - p.age / p.life),
      kind: p.kind,
      rotation: Math.atan2(p.vy, p.vx) + p.age * 5,
    })),
    bursts: state.bursts.map(b => ({
      x: b.x,
      y: b.y,
      radius: 8 + b.age * 65,
      opacity: 1 - b.age / 0.45,
    })),
    trail: [...state.trail],
  };
}

export function simulationSetCharacter(
  state: SimulationState,
  legCount: number,
  legStyle: LegStyle,
) {
  'worklet';
  state.legCount = legCount;
  state.legStyle = legStyle;
  if (state.player.mode === 'orbit') {
    const platform = state.platforms.find(
      p => p.key === state.player.platformKey,
    );
    if (platform) {
      const pose = groundedPose(
        orbitPose(state.orbits[platform.index], state.player.distance),
        state.gearAngle,
        legCount,
        legStyle,
      );
      Object.assign(state.player, pose, { y: pose.y + platform.offset });
    }
  }
  simulationCapturePose(state);
}

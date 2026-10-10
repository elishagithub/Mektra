import {
  createSimulation,
  type SimulationState,
  simulationSetViewport,
  simulationStart,
  simulationPause,
  simulationResume,
  simulationJump,
  simulationAdvance,
  simulationSummary,
  simulationDrainEvents,
  simulationFrame,
} from './simulation';
import type {
  LevelDefinition,
  GameFrame,
  GameSummary,
  GameEvent,
} from './types';

/** JS facade for tests and authoring tools; the live app owns plain state on the UI thread. */
export class GameEngine {
  private state: SimulationState;
  constructor(level: LevelDefinition) {
    this.state = createSimulation(level);
  }
  get phase() {
    return this.state.phase;
  }
  set phase(value: SimulationState['phase']) {
    this.state.phase = value;
  }
  get player() {
    return this.state.player;
  }
  set player(value: SimulationState['player']) {
    this.state.player = value;
  }
  get platforms() {
    return this.state.platforms;
  }
  set platforms(value: SimulationState['platforms']) {
    this.state.platforms = value;
  }
  get stars() {
    return this.state.stars;
  }
  set stars(value: SimulationState['stars']) {
    this.state.stars = value;
  }
  get particles() {
    return this.state.particles;
  }
  set particles(value: SimulationState['particles']) {
    this.state.particles = value;
  }
  get cameraY() {
    return this.state.cameraY;
  }
  set cameraY(value: SimulationState['cameraY']) {
    this.state.cameraY = value;
  }
  get score() {
    return this.state.score;
  }
  set score(value: SimulationState['score']) {
    this.state.score = value;
  }
  get highestSection() {
    return this.state.highestSection;
  }
  set highestSection(value: SimulationState['highestSection']) {
    this.state.highestSection = value;
  }
  get reason() {
    return this.state.reason;
  }
  set reason(value: SimulationState['reason']) {
    this.state.reason = value;
  }
  get orbits() {
    return this.state.orbits;
  }
  get level() {
    return this.state.level;
  }
  setViewport(height: number) {
    return simulationSetViewport(this.state, height);
  }
  start() {
    return simulationStart(this.state);
  }
  pause() {
    return simulationPause(this.state);
  }
  resume() {
    return simulationResume(this.state);
  }
  jump() {
    return simulationJump(this.state);
  }
  advance(delta: number) {
    return simulationAdvance(this.state, delta);
  }
  summary(): GameSummary {
    return simulationSummary(this.state);
  }
  drainEvents(): GameEvent[] {
    return simulationDrainEvents(this.state);
  }
  frame(): GameFrame {
    return simulationFrame(this.state);
  }
}

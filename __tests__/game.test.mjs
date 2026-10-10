import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { GameEngine } = require('../src/game/engine/GameEngine.ts');
const { GAME } = require('../src/game/engine/config.ts');
const { orbitPose } = require('../src/game/engine/orbit.ts');
const { level01 } = require('../src/game/levels/index.ts');
const plans = require('../assets/levels/jump-plan.json');
function putOnPlatform(engine, index, section = 0) {
  const platform = engine.platforms.find(
    p => p.index === index && p.section === section,
  );
  assert.ok(platform);
  const plan = plans[index],
    pose = orbitPose(engine.orbits[index], plan.distance);
  Object.assign(engine.player, pose, {
    y: pose.y + platform.offset,
    mode: 'orbit',
    platformKey: platform.key,
    distance: plan.distance,
    flightTime: 0,
  });
  engine.setViewport(750);
}
function tick(engine, seconds) {
  for (let i = 0; i < seconds * 120; i++) engine.advance(GAME.fixedStep);
}
for (let index = 0; index < level01.platforms.length; index++)
  test(`authored jump ${index + 1} lands and collects its stars`, () => {
    const engine = new GameEngine(level01);
    engine.start();
    putOnPlatform(engine, index);
    engine.jump();
    for (
      let i = 0;
      i < 240 && engine.player.mode === 'flight' && engine.phase === 'playing';
      i++
    )
      engine.advance(GAME.fixedStep);
    assert.equal(engine.phase, 'playing');
    assert.equal(engine.player.mode, 'orbit');
    assert.equal(
      engine.player.platformKey,
      index === 28 ? '1:p:0' : `0:p:${index + 1}`,
    );
    assert.ok(engine.score > 0);
  });
for (const x of [GAME.playerRadius - 1, GAME.width - GAME.playerRadius + 1])
  test(`wall contact at x=${x} explodes before game over`, () => {
    const engine = new GameEngine(level01);
    engine.start();
    Object.assign(engine.player, { mode: 'flight', x, vx: 0, vy: 0 });
    engine.advance(GAME.fixedStep);
    assert.equal(engine.phase, 'exploding');
    assert.equal(engine.reason, 'wall');
    assert.ok(engine.particles.length > 0);
    tick(engine, 1);
    assert.equal(engine.phase, 'over');
    assert.equal(engine.frame().player.visible, false);
  });
test('pause freezes movement until deliberate resume', () => {
  const engine = new GameEngine(level01);
  engine.start();
  tick(engine, 0.1);
  engine.pause();
  const before = { ...engine.player };
  const gearBefore = engine.frame().player.gearAngle;
  tick(engine, 2);
  assert.deepEqual(engine.player, before);
  assert.equal(engine.frame().player.gearAngle, gearBefore);
  engine.resume();
  tick(engine, 0.1);
  assert.notEqual(engine.player.distance, before.distance);
  assert.notEqual(engine.frame().player.gearAngle, gearBefore);
});
test('eight repeat seams land and reclaim old sections', () => {
  const engine = new GameEngine(level01);
  engine.start();
  for (let section = 0; section < 8; section++) {
    putOnPlatform(engine, 28, section);
    engine.jump();
    for (let i = 0; i < 240 && engine.player.mode === 'flight'; i++)
      engine.advance(GAME.fixedStep);
    assert.equal(engine.player.platformKey, `${section + 1}:p:0`);
    assert.ok(engine.platforms.length <= 29 * 4);
    assert.ok(engine.stars.length <= level01.stars.length * 4);
  }
});
test('star collection awards once', () => {
  const engine = new GameEngine(level01);
  engine.start();
  const p = engine.player;
  engine.stars = [
    { id: 'test', key: 'test', section: 0, x: p.x, y: p.y, collected: false },
  ];
  engine.advance(GAME.fixedStep);
  assert.equal(engine.score, 1);
  assert.equal(engine.frame().player.expression, 'happy');
  assert.equal(engine.frame().bursts.length, 1);
  assert.ok(
    engine.frame().particles.some(particle => particle.kind === 'star'),
  );
  assert.deepEqual(
    engine.drainEvents().map(e => e.type),
    ['star'],
  );
  assert.deepEqual(engine.drainEvents(), []);
  tick(engine, 0.1);
  assert.equal(engine.score, 1);
});
test('swept star collection catches a star between positions', () => {
  const { collectedOnStep } = require('../src/game/engine/collision.ts');
  assert.equal(
    collectedOnStep({ x: 100, y: 100 }, { x: 0, y: 100 }, { x: 200, y: 100 }),
    true,
  );
});

test('render interpolation fills time between fixed physics steps', () => {
  const {
    createSimulation,
    simulationStart,
    simulationAdvance,
    simulationFrame,
  } = require('../src/game/engine/simulation.ts');
  const state = createSimulation(level01);
  simulationStart(state);
  simulationAdvance(state, GAME.fixedStep);
  const before = simulationFrame(state, true).player;
  simulationAdvance(state, GAME.fixedStep / 2);
  const middle = simulationFrame(state, true).player;
  const current = simulationFrame(state).player;
  assert.ok(Math.hypot(middle.x - before.x, middle.y - before.y) > 0);
  assert.ok(Math.abs(middle.x - (before.x + current.x) / 2) < 1e-8);
  assert.ok(Math.abs(middle.y - (before.y + current.y) / 2) < 1e-8);
});

test('rotating gear tips contact a flat shape without body penetration', () => {
  const {
    gearContactRadius,
    groundedPose,
  } = require('../src/game/engine/orbit.ts');
  const { CHARACTERS } = require('../src/game/characters/catalog.ts');
  for (const character of CHARACTERS) {
    for (let angle = 0; angle < Math.PI * 2; angle += 0.07) {
      const contact = gearContactRadius(
        0,
        -1,
        angle,
        character.legCount,
        character.legStyle,
      );
      const pose = groundedPose(
        { x: 100, y: -GAME.playerRadius, nx: 0, ny: -1 },
        angle,
        character.legCount,
        character.legStyle,
      );
      assert.ok(Math.abs(pose.y + contact) < 1e-8);
      assert.ok(
        contact > 14 * GAME.playerScale,
        'body must remain above the shape',
      );
    }
  }
});

const { CHARACTERS: skins } = require('../src/game/characters/catalog.ts');
for (const character of skins)
  test(`${character.name} retains every authored jump and star lane`, () => {
    const {
      createSimulation,
      simulationSetCharacter,
      simulationSetViewport,
      simulationStart,
      simulationJump,
      simulationAdvance,
    } = require('../src/game/engine/simulation.ts');
    for (let index = 0; index < plans.length; index++) {
      const state = createSimulation(level01);
      Object.assign(
        state.player,
        orbitPose(state.orbits[index], plans[index].distance),
        {
          platformKey: `0:p:${index}`,
          distance: plans[index].distance,
          mode: 'orbit',
        },
      );
      simulationSetViewport(state, 750);
      simulationSetCharacter(state, character.legCount, character.legStyle);
      simulationStart(state);
      simulationJump(state);
      for (
        let step = 0;
        step < 240 &&
        state.player.mode === 'flight' &&
        state.phase === 'playing';
        step++
      )
        simulationAdvance(state, GAME.fixedStep);
      assert.equal(state.phase, 'playing');
      assert.equal(
        state.player.platformKey,
        index === plans.length - 1 ? '1:p:0' : `0:p:${index + 1}`,
      );
      assert.ok(state.score > 0);
    }
  });

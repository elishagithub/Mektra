import Sound from 'react-native-sound';
import type { GameEvent } from '../game/engine/types';

type Effect = GameEvent['type'] | 'hello';
const FILES: Record<Effect, string> = {
  jump: 'mektra_jump.wav',
  star: 'mektra_star.wav',
  land: 'mektra_land.wav',
  crash: 'mektra_crash.wav',
  hello: 'mektra_hello.wav',
};
/** Preloaded native players: no network or decode work during a jump/collection. */
export class GameAudio {
  private enabled = true;
  private pools = new Map<Effect, Sound[]>();
  private disposed = false;
  constructor() {
    // Respect the iPhone silent switch and mix politely with the user's music.
    Sound.setCategory('Ambient', true);
    for (const type of Object.keys(FILES) as Effect[]) {
      const pool: Sound[] = [];
      for (
        let i = 0;
        i < (type === 'star' ? 3 : type === 'jump' ? 2 : 1);
        i++
      ) {
        const sound = new Sound(FILES[type], Sound.MAIN_BUNDLE, error => {
          if (error) {
            console.warn(`Could not preload ${FILES[type]}`, error);
            return;
          }
          if (this.disposed) sound.release();
          else sound.setVolume(type === 'crash' ? 0.45 : 0.6);
        });
        pool.push(sound);
      }
      this.pools.set(type, pool);
    }
  }
  setEnabled(enabled: boolean) {
    this.enabled = enabled;
    if (!enabled) this.stop();
  }
  play(type: Effect) {
    if (!this.enabled || this.disposed) return;
    const sound = this.pools
      .get(type)
      ?.find(s => s.isLoaded() && !s.isPlaying());
    if (sound) {
      sound.setCurrentTime(0);
      sound.play();
    }
  }
  playEvents(events: GameEvent[]) {
    for (const event of events) this.play(event.type);
  }
  stop() {
    for (const pool of this.pools.values())
      for (const sound of pool) if (sound.isLoaded()) sound.stop();
  }
  release() {
    this.disposed = true;
    for (const pool of this.pools.values())
      for (const sound of pool) sound.release();
    this.pools.clear();
  }
}

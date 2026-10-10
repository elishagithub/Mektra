import { CHARACTERS } from '../game/characters/catalog';
export { CHARACTERS } from '../game/characters/catalog';
import AsyncStorage from '@react-native-async-storage/async-storage';
export type Progress = {
  bank: number;
  best: number;
  selected: string;
  unlocked: string[];
};
const KEY = 'mektra.progress.v1';
export const DEFAULT_PROGRESS: Progress = {
  bank: 0,
  best: 0,
  selected: 'mektra',
  unlocked: ['mektra'],
};
export async function loadProgress(): Promise<Progress> {
  try {
    const raw = await AsyncStorage.getItem(KEY);
    if (!raw) return DEFAULT_PROGRESS;
    const p = JSON.parse(raw);
    if (
      !Number.isFinite(p.bank) ||
      p.bank < 0 ||
      !Number.isFinite(p.best) ||
      !Array.isArray(p.unlocked)
    )
      return DEFAULT_PROGRESS;
    const unlocked = p.unlocked.filter((id: unknown) =>
      CHARACTERS.some(c => c.id === id),
    );
    if (!unlocked.includes('mektra')) unlocked.push('mektra');
    return {
      bank: Math.floor(p.bank),
      best: Math.max(0, Math.floor(p.best)),
      unlocked,
      selected: unlocked.includes(p.selected) ? p.selected : 'mektra',
    };
  } catch {
    return DEFAULT_PROGRESS;
  }
}
export async function saveProgress(progress: Progress) {
  await AsyncStorage.setItem(KEY, JSON.stringify(progress));
}

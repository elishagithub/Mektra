import { getCharacter } from './src/game/characters/catalog';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StatusBar,
  StyleSheet,
  Text,
  View,
  Vibration,
} from 'react-native';
import {
  SafeAreaProvider,
  useSafeAreaInsets,
} from 'react-native-safe-area-context';
import { GAME } from './src/game/engine/config';
import { useGame } from './src/game/hooks/useGame';
import { GameCanvas } from './src/game/render/GameCanvas';
import {
  CHARACTERS,
  DEFAULT_PROGRESS,
  loadProgress,
  saveProgress,
  type Progress,
} from './src/storage/progress';
import { Button } from './src/ui/Button';
import { CharacterShop } from './src/ui/CharacterShop';
import { GameOverlay, GameOver } from './src/ui/GameOverlay';
import { colors } from './src/ui/theme';
function Mektra() {
  const insets = useSafeAreaInsets();
  const [size, setSize] = useState({ width: 390, height: 844 });
  const [progress, setProgress] = useState<Progress>(DEFAULT_PROGRESS);
  const character = getCharacter(progress.selected);
  const game = useGame(size.height / (size.width / GAME.width), character);
  const [loaded, setLoaded] = useState(false),
    [shop, setShop] = useState(false),
    [saveError, setSaveError] = useState(false);
  const settled = useRef(false),
    progressRef = useRef(progress),
    saving = useRef(Promise.resolve());
  const commit = useCallback((next: Progress) => {
    progressRef.current = next;
    setProgress(next);
    saving.current = saving.current
      .catch(() => {})
      .then(() => saveProgress(next))
      .then(() => setSaveError(false))
      .catch(() => setSaveError(true));
  }, []);
  useEffect(() => {
    let alive = true;
    loadProgress().then(p => {
      if (alive) {
        progressRef.current = p;
        setProgress(p);
        setLoaded(true);
      }
    });
    return () => {
      alive = false;
    };
  }, []);
  useEffect(() => {
    if (game.summary.phase !== 'over') {
      settled.current = false;
      return;
    }
    if (settled.current) return;
    settled.current = true;
    const p = progressRef.current;
    commit({
      ...p,
      bank: p.bank + game.summary.stars,
      best: Math.max(p.best, game.summary.distance),
    });
  }, [game.summary.phase, game.summary.stars, game.summary.distance, commit]);
  useEffect(() => {
    if (game.summary.phase === 'exploding') Vibration.vibrate(35);
  }, [game.summary.phase]);
  const choose = (id: string) => {
    const c = CHARACTERS.find(x => x.id === id);
    if (!c) return;
    const p = progressRef.current,
      owned = p.unlocked.includes(id);
    if (!owned && p.bank < c.cost) return;
    commit({
      ...p,
      selected: id,
      bank: p.bank - (owned ? 0 : c.cost),
      unlocked: owned ? p.unlocked : [...p.unlocked, id],
    });
  };
  const playing = game.summary.phase === 'playing';
  return (
    <View style={styles.root} onLayout={e => setSize(e.nativeEvent.layout)}>
      <StatusBar barStyle="dark-content" backgroundColor={colors.background} />
      <GameCanvas
        scene={game.scene}
        width={size.width}
        height={size.height}
        character={character}
      />
      {playing && (
        <Pressable
          style={StyleSheet.absoluteFill}
          onPressIn={game.jump}
          accessibilityRole="button"
          accessibilityLabel="Jump outward from the current shape"
        />
      )}
      <View
        pointerEvents="box-none"
        style={[styles.hud, { top: insets.top + 14 }]}
      >
        <View>
          <Text style={styles.brand}>mektra</Text>
          <Text style={styles.distance}>
            {game.summary.distance}m · ORBIT {game.summary.sections}
          </Text>
        </View>
        <View style={styles.hudRight}>
          <Pressable
            onPress={game.toggleSound}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel={
              game.soundEnabled ? 'Mute game sounds' : 'Enable game sounds'
            }
          >
            <Text style={styles.pauseText}>
              {game.soundEnabled ? '♪' : '♪ off'}
            </Text>
          </Pressable>
          <Text style={styles.score}>
            ✦{' '}
            {game.summary.phase === 'ready'
              ? progress.bank
              : game.summary.stars}
          </Text>
          {playing && (
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Pause game"
              onPress={game.pause}
              hitSlop={12}
              style={styles.pause}
            >
              <Text style={styles.pauseText}>Ⅱ</Text>
            </Pressable>
          )}
        </View>
      </View>
      {playing && game.summary.distance < 20 && (
        <Text
          pointerEvents="none"
          style={[styles.hint, { bottom: insets.bottom + 30 }]}
        >
          WAIT FOR YOUR OPENING · TAP TO JUMP
        </Text>
      )}
      {!loaded && (
        <View style={styles.loading}>
          <ActivityIndicator color={colors.ink} />
        </View>
      )}
      {loaded && game.summary.phase === 'ready' && !shop && (
        <GameOverlay
          eyebrow="AN ENDLESS LITTLE WORLD"
          title="Find your orbit"
          description="Walk the curves. Tap to launch yourself toward the next shape. Catch the stars. Keep clear of both side walls."
        >
          <Button label="Let’s play" onPress={game.start} />
          <Button label="Characters" secondary onPress={() => setShop(true)} />
          <Text style={styles.best}>
            BEST {progress.best}m · {progress.bank} STARS
          </Text>
        </GameOverlay>
      )}
      {game.summary.phase === 'paused' && (
        <GameOverlay
          eyebrow="TAKE A BREATH"
          title="Paused"
          description="Your orbit is waiting."
        >
          <Button label="Continue" onPress={game.resume} />
          <Button label="Restart" secondary onPress={game.restart} />
          <Button label="Home" secondary onPress={game.home} />
        </GameOverlay>
      )}
      {game.summary.phase === 'over' && (
        <GameOver
          stars={game.summary.stars}
          distance={game.summary.distance}
          best={progress.best}
          reason={game.summary.reason}
          onRetry={game.restart}
          onHome={game.home}
        />
      )}
      {shop && (
        <CharacterShop
          progress={progress}
          onChoose={choose}
          onClose={() => setShop(false)}
        />
      )}
      {saveError && (
        <Text style={[styles.saveError, { bottom: insets.bottom + 8 }]}>
          Progress is in memory; local storage could not be saved.
        </Text>
      )}
    </View>
  );
}
export default function App() {
  return (
    <SafeAreaProvider>
      <Mektra />
    </SafeAreaProvider>
  );
}
const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: colors.background },
  hud: {
    position: 'absolute',
    left: 24,
    right: 24,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  brand: {
    fontSize: 24,
    fontWeight: '700',
    letterSpacing: -1,
    color: colors.ink,
  },
  distance: {
    fontSize: 9,
    letterSpacing: 1.4,
    color: colors.muted,
    marginTop: 5,
  },
  hudRight: { flexDirection: 'row', alignItems: 'center', gap: 18 },
  score: { fontSize: 18, fontWeight: '600', color: colors.ink },
  pause: {
    width: 38,
    height: 38,
    borderRadius: 19,
    backgroundColor: 'rgba(255,255,255,.65)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  pauseText: { fontSize: 18, color: colors.ink },
  hint: {
    position: 'absolute',
    left: 0,
    right: 0,
    textAlign: 'center',
    fontSize: 9,
    letterSpacing: 1.5,
    color: colors.muted,
  },
  best: {
    textAlign: 'center',
    fontSize: 10,
    letterSpacing: 1.2,
    color: colors.muted,
    marginTop: 8,
  },
  loading: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
  },
  saveError: {
    position: 'absolute',
    left: 12,
    right: 12,
    textAlign: 'center',
    fontSize: 11,
    color: '#9B4A39',
  },
});

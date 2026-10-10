import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { Button } from './Button';
import { colors } from './theme';
export function GameOverlay({
  eyebrow,
  title,
  description,
  children,
}: {
  eyebrow: string;
  title: string;
  description: string;
  children: React.ReactNode;
}) {
  return (
    <View style={styles.scrim}>
      <View style={styles.card}>
        <Text style={styles.eyebrow}>{eyebrow}</Text>
        <Text style={styles.title}>{title}</Text>
        <Text style={styles.description}>{description}</Text>
        <View style={styles.actions}>{children}</View>
      </View>
    </View>
  );
}
export function GameOver({
  stars,
  distance,
  reason,
  best,
  onRetry,
  onHome,
}: {
  stars: number;
  distance: number;
  reason: string | null;
  best: number;
  onRetry: () => void;
  onHome: () => void;
}) {
  return (
    <GameOverlay
      eyebrow="ONE MORE ORBIT"
      title="Game over"
      description={
        reason === 'wall'
          ? 'You touched the edge. Find your next opening.'
          : 'You drifted away. Time your next jump.'
      }
    >
      <View style={styles.stats}>
        <View>
          <Text style={styles.value}>{stars}</Text>
          <Text style={styles.label}>STARS</Text>
        </View>
        <View>
          <Text style={styles.value}>{distance}m</Text>
          <Text style={styles.label}>DISTANCE</Text>
        </View>
      </View>
      <Text style={styles.best}>Best distance · {best}m</Text>
      <Button label="Play again" onPress={onRetry} />
      <Button label="Back to home" secondary onPress={onHome} />
    </GameOverlay>
  );
}
const styles = StyleSheet.create({
  scrim: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: 'rgba(226,230,235,0.8)',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 26,
  },
  card: {
    width: '100%',
    maxWidth: 370,
    backgroundColor: 'rgba(255,255,255,0.96)',
    borderRadius: 32,
    padding: 28,
  },
  eyebrow: {
    color: colors.muted,
    fontSize: 10,
    fontWeight: '700',
    letterSpacing: 2,
    textAlign: 'center',
  },
  title: {
    fontSize: 38,
    fontWeight: '700',
    color: colors.ink,
    textAlign: 'center',
    marginTop: 14,
    letterSpacing: -1.5,
  },
  description: {
    fontSize: 14,
    lineHeight: 22,
    color: colors.muted,
    textAlign: 'center',
    marginTop: 12,
  },
  actions: { gap: 12, marginTop: 28 },
  stats: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginBottom: 2,
  },
  value: {
    fontSize: 28,
    fontWeight: '600',
    color: colors.ink,
    textAlign: 'center',
  },
  label: {
    fontSize: 9,
    letterSpacing: 1.5,
    color: colors.muted,
    textAlign: 'center',
    marginTop: 5,
  },
  best: {
    fontSize: 12,
    color: colors.muted,
    textAlign: 'center',
    marginBottom: 12,
  },
});

import React from 'react';
import { FlatList, Pressable, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import {
  CHARACTERS,
  type CharacterDefinition,
} from '../game/characters/catalog';
import type { Progress } from '../storage/progress';
import { Button } from './Button';
import { CharacterPortrait } from './CharacterPortrait';
import { colors } from './theme';

function CharacterCard({
  character,
  progress,
  onChoose,
}: {
  character: CharacterDefinition;
  progress: Progress;
  onChoose: (id: string) => void;
}) {
  const selected = progress.selected === character.id;
  const owned = progress.unlocked.includes(character.id);
  const affordable = owned || progress.bank >= character.cost;
  const label = selected
    ? 'Selected'
    : owned
    ? 'Use character'
    : `Unlock · ✦ ${character.cost}`;
  return (
    <View style={[styles.card, selected && styles.selectedCard]}>
      <View style={styles.badgeRow}>
        <Text style={[styles.badge, selected && styles.selectedBadge]}>
          {selected
            ? 'YOUR CHARACTER'
            : owned
            ? 'UNLOCKED'
            : `✦ ${character.cost} STARS`}
        </Text>
        {selected && <Text style={styles.check}>✓</Text>}
      </View>
      <View style={styles.portrait}>
        <CharacterPortrait character={character} selected={selected} />
      </View>
      <Text style={styles.name}>{character.name}</Text>
      <Text style={styles.tagline}>{character.tagline}</Text>
      <Pressable
        onPress={() => onChoose(character.id)}
        disabled={selected || !affordable}
        accessibilityRole="button"
        accessibilityState={{ selected, disabled: selected || !affordable }}
        accessibilityLabel={
          selected
            ? `${character.name}, selected`
            : owned
            ? `Use ${character.name}`
            : `Unlock ${character.name} for ${character.cost} stars${
                affordable ? '' : ', not enough stars'
              }`
        }
        style={({ pressed }) => [
          styles.action,
          selected && styles.selectedAction,
          !affordable && styles.unaffordable,
          pressed && styles.pressed,
        ]}
      >
        <Text
          style={[styles.actionText, selected && styles.selectedActionText]}
        >
          {label}
        </Text>
      </Pressable>
      {!affordable && (
        <Text style={styles.needMore}>
          {character.cost - progress.bank} more stars to go
        </Text>
      )}
    </View>
  );
}
export function CharacterShop({
  progress,
  onChoose,
  onClose,
}: {
  progress: Progress;
  onChoose: (id: string) => void;
  onClose: () => void;
}) {
  const insets = useSafeAreaInsets();
  return (
    <View style={styles.screen} accessibilityViewIsModal>
      <View style={[styles.topbar, { paddingTop: insets.top + 12 }]}>
        <Pressable
          onPress={onClose}
          accessibilityRole="button"
          accessibilityLabel="Close character selection"
          hitSlop={8}
          style={styles.back}
        >
          <Text style={styles.backGlyph}>‹</Text>
        </Pressable>
        <Text style={styles.brand}>mektra</Text>
        <View style={styles.wallet}>
          <Text style={styles.walletText}>✦ {progress.bank}</Text>
        </View>
      </View>
      <FlatList
        data={CHARACTERS}
        numColumns={2}
        keyExtractor={item => item.id}
        renderItem={({ item }) => (
          <CharacterCard
            character={item}
            progress={progress}
            onChoose={onChoose}
          />
        )}
        extraData={progress}
        style={styles.list}
        contentContainerStyle={[
          styles.content,
          { paddingBottom: insets.bottom + 112 },
        ]}
        columnWrapperStyle={styles.columns}
        showsVerticalScrollIndicator={false}
        initialNumToRender={6}
        windowSize={3}
        ListHeaderComponent={
          <View style={styles.intro}>
            <Text style={styles.eyebrow}>YOUR LITTLE MECHANICAL CREW</Text>
            <Text accessibilityRole="header" style={styles.title}>
              Your characters
            </Text>
            <Text style={styles.description}>
              Collect stars. Find your kind of robot.
            </Text>
          </View>
        }
      />
      <View style={[styles.footer, { paddingBottom: insets.bottom + 14 }]}>
        <Button label="Ready to roll" onPress={onClose} />
      </View>
    </View>
  );
}
const styles = StyleSheet.create({
  screen: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: colors.background,
  },
  topbar: {
    paddingHorizontal: 20,
    paddingBottom: 12,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  back: {
    width: 42,
    height: 42,
    borderRadius: 21,
    backgroundColor: 'rgba(255,255,255,.65)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  backGlyph: { fontSize: 33, lineHeight: 36, color: colors.ink, marginTop: -3 },
  brand: {
    fontSize: 22,
    fontWeight: '700',
    letterSpacing: -1,
    color: colors.ink,
    flex: 1,
  },
  wallet: {
    paddingHorizontal: 15,
    paddingVertical: 10,
    borderRadius: 22,
    backgroundColor: colors.white,
  },
  walletText: { fontSize: 15, fontWeight: '700', color: colors.ink },
  list: { flex: 1 },
  content: { paddingHorizontal: 20 },
  columns: { gap: 14, marginBottom: 14 },
  intro: { paddingTop: 18, paddingBottom: 24 },
  eyebrow: {
    fontSize: 9,
    fontWeight: '600',
    letterSpacing: 1.7,
    color: colors.muted,
  },
  title: {
    fontSize: 32,
    fontWeight: '700',
    letterSpacing: -1.2,
    color: colors.ink,
    marginTop: 10,
  },
  description: {
    fontSize: 13,
    lineHeight: 20,
    color: colors.muted,
    marginTop: 6,
  },
  card: {
    flex: 1,
    borderRadius: 24,
    padding: 12,
    backgroundColor: '#F7FAFD',
    borderWidth: 2,
    borderColor: '#F7FAFD',
    alignItems: 'center',
  },
  selectedCard: { borderColor: '#2585FF', backgroundColor: '#F6FAFF' },
  badgeRow: {
    height: 19,
    width: '100%',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  badge: {
    fontSize: 8,
    fontWeight: '700',
    letterSpacing: 0.65,
    color: colors.muted,
  },
  selectedBadge: { color: '#2585FF' },
  check: { fontSize: 14, fontWeight: '700', color: '#2585FF' },
  portrait: { height: 124, justifyContent: 'center', alignItems: 'center' },
  name: {
    fontSize: 18,
    fontWeight: '700',
    letterSpacing: -0.3,
    color: colors.ink,
  },
  tagline: {
    height: 36,
    textAlign: 'center',
    fontSize: 10,
    lineHeight: 15,
    color: colors.muted,
    marginTop: 4,
    paddingHorizontal: 2,
  },
  action: {
    width: '100%',
    minHeight: 37,
    borderRadius: 19,
    backgroundColor: '#E9EEF5',
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 6,
  },
  actionText: { fontSize: 11, fontWeight: '700', color: colors.ink },
  selectedAction: { backgroundColor: '#2585FF' },
  selectedActionText: { color: colors.white },
  unaffordable: { opacity: 0.55 },
  pressed: { opacity: 0.7 },
  needMore: { fontSize: 8, lineHeight: 12, color: colors.muted, marginTop: 5 },
  footer: {
    position: 'absolute',
    left: 0,
    right: 0,
    bottom: 0,
    paddingTop: 12,
    paddingHorizontal: 20,
    backgroundColor: 'rgba(226,230,235,.96)',
    borderTopWidth: 1,
    borderTopColor: 'rgba(255,255,255,.7)',
  },
});

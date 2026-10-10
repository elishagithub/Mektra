import React from 'react';
import { Pressable, StyleSheet, Text } from 'react-native';
import { colors } from './theme';
export function Button({
  label,
  onPress,
  secondary = false,
  disabled = false,
}: {
  label: string;
  onPress: () => void;
  secondary?: boolean;
  disabled?: boolean;
}) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      disabled={disabled}
      onPress={onPress}
      style={({ pressed }) => [
        styles.button,
        secondary && styles.secondary,
        disabled && styles.disabled,
        pressed && styles.pressed,
      ]}
    >
      <Text style={[styles.text, secondary && styles.secondaryText]}>
        {label}
      </Text>
    </Pressable>
  );
}
const styles = StyleSheet.create({
  button: {
    minHeight: 54,
    borderRadius: 28,
    backgroundColor: colors.ink,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 28,
  },
  secondary: { backgroundColor: colors.white },
  text: { color: colors.white, fontWeight: '600', fontSize: 16 },
  secondaryText: { color: colors.ink },
  disabled: { opacity: 0.4 },
  pressed: { opacity: 0.75 },
});

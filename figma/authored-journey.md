# Authored journey

The game follows journey-order.json in a fixed 84-section sequence. Each non-recovery world presents its four variants in order. Open-stretch variants provide recovery after Star fork, Lobed crossroads, Counterflow and Long leaps. Figma design numbers and existing column positions are preserved; journey section numbers are mapped separately. After section 84, the sequence repeats for endless play.

Testing checkpoint retries restart the same section and layout from its entry platform with full health and a fresh lava timer. Earlier distance origin is retained. Set SectionReplayMode.Enabled=false for production restarts from section one. Random seed and retry count no longer affect layout selection.

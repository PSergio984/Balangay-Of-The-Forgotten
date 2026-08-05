# Balangay Combat

Deck-based combat where a party of heroes each fight with their own deck, hand, and discard pile. Every hero draws a fixed number of normal cards per turn, and may additionally carry earned one-shot ability cards in dedicated hand slots.

## Language

**Special Card**:
A one-shot ability card earned from a mini-boss defeat. It occupies a dedicated slot in a hero's hand, applies its effect when played, and is then consumed permanently. It is never part of the draw/discard cycle.
_Avoid_: Ultra card, reward card, boss card, deck card

**Consumed**:
The permanent removal of a special card upon playing it. A consumed card cannot return, even if the deck recycles discards back into the draw pile. Consumption is matched primarily by `Card.Title == SpecialCardData.CardName`, with reference-equality fallbacks against `CardDataRepresentation` and the playable card data — reference equality alone is unreliable because `SpecialCardData.GetOrCreatePlayableCardData()` may synthesize a new instance.
_Avoid_: Used up, spent, gone

**Special Card Slot**:
A guaranteed extra position in a hero's hand that holds an unplayed special card on that hero's turn. Slots are separate from the normal draw; a hero can hold up to three simultaneously.
_Avoid_: Extra card, bonus card

**Hand Composition**:
The rule that a hero's hand consists of a fixed number of normal card draws plus up to three special card slots. A full hand is the sum of both, not a single flat cap.
_Avoid_: Hand limit, max hand size

**Cooldown**:
A per-card timer that prevents the card from being played again for N rounds after the round it was just played. Set by `CardSystem.OnCardPlayed → CooldownSystem.OnCardPlayed` when no `NO_COOLDOWN` buff is active, and decremented by `CooldownSystem.OnRoundEnd → ReduceCooldownPerformer` once per round (skipping the round a card was just played in).
_Avoid_: Lockout, freeze

**NoCooldown**:
A status effect (party-wide; `StatusEffectType.NO_COOLDOWN`) granted by the Agos special card for `Duration` rounds. It ticks down like any other status effect via `StatusEffectTickSystem.TickStatusEffects`. While active for any combatant: (a) all currently-on-cooldown cards have their `CurrentCooldown` reset to 0 at the moment the buff is applied, and (b) cards played during the buff window do not start a new cooldown. After the duration expires, normal cooldown rules resume.
_Avoid_: No-cd, skip cooldown, infinite cooldown

**Player Name**:
The active session identifier entered on the main menu's name entry panel. Required to start a session — the play button is disabled and the click is rejected while the field is blank. Truncated to 20 characters and trimmed of surrounding whitespace; an empty or whitespace-only submission is *not* coerced to `"Anonymous"`. `SessionNameEntryUI` and `MainMenu` resolve their `GameProgressData` field-first (serialized reference, then the runtime singleton) — equivalent to singleton-first in the shipped scene, where the wired asset is the singleton, but keeps components testable in isolation.
_Avoid_: Display name, username

**New Player**:
The main-menu reset action that clears all `GameProgressData` state (completed maps, unlocks, intro-seen flag, and player name) and re-opens the name entry panel. The panel must show an empty input field, never the previous name. Resolved at runtime through `GameProgressData.Instance`, not a separately-asset-wired serialized field, to guarantee all UI components read from the same ScriptableObject instance.
_Avoid_: Reset, restart

**Name Gate**:
The main-menu rule that the Start button is disabled and `MainMenu.StartSession()` refuses to run until a session player name exists (`GameProgressData.HasPlayerName`). The gate re-evaluates on the `GameProgressData.PlayerNameStateChanged` event, which fires when a name is set and when progress is reset. The Start button may be wired by Inspector or auto-found by GameObject name `"Start"`.
_Avoid_: Login, sign-in

**Fan Cap**:
The `HandView` rule that caps the hand's arc amplitude and edge tilt at `maxFanSpread` (default 0.5) regardless of hand size, so edge cards stay readable. The existing many-cards compression (`maxCardsBeforeCompression`) shrinks the arc further on crowded hands, and `minEdgeCurveValue` (default 0.3) raises the leftmost/rightmost cards so they never sit at the absolute bottom of the bell curve. Edge cards in a 4–6 card hand now tilt ~7.5° instead of 15° and drop ~1/3 as far.
_Avoid_: Fan layout, arc flattening
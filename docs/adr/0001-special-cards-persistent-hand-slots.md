# Special cards are persistent hand slots, not deck cards

Special cards earned from mini-bosses are held in dedicated hand slots separate from the normal card draw, rather than being injected into a hero's deck or shown in a standalone panel. A hero draws their normal 5 cards and additionally carries up to 3 assigned special cards; playing one consumes it permanently so it never recurs. This diverges from the original implementation, which injected the card into the hero's deck at `CardSystem.Setup()` and suffered infinite replays when the discard pile reshuffled.

**Status**: accepted

**Considered Options**:
- *Deck card* (previous): injected into the deck, competes for draw slots, and returns on deck refill — caused the infinite-replay bug.
- *Standalone panel* (`SpecialCardPanelUI`): cards usable outside the hand — rejected because the panel is deactivated and the observed behavior was that specials should live in the hand.

**Consequences**: card draw, hand rendering, and turn-end discard logic must treat special cards as never entering the draw/discard piles; the 1-special-card-per-hero assignment rule in `SpecialCardCollectionData` must be relaxed to allow up to 3 per hero.
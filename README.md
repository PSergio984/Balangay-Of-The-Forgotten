# Balangay Of The Forgotten - Combat Testing

This Unity project manages the map selection UI, level unlocking, and navigation logic for a tactical RPG prototype. It features robust, defensive programming for UI navigation, player movement, and level management.

## Features

- Dynamic map button creation and unlock state management
- Defensive navigation setup for UI Selectables
- Player avatar movement and orientation logic
- Line rendering between map nodes
- Null safety and error logging throughout

## Key Scripts

- `MapSelectManager.cs`: Central manager for map selection, unlocks, navigation, and player movement
- `LevelSelectSystemEventHandler.cs`: Handles UI event system for level selection
- `LineRendererConnector.cs`: Draws lines between map nodes, robust to missing references
- `ZigzagLayoutGroup.cs`: Custom layout for map node arrangement

## Getting Started

1. Open the project in Unity (2021.3+ recommended)
2. Assign all required references in the `MapSelectManager` inspector
3. Run the scene and use the map selection UI to unlock and navigate levels

## Defensive Programming

- All public fields are validated in `Start()`
- Navigation only links to unlocked and valid buttons
- All event handlers check for nulls and log warnings

## Play Online

<a href="https://play.unity.com/en/games/449145e1-31aa-4836-857c-9f87a6017d27/balangay-of-the-forgotten"> Play Here </a>

## Contributing

Pull requests are welcome! Please ensure all new code follows the defensive and robust style of the project.

## License

MIT License

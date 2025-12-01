using UnityEngine;

public static class SceneDatabase
{
    public class Slots
    {
        /* In the provided C# code snippet, `public const string MainMenu = "MainMenu";` is defining a
        constant string variable named `MainMenu` within the `Slots` class. This constant string
        variable has a value of "MainMenu". */
        public const string MainMenu = "MainMenu";
        public const string Session = "Session";
        public const string SessionContent = "SessionContent";
        public const string LoadingScreen = "LoadingScreen";
    }

    public class Scenes
    {
        public const string MainMenu = "MainMenu";
        public const string CharacterSelection = "CharacterSelection";
        public const string MapSelection = "MapSelection";
        public const string LoadingScreen = "LoadingScreen";
        public const string Session = "Session";
        public const string Combat = "Combat";
        public const string Rewards = "Rewards";
    }
}

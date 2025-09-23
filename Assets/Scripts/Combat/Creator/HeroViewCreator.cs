using UnityEngine;

public class HeroViewCreator : Singleton<HeroViewCreator>
{
    /// <summary>
   /// Prefab reference for the enemy view that will be instantiated
   /// </summary>
   /// <remarks>
   /// This is the template used to create all enemy visuals in the game.
   /// Should be a prefab with EnemyView component and all necessary UI elements.
   /// Assign this in the Inspector.
   /// </remarks>
   [SerializeField] private HeroView heroViewPrefab;

    /// <summary>
    /// Creates a new hero view with specified data, position, and rotation
    /// </summary>
    /// <param name="heroData">Data containing hero stats, appearance, and behavior</param>
    /// <param name="position">Where to position the hero on the battlefield</param>
    /// <param name="rotation">What rotation to give the hero</param>
    /// <returns>The created and set up hero view ready for combat</returns>
    /// <remarks>
    /// Creates a new hero from the prefab template and sets it up with all the data 
    /// from the HeroData asset. This includes health, attack power, visual appearance, 
    /// and name. The hero is positioned correctly and ready to participate in combat.
    /// </remarks>
    public HeroView CreateHeroView(HeroData heroData, Vector3 position, Quaternion rotation)
    {
        // Instantiate a new hero view from the prefab at the specified position and rotation
        HeroView heroView = Instantiate(heroViewPrefab, position, rotation);
        // Set up the hero with its data (health, attack, image, name, etc.)
        heroView.Setup(heroData);
        // Return the created and set up hero view
        return heroView;
    }
}

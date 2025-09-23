using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HeroBoardView : MonoBehaviour
{
    /// <summary>
    /// List of positions where enemies can be placed on the battlefield
    /// </summary>
    /// <remarks>
    /// These are Transform objects that define where each enemy should appear.
    /// Set these up in the Inspector to control enemy positioning and spacing.
    /// </remarks>
    [SerializeField] private List<Transform> slots;

    /// <summary>
    /// List of all enemy views currently active on the battlefield
    /// </summary>
    /// <remarks>
    /// Keeps track of every enemy that's been added to the board.
    /// Other systems can use this to find all enemies for targeting, effects, etc.
    /// </remarks>
    public List<HeroView> HeroViews { get; private set; } = new();

    /// <summary>
    /// Adds a new enemy to the battlefield in the next available slot
    /// </summary>
    /// <param name="enemyData">Data containing enemy stats, appearance, and behavior</param>
    /// <remarks>
    /// Creates a new enemy view using the EnemyViewCreator, positions it in the next 
    /// available slot, and adds it to the list of active enemies. The enemy gets 
    /// parented to the slot for proper organization in the hierarchy.
    /// </remarks>
    public void AddHero(HeroData heroData)
    {
        // Get the next available slot based on how many heroes we already have
        Transform slot = slots[HeroViews.Count];
        // Create a new hero view at the slot's position and rotation
        HeroView heroView = HeroViewCreator.Instance.CreateHeroView(heroData, slot.position, slot.rotation);
        // Make the hero a child of the slot for organization
        heroView.transform.parent = slot;
        // Add the new hero to our list of active heroes
        HeroViews.Add(heroView);
    }
    
    /// <summary>
    /// Removes an enemy from the battlefield with smooth scaling animation
    /// </summary>
    /// <param name="enemyView">The enemy view to remove from the battlefield</param>
    /// <returns>Coroutine that completes when the removal animation finishes</returns>
    /// <remarks>
    /// Removes an enemy from the battlefield with a nice visual effect. First removes 
    /// the enemy from the active list, then plays a shrinking animation where the enemy 
    /// scales down to zero over 0.25 seconds. After the animation completes, destroys 
    /// the enemy GameObject. This creates a satisfying death/removal effect.
    /// </remarks>
    public IEnumerator RemoveHero(HeroView heroView)
    {
        // Remove the hero from our active heroes list
        HeroViews.Remove(heroView);
        // Create a scaling animation that shrinks the hero to zero size over 0.25 seconds
        Tween tween = heroView.transform.DOScale(Vector3.zero, 0.25f);
        // Wait for the scaling animation to complete
        yield return tween.WaitForCompletion();
        // Destroy the hero GameObject after the animation finishes
        Destroy(heroView.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Manages the visual representation and positioning of heroes on the battlefield
/// </summary>
public class HeroBoardView : MonoBehaviour
{
    /// <summary>
    /// List of positions where heroes can be placed on the battlefield
    /// </summary>
    [SerializeField] private List<Transform> slots;

    [Header("Spawn Animation")]
    [Tooltip("How far off-screen to start the hero (negative = from left)")]
    [SerializeField] private float spawnOffsetX = -10f;
    
    [Tooltip("Duration of the slide-in animation")]
    [SerializeField] private float slideInDuration = 0.6f;
    
    [Tooltip("Ease type for the slide-in")]
    [SerializeField] private Ease slideInEase = Ease.OutBack;
    
    [Tooltip("Delay between spawning multiple heroes")]
    [SerializeField] private float spawnDelay = 0.2f;

    /// <summary>
    /// List of all hero views currently active on the battlefield
    /// </summary>
    public List<HeroView> HeroViews { get; private set; } = new();

    /// <summary>
    /// Adds a new hero to the battlefield with a slide-in animation from the left
    /// </summary>
    /// <param name="heroData">Data containing hero stats, appearance, and behavior</param>
    public void AddHero(HeroData heroData)
    {
        // Get the next available slot based on how many heroes we already have
        Transform slot = slots[HeroViews.Count];
        
        // Calculate spawn position (off-screen to the left)
        Vector3 targetPosition = slot.position;
        Vector3 spawnPosition = new Vector3(targetPosition.x + spawnOffsetX, targetPosition.y, targetPosition.z);
        
        // Create a new hero view at the off-screen spawn position
        HeroView heroView = HeroViewCreator.Instance.CreateHeroView(heroData, spawnPosition, slot.rotation);
        
        // Make the hero a child of the slot for organization
        heroView.transform.parent = slot;
        
        // Start transparent for fade-in effect
        SpriteRenderer spriteRenderer = heroView.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            startColor.a = 0f;
            spriteRenderer.color = startColor;
        }
        
        // Add the new hero to our list of active heroes
        HeroViews.Add(heroView);
        
        // Play spawn animation
        StartCoroutine(PlaySpawnAnimation(heroView, targetPosition, spriteRenderer));
    }

    /// <summary>
    /// Plays the slide-in spawn animation for a hero
    /// </summary>
    private IEnumerator PlaySpawnAnimation(HeroView heroView, Vector3 targetPosition, SpriteRenderer spriteRenderer)
    {
        // Small delay based on hero index for staggered spawning
        int heroIndex = HeroViews.IndexOf(heroView);
        if (heroIndex > 0)
        {
            yield return new WaitForSeconds(spawnDelay * heroIndex);
        }
        
        // Slide in from left
        heroView.transform.DOMove(targetPosition, slideInDuration).SetEase(slideInEase);
        
        // Fade in
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(1f, slideInDuration * 0.5f);
        }
        
        yield return new WaitForSeconds(slideInDuration);
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

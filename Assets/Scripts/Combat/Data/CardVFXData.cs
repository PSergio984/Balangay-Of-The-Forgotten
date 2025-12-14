using UnityEngine;
using AudioSystem;
using Sirenix.OdinInspector;

/// <summary>
/// ScriptableObject that defines VFX and SFX for a card's effects
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Allows designers to assign unique VFX and SFX to cards</para>
/// 
/// <para><strong>How to use:</strong></para>
/// <list type="bullet">
/// <item>Create via Assets menu: "Data/Card VFX"</item>
/// <item>Assign VFX prefabs for cast, impact, and trail effects</item>
/// <item>Assign SFX for cast, impact, and ambient sounds</item>
/// <item>Reference this from CardData for per-card customization</item>
/// </list>
/// </remarks>
[CreateAssetMenu(menuName = "Data/Card VFX", fileName = "NewCardVFX")]
public class CardVFXData : ScriptableObject
{
    [Title("Cast Effects", "Visual and audio effects when the card is played", TitleAlignments.Centered)]
    
    [Tooltip("VFX spawned at the caster when the card is played")]
    [PreviewField(50)]
    public GameObject castVFXPrefab;
    
    [Tooltip("Sound played when the card is cast")]
    public SoundData castSFX;

    [Title("Impact Effects", "Visual and audio effects when the card hits a target", TitleAlignments.Centered)]
    
    [Tooltip("VFX spawned at each target when the effect hits")]
    [PreviewField(50)]
    public GameObject impactVFXPrefab;
    
    [Tooltip("Sound played when the effect hits a target")]
    public SoundData impactSFX;

    [Title("Trail/Projectile Effects", "Visual effects for projectile-based cards", TitleAlignments.Centered)]
    
    [Tooltip("VFX that travels from caster to target (optional)")]
    [PreviewField(50)]
    public GameObject trailVFXPrefab;
    
    [Tooltip("Speed of the projectile trail (if applicable)")]
    [ShowIf("@trailVFXPrefab != null")]
    [Range(5f, 50f)]
    public float trailSpeed = 20f;

    [Title("Timing", "Control when effects appear during card animation", TitleAlignments.Centered)]
    
    [Tooltip("Delay before spawning cast VFX (from card play start)")]
    [Range(0f, 2f)]
    public float castVFXDelay = 0f;
    
    [Tooltip("Delay before spawning impact VFX (from cast VFX)")]
    [Range(0f, 2f)]
    public float impactVFXDelay = 0.3f;
    
    [Tooltip("Duration before auto-destroying VFX")]
    [Range(0.5f, 5f)]
    public float vfxDuration = 2f;

    [Title("Screen Effects", "Camera and screen effects for impactful cards", TitleAlignments.Centered)]
    
    [Tooltip("Enable screen shake on impact")]
    public bool enableScreenShake = false;
    
    [Tooltip("Screen shake intensity")]
    [ShowIf("enableScreenShake")]
    [Range(0.1f, 2f)]
    public float screenShakeIntensity = 0.5f;
    
    [Tooltip("Screen shake duration")]
    [ShowIf("enableScreenShake")]
    [Range(0.1f, 1f)]
    public float screenShakeDuration = 0.2f;
}


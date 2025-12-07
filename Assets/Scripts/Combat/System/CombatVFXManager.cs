using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

/// <summary>
/// Centralized manager for spawning VFX and playing SFX during combat
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides a unified interface for all visual and audio effects in combat</para>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Spawns particle effects at specific positions</item>
/// <item>Plays sound effects with proper pooling</item>
/// <item>Supports different effect types (damage, heal, buff, debuff, etc.)</item>
/// <item>Optional VFX prefab pooling for performance</item>
/// </list>
/// 
/// <para><strong>Usage:</strong> Call methods like PlayDamageEffect(), PlayHealEffect(), etc.</para>
/// </remarks>
public class CombatVFXManager : Singleton<CombatVFXManager>
{
    [Header("VFX Prefabs")]
    [Tooltip("Default damage hit effect")]
    [SerializeField] private GameObject damageVFXPrefab;
    
    [Tooltip("Healing effect")]
    [SerializeField] private GameObject healVFXPrefab;
    
    [Tooltip("Shield/block effect")]
    [SerializeField] private GameObject shieldVFXPrefab;
    
    [Tooltip("Buff applied effect")]
    [SerializeField] private GameObject buffVFXPrefab;
    
    [Tooltip("Debuff applied effect")]
    [SerializeField] private GameObject debuffVFXPrefab;
    
    [Tooltip("Critical hit effect")]
    [SerializeField] private GameObject critVFXPrefab;
    
    [Tooltip("Miss effect")]
    [SerializeField] private GameObject missVFXPrefab;
    
    [Tooltip("Fire/burn effect")]
    [SerializeField] private GameObject fireVFXPrefab;
    
    [Tooltip("Electric/shock effect")]
    [SerializeField] private GameObject electricVFXPrefab;

    [Header("SFX Data")]
    [Tooltip("Default damage sound")]
    [SerializeField] private SoundData damageSFX;
    
    [Tooltip("Healing sound")]
    [SerializeField] private SoundData healSFX;
    
    [Tooltip("Shield gain sound")]
    [SerializeField] private SoundData shieldSFX;
    
    [Tooltip("Buff applied sound")]
    [SerializeField] private SoundData buffSFX;
    
    [Tooltip("Debuff applied sound")]
    [SerializeField] private SoundData debuffSFX;
    
    [Tooltip("Critical hit sound")]
    [SerializeField] private SoundData critSFX;
    
    [Tooltip("Miss sound")]
    [SerializeField] private SoundData missSFX;
    
    [Tooltip("Fire/burn sound")]
    [SerializeField] private SoundData fireSFX;

    [Header("Settings")]
    [Tooltip("Default duration before destroying VFX")]
    [SerializeField] private float defaultVFXDuration = 2f;
    
    [Tooltip("Enable object pooling for VFX")]
    [SerializeField] private bool usePooling = true;
    
    [Tooltip("Pool size for each VFX type")]
    [SerializeField] private int poolSize = 5;

    // Cached sound builder
    private SoundBuilder soundBuilder;
    
    // VFX pools
    private Dictionary<GameObject, Queue<GameObject>> vfxPools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Start()
    {
        // Initialize sound builder
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        
        // Initialize VFX pools
        if (usePooling)
        {
            InitializePool(damageVFXPrefab);
            InitializePool(healVFXPrefab);
            InitializePool(shieldVFXPrefab);
            InitializePool(buffVFXPrefab);
            InitializePool(debuffVFXPrefab);
            InitializePool(critVFXPrefab);
            InitializePool(missVFXPrefab);
            InitializePool(fireVFXPrefab);
            InitializePool(electricVFXPrefab);
        }
    }

    /// <summary>
    /// Initializes a pool for a specific VFX prefab
    /// </summary>
    private void InitializePool(GameObject prefab)
    {
        if (prefab == null) return;
        
        Queue<GameObject> pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        vfxPools[prefab] = pool;
    }

    /// <summary>
    /// Gets a VFX instance from pool or creates a new one
    /// </summary>
    private GameObject GetVFX(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        
        if (usePooling && vfxPools.ContainsKey(prefab) && vfxPools[prefab].Count > 0)
        {
            GameObject pooled = vfxPools[prefab].Dequeue();
            pooled.transform.position = position;
            pooled.SetActive(true);
            return pooled;
        }
        
        return Instantiate(prefab, position, Quaternion.identity);
    }

    /// <summary>
    /// Returns a VFX instance to its pool
    /// </summary>
    private void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null) return;
        
        if (usePooling && vfxPools.ContainsKey(prefab))
        {
            instance.SetActive(false);
            instance.transform.SetParent(transform);
            vfxPools[prefab].Enqueue(instance);
        }
        else
        {
            Destroy(instance);
        }
    }

    /// <summary>
    /// Spawns a VFX at a position and returns it to pool after duration
    /// </summary>
    private void SpawnVFX(GameObject prefab, Vector3 position, float duration = -1)
    {
        if (prefab == null) return;
        
        if (duration < 0) duration = defaultVFXDuration;
        
        GameObject vfx = GetVFX(prefab, position);
        if (vfx != null)
        {
            StartCoroutine(ReturnVFXAfterDelay(prefab, vfx, duration));
        }
    }

    /// <summary>
    /// Coroutine to return VFX to pool after delay
    /// </summary>
    private IEnumerator ReturnVFXAfterDelay(GameObject prefab, GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(prefab, instance);
    }

    /// <summary>
    /// Plays a sound effect at a position
    /// </summary>
    private void PlaySFX(SoundData soundData, Vector3 position)
    {
        if (soundData == null || soundBuilder == null) return;
        
        soundBuilder.WithPosition(position).Play(soundData);
    }

    // ============================================
    // PUBLIC API - Effect Methods
    // ============================================

    /// <summary>
    /// Plays a damage effect (VFX + SFX) at the target position
    /// </summary>
    public void PlayDamageEffect(Vector3 position, bool isCrit = false)
    {
        if (isCrit)
        {
            SpawnVFX(critVFXPrefab, position);
            PlaySFX(critSFX, position);
        }
        else
        {
            SpawnVFX(damageVFXPrefab, position);
            PlaySFX(damageSFX, position);
        }
    }

    /// <summary>
    /// Plays a miss effect at the target position
    /// </summary>
    public void PlayMissEffect(Vector3 position)
    {
        SpawnVFX(missVFXPrefab, position);
        PlaySFX(missSFX, position);
    }

    /// <summary>
    /// Plays a healing effect at the target position
    /// </summary>
    public void PlayHealEffect(Vector3 position)
    {
        SpawnVFX(healVFXPrefab, position);
        PlaySFX(healSFX, position);
    }

    /// <summary>
    /// Plays a shield/block effect at the target position
    /// </summary>
    public void PlayShieldEffect(Vector3 position)
    {
        SpawnVFX(shieldVFXPrefab, position);
        PlaySFX(shieldSFX, position);
    }

    /// <summary>
    /// Plays a buff applied effect at the target position
    /// </summary>
    public void PlayBuffEffect(Vector3 position)
    {
        SpawnVFX(buffVFXPrefab, position);
        PlaySFX(buffSFX, position);
    }

    /// <summary>
    /// Plays a debuff applied effect at the target position
    /// </summary>
    public void PlayDebuffEffect(Vector3 position)
    {
        SpawnVFX(debuffVFXPrefab, position);
        PlaySFX(debuffSFX, position);
    }

    /// <summary>
    /// Plays a fire/burn effect at the target position
    /// </summary>
    public void PlayFireEffect(Vector3 position)
    {
        SpawnVFX(fireVFXPrefab, position);
        PlaySFX(fireSFX, position);
    }

    /// <summary>
    /// Plays an electric/shock effect at the target position
    /// </summary>
    public void PlayElectricEffect(Vector3 position)
    {
        SpawnVFX(electricVFXPrefab, position);
        // Could add electric SFX here
    }

    /// <summary>
    /// Plays a custom VFX prefab at the target position
    /// </summary>
    public void PlayCustomVFX(GameObject vfxPrefab, Vector3 position, float duration = -1)
    {
        if (vfxPrefab == null) return;
        
        if (duration < 0) duration = defaultVFXDuration;
        
        // For custom VFX, always instantiate (no pooling)
        GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
        Destroy(vfx, duration);
    }

    /// <summary>
    /// Plays a custom sound at the target position
    /// </summary>
    public void PlayCustomSFX(SoundData soundData, Vector3 position)
    {
        PlaySFX(soundData, position);
    }

    /// <summary>
    /// Gets the appropriate effect position for a combatant
    /// </summary>
    public static Vector3 GetEffectPosition(CombatantView combatant)
    {
        if (combatant == null) return Vector3.zero;
        
        SpriteRenderer spriteRenderer = combatant.GetComponentInChildren<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.transform.position : combatant.transform.position;
    }
}


using TMPro;
using UnityEngine;

/// <summary>
/// Popup text display system for damage numbers, status effects, and announcements.
/// Supports multiple animation modes: scaling (default) and fade-only.
/// </summary>
public class DamagePopUp : MonoBehaviour
{
    /// <summary>
    /// Animation mode for the popup display
    /// </summary>
    public enum PopUpAnimationMode
    {
        /// <summary>Default mode - scales up then down before fading</summary>
        ScaleAndFade,
        /// <summary>Simple mode - only moves up and fades out, no scaling</summary>
        FadeOnly
    }

    /// <summary>
    /// Creates a damage popup with the default scaling animation
    /// </summary>
    public static DamagePopUp Create(Vector3 position, int popUpAmount, bool isCrit, bool isMiss)
    {
        return Create(position, popUpAmount, isCrit, isMiss, PopUpAnimationMode.ScaleAndFade);
    }

    /// <summary>
    /// Creates a damage popup with specified animation mode
    /// </summary>
    public static DamagePopUp Create(Vector3 position, int popUpAmount, bool isCrit, bool isMiss, PopUpAnimationMode animationMode)
    {
        if (GameAssets.i.pfDamagePopup == null)
        {
            Debug.LogError("[DamagePopUp] DamagePopUpPrefab not assigned!");
            return null;
        }
        
        // Instantiate at target position first
        GameObject damagePopUpObj = Instantiate(GameAssets.i.pfDamagePopup, position, Quaternion.identity);
        DamagePopUp damagePopUp = damagePopUpObj.GetComponent<DamagePopUp>();
        if (damagePopUp == null)
        {
            Debug.LogWarning("[DamagePopUp] Instantiated prefab is missing the DamagePopUp component. Destroying orphaned GameObject.");
            Destroy(damagePopUpObj);
            return null;
        }
        // Apply horizontal and vertical offset after instantiation
        Vector3 offset = new Vector3(damagePopUp.spawnOffsetX, damagePopUp.spawnOffsetY, 0f);
        damagePopUpObj.transform.position = position + offset;
        damagePopUp.Setup(popUpAmount, isCrit, isMiss, animationMode);
        return damagePopUp;
    }

    /// <summary>
    /// Creates a text-based popup (for status effects, move names, etc.) with fade-only animation
    /// </summary>
    /// <param name="position">World position to spawn the popup</param>
    /// <param name="text">Text to display</param>
    /// <param name="color">Color of the text</param>
    /// <param name="animationMode">Animation style (default: FadeOnly for text announcements)</param>
    /// <param name="scaleMultiplier">Scale multiplier for the popup size (default: 0.7 for smaller text popups)</param>
    public static DamagePopUp CreateTextPopUp(Vector3 position, string text, Color color, PopUpAnimationMode animationMode = PopUpAnimationMode.FadeOnly, float scaleMultiplier = 0.7f)
    {
        if (GameAssets.i.pfDamagePopup == null)
        {
            Debug.LogError("[DamagePopUp] DamagePopUpPrefab not assigned!");
            return null;
        }
        
        GameObject damagePopUpObj = Instantiate(GameAssets.i.pfDamagePopup, position, Quaternion.identity);
        DamagePopUp damagePopUp = damagePopUpObj.GetComponent<DamagePopUp>();
        if (damagePopUp == null)
        {
            Debug.LogWarning("[DamagePopUp] Instantiated prefab is missing the DamagePopUp component.");
            Destroy(damagePopUpObj);
            return null;
        }
        
        Vector3 offset = new Vector3(damagePopUp.spawnOffsetX, damagePopUp.spawnOffsetY, 0f);
        damagePopUpObj.transform.position = position + offset;
        damagePopUp.SetupText(text, color, animationMode, scaleMultiplier);
        return damagePopUp;
    }

    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private Canvas canvas;
    
    [Header("Animation Settings")]
    [Tooltip("Maximum scale multiplier relative to initial scale (e.g., 1.5 = 150% of original size)")]
    [SerializeField] private float maxScaleMultiplier = 1.5f;
    
    [Tooltip("Horizontal offset from target position where popup spawns (world units)")]
    [SerializeField] private float spawnOffsetX = 0f;
    [Tooltip("Vertical offset above target position where popup spawns (world units)")]
    [SerializeField] private float spawnOffsetY = 1.0f;
    
    [Tooltip("Upward movement speed (lower = less movement)")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Fade Only Mode Settings")]
    [Tooltip("Duration for fade-only animation mode")]
    [SerializeField] private float fadeOnlyDuration = 1.2f;
    
    private float disappearTimer;
    private Color textColor;
    private const float DISAPPEAR_TIMER_MAX = 1f;
    private Vector3 moveVector;

    private static int sortingOrder;
    private Vector3 initialScale;
    private float originalFontSize;
    
    /// <summary>
    /// Current animation mode for this popup instance
    /// </summary>
    private PopUpAnimationMode currentAnimationMode = PopUpAnimationMode.ScaleAndFade;

    private void Awake()
    {
        initialScale = transform.localScale;
        // Store original font size for text popup scaling
        if (textMesh != null)
        {
            originalFontSize = textMesh.fontSize;
        }
    }

    /// <summary>
    /// Legacy setup method - uses default ScaleAndFade animation
    /// </summary>
    public void Setup(int popUpAmount, bool isCrit, bool isMiss)
    {
        Setup(popUpAmount, isCrit, isMiss, PopUpAnimationMode.ScaleAndFade);
    }

    /// <summary>
    /// Setup popup with specified animation mode
    /// </summary>
    public void Setup(int popUpAmount, bool isCrit, bool isMiss, PopUpAnimationMode animationMode)
    {
        currentAnimationMode = animationMode;
        
        if (isMiss)
        {
            textMesh.text = "MISS";
            textColor = Color.gray;
        }
        else if (isCrit)
        {
            textMesh.text = popUpAmount.ToString();
            textColor = Color.red;
        }
        else
        {
            textMesh.text = popUpAmount.ToString();
            textColor = Color.white;
        }

        textMesh.color = textColor;

        // Set timer based on animation mode
        disappearTimer = (animationMode == PopUpAnimationMode.FadeOnly) ? fadeOnlyDuration : DISAPPEAR_TIMER_MAX;

        sortingOrder++;
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        moveVector = Vector3.up * moveSpeed;
        transform.localScale = initialScale;
        
        // Reset font size to original for damage numbers
        if (textMesh != null && originalFontSize > 0)
        {
            textMesh.fontSize = originalFontSize;
        }
    }

    /// <summary>
    /// Setup popup with custom text and color (for status effects, move names, etc.)
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="color">Color of the text</param>
    /// <param name="animationMode">Animation style (default: FadeOnly for text announcements)</param>
    /// <param name="scaleMultiplier">Scale multiplier for the popup size (default: 0.7 for smaller text popups)</param>
    public void SetupText(string text, Color color, PopUpAnimationMode animationMode = PopUpAnimationMode.FadeOnly, float scaleMultiplier = 0.1f)
    {
        currentAnimationMode = animationMode;
        
        textMesh.text = text;
        textColor = color;
        textMesh.color = textColor;

        // Set timer based on animation mode
        disappearTimer = (animationMode == PopUpAnimationMode.FadeOnly) ? fadeOnlyDuration : DISAPPEAR_TIMER_MAX;

        sortingOrder++;
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        // Slower movement for text announcements
        moveVector = Vector3.up * (moveSpeed * 0.5f);
        
        // Apply scale multiplier to make text popups smaller than damage numbers
        transform.localScale = initialScale * scaleMultiplier;
        
        // Also reduce font size directly to make text popups smaller
        // This ensures longer text strings don't appear too large
        if (textMesh != null && originalFontSize > 0)
        {
            textMesh.fontSize = originalFontSize * scaleMultiplier;
        }
    }




    private void Update()
    {
        // Move upward with deceleration
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (currentAnimationMode == PopUpAnimationMode.FadeOnly)
        {
            // FADE ONLY MODE: Just move up and fade out, no scaling
            UpdateFadeOnlyMode();
        }
        else
        {
            // SCALE AND FADE MODE: Original behavior with scaling animation
            UpdateScaleAndFadeMode();
        }
    }

    /// <summary>
    /// Simple fade-only animation - moves up and fades out without scaling
    /// Used for status effect procs, move names, and announcements
    /// </summary>
    private void UpdateFadeOnlyMode()
    {
        if (disappearTimer > 0f)
        {
            disappearTimer -= Time.deltaTime;
            
            // Calculate fade progress (start fading after 50% of duration)
            float fadeStartTime = fadeOnlyDuration * 0.5f;
            if (disappearTimer < fadeStartTime)
            {
                float fadeProgress = 1f - (disappearTimer / fadeStartTime);
                textColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                textMesh.color = textColor;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Original scale and fade animation - scales up then down before fading
    /// Used for damage numbers
    /// </summary>
    private void UpdateScaleAndFadeMode()
    {
        // count down scale timer first
        if (disappearTimer > 0f)
        {
            disappearTimer -= Time.deltaTime;

            // scale: 1 -> max -> 0 (small) while timer is > 0
            float halfTime = DISAPPEAR_TIMER_MAX * 0.5f;
            float t;

            if (disappearTimer > halfTime)
            {
                // 1st half: grow 1 -> max
                float norm = (DISAPPEAR_TIMER_MAX - disappearTimer) / halfTime; // 0..1
                t = norm;
            }
            else
            {
                // 2nd half: shrink max -> 0
                float norm = (halfTime - disappearTimer) / halfTime; // 0..1
                t = 1f + norm; // use 1..2 range to distinguish
            }

            float scale;
            if (t <= 1f)
            {
                // grow 1 -> max
                scale = Mathf.Lerp(1f, maxScaleMultiplier, t);
            }
            else
            {
                // shrink max -> 0
                float s = t - 1f; // 0..1
                scale = Mathf.Lerp(maxScaleMultiplier, 0f, s);
            }

            transform.localScale = initialScale * Mathf.Max(scale, 0.01f);
        }
        else
        {
            // fade + destroy AFTER scale animation is done
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    
}

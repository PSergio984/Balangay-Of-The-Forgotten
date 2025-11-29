using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{

    public static DamagePopUp Create(Vector3 position, int popUpAmount, bool isCrit, bool isMiss)
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
        damagePopUp.Setup(popUpAmount, isCrit, isMiss);
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
    
    private float disappearTimer;
    private Color textColor;
    private const float DISAPPEAR_TIMER_MAX = 1f;
    private Vector3 moveVector;

    private static int sortingOrder;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void Setup(int popUpAmount, bool isCrit, bool isMiss)
    {
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

        disappearTimer = DISAPPEAR_TIMER_MAX;

        sortingOrder++;
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        moveVector = Vector3.up * moveSpeed;
        transform.localScale = initialScale;
    }




    private void Update()
    {
        // move
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

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

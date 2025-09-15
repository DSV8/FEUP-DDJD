using System.Collections;
using UnityEngine;

public class GroundPoundEnemy : BaseWalkingEnemy
{
    [Header("Ground Detection")]
    [SerializeField] LayerMask groundLayerMask = 1;

    [Header("Ground Pound Attack")]
    [SerializeField] float telegraphDuration = 1.5f;
    [SerializeField] float jumpDuration = 1.2f;
    [SerializeField] float landingEffectDuration = 0.5f;
    [SerializeField] float telegraphRadius = 4f;
    [SerializeField] int slamDamage = 30;
    
    [Header("Visuals")]
    [SerializeField] Material telegraphMaterial;
    [SerializeField] Color telegraphColor = Color.yellow;
    [SerializeField] Color dangerColor = Color.red;
    [SerializeField] float telegraphAlpha = 0.3f;
    [SerializeField] float dangerAlpha = 0.6f;
    [SerializeField] int circleSegments = 64;
    [SerializeField] float groundOffset = 0.01f;
    [SerializeField] AnimationCurve telegraphExpansionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected int tauntTriggerHash;
    protected int groundPoundTriggerHash;
    
    private GameObject telegraphRoot;
    private MeshRenderer fillMeshRenderer;
    private MeshFilter fillMeshFilter;
    private Material telegraphMatInstance;
    
    private bool isPerformingGroundPound = false;
    private PlayerHealth playerHealth;

    private float damageMultiplier = 1f;

    protected override void Awake()
    {
        base.Awake();

        tauntTriggerHash = Animator.StringToHash("TauntTrigger");
        groundPoundTriggerHash = Animator.StringToHash("GroundPoundTrigger");

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerHealth = playerGO.GetComponent<PlayerHealth>();

        CreateTelegraphVisual();
    }
    
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    protected override IEnumerator PrimaryAttack()
    {
        isPerformingGroundPound = true;

        animator.SetTrigger(tauntTriggerHash);
        StartCoroutine(AnimateTelegraph());
        yield return new WaitForSeconds(telegraphDuration);

        animator.SetTrigger(groundPoundTriggerHash);

        yield return new WaitForSeconds(jumpDuration * 0.85f);

        CheckDamageOnImpact();

        SetTelegraphColor(dangerColor);

        yield return new WaitForSeconds(jumpDuration * 0.15f);

        yield return new WaitForSeconds(landingEffectDuration);

        SetTelegraphVisible(false);

        isPerformingGroundPound = false;
    }
    
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(tauntTriggerHash);
            animator.ResetTrigger(groundPoundTriggerHash);
        }
    }

    private void CreateTelegraphVisual()
    {
        telegraphRoot = new GameObject("GroundPoundTelegraph");
        telegraphRoot.transform.SetParent(transform, false);

        Vector3 groundPosition = GetGroundPosition();
        telegraphRoot.transform.position = groundPosition + Vector3.up * groundOffset;
        telegraphRoot.transform.rotation = Quaternion.identity;

        if (telegraphMaterial != null)
        {
            telegraphMatInstance = new Material(telegraphMaterial);
        }
        else
        {
            telegraphMatInstance = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            telegraphMatInstance.SetFloat("_Surface", 1); // Transparent
            telegraphMatInstance.SetFloat("_Blend", 0);   // Alpha blend
            telegraphMatInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            telegraphMatInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            telegraphMatInstance.SetInt("_ZWrite", 0);
            telegraphMatInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        fillMeshFilter = telegraphRoot.AddComponent<MeshFilter>();
        fillMeshRenderer = telegraphRoot.AddComponent<MeshRenderer>();
        fillMeshRenderer.material = telegraphMatInstance;
        fillMeshFilter.mesh = CreateCircleMesh(telegraphRadius, circleSegments);

        SetTelegraphVisible(false);
    }

    private Vector3 GetGroundPosition()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, groundLayerMask))
        {
            return hit.point;
        }
        return transform.position;
    }

    private Mesh CreateCircleMesh(float radius, int segments)
    {
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];
        Vector2[] uvs = new Vector2[segments + 1];

        vertices[0] = Vector3.zero;
        uvs[0] = Vector2.one * 0.5f;

        float angleStep = 2f * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            vertices[i + 1] = new Vector3(x, 0, z);
            uvs[i + 1] = new Vector2(
                (x / radius + 1f) * 0.5f,
                (z / radius + 1f) * 0.5f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = (i + 1) % segments + 1;
            
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = next;
            triangles[i * 3 + 2] = current;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }

    private IEnumerator AnimateTelegraph()
    {
        SetTelegraphVisible(true);
        SetTelegraphColor(telegraphColor);
        
        float elapsedTime = 0f;
        Vector3 originalScale = telegraphRoot.transform.localScale;
        
        while (elapsedTime < telegraphDuration)
        {
            float progress = elapsedTime / telegraphDuration;
            
            float expansionProgress = telegraphExpansionCurve.Evaluate(progress);
            telegraphRoot.transform.localScale = originalScale * (0.1f + expansionProgress * 0.9f);
            
            float pulseFactor = 0.7f + 0.3f * Mathf.Sin(progress * Mathf.PI * 4); // Pulse 4 times
            SetTelegraphAlpha(telegraphAlpha * pulseFactor);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        telegraphRoot.transform.localScale = originalScale;
        SetTelegraphColor(telegraphColor); // Reset to normal alpha
    }

    private void SetTelegraphColor(Color color)
    {
        if (telegraphMatInstance != null)
        {
            float alpha = (color == dangerColor) ? dangerAlpha : telegraphAlpha;
            
            Color colorWithAlpha = new Color(color.r, color.g, color.b, alpha);
            
            telegraphMatInstance.color = colorWithAlpha;
            
            if (telegraphMatInstance.HasProperty("_BaseColor"))
            {
                telegraphMatInstance.SetColor("_BaseColor", colorWithAlpha);
            }
        }
    }

    private void SetTelegraphAlpha(float alpha)
    {
        if (telegraphMatInstance != null)
        {
            Color currentColor = telegraphMatInstance.color;
            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            
            telegraphMatInstance.color = newColor;
            
            if (telegraphMatInstance.HasProperty("_BaseColor"))
            {
                telegraphMatInstance.SetColor("_BaseColor", newColor);
            }
        }
    }

    private void SetTelegraphVisible(bool visible)
    {
        if (telegraphRoot != null)
        {
            telegraphRoot.SetActive(visible);
        }
    }

    private void CheckDamageOnImpact()
    {
        if (playerHealth == null) return;
        
        Vector3 impactCenter = telegraphRoot.transform.position;
        float distanceToPlayer = Vector3.Distance(PlayerPos, impactCenter);
        
        if (distanceToPlayer <= telegraphRadius)
        {
            playerHealth.TakeDamage(Mathf.RoundToInt(slamDamage * damageMultiplier));
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (isPerformingGroundPound && telegraphRoot != null)
        {
            Vector3 groundPos = GetGroundPosition();
            telegraphRoot.transform.position = groundPos + Vector3.up * groundOffset;
        }
    }

    private void OnDestroy()
    {
        if (telegraphMatInstance != null)
        {
            DestroyImmediate(telegraphMatInstance);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, telegraphRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

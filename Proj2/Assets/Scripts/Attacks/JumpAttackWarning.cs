using UnityEngine;

public class JumpAttackWarning : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private float radius = 8f;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private Color warningColor = Color.red;
    
    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float minAlpha = 0.3f;
    
    private GameObject warningRing;
    private Material warningMaterial;
    private float timer = 0f;
    
    void Start()
    {
        CreateWarningRing();
        Destroy(gameObject, warningDuration);
    }
    
    void Update()
    {
        AnimateWarning();
    }
    
    void CreateWarningRing()
    {
        warningRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warningRing.transform.SetParent(transform);
        warningRing.transform.localPosition = Vector3.zero;
        warningRing.transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2);
        
        Destroy(warningRing.GetComponent<Collider>());
        
        warningMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        warningMaterial.SetColor("_BaseColor", warningColor);
        warningMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        warningMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        warningMaterial.SetInt("_ZWrite", 0);
        warningMaterial.SetFloat("_Surface", 1);
        warningMaterial.SetFloat("_Blend", 0);
        warningMaterial.renderQueue = 3000;
        
        warningRing.GetComponent<Renderer>().material = warningMaterial;
    }
    
    void AnimateWarning()
    {
        timer += Time.deltaTime;
        float pulse = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(timer * pulseSpeed) + 1f) * 0.5f);
        
        Color color = warningColor;
        color.a = pulse;
        warningMaterial.SetColor("_BaseColor", color);
        
        float scaleMultiplier = 1f + Mathf.Sin(timer * pulseSpeed * 0.5f) * 0.1f;
        warningRing.transform.localScale = new Vector3(radius * 2 * scaleMultiplier, 0.01f, radius * 2 * scaleMultiplier);
    }
    
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        if (warningRing != null)
            warningRing.transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2);
    }
    
    public void SetDuration(float newDuration)
    {
        warningDuration = newDuration;
        Destroy(gameObject, warningDuration);
    }
}
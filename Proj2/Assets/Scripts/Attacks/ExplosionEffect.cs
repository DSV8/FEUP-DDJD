using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Color explosionColor = Color.magenta;
    [SerializeField] private Color shockwaveColor = Color.yellow;
    
    [Header("Animation")]
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private int ringCount = 3;
    [SerializeField] private float ringDelay = 0.1f;
    
    private GameObject[] explosionRings;
    private Material[] explosionMaterials;
    private float timer = 0f;
    private bool hasStarted = false;
    
    void Start()
    {
        CreateExplosionRings();
        StartExplosion();
        Destroy(gameObject, duration);
    }
    
    void Update()
    {
        if (hasStarted)
        {
            AnimateExplosion();
        }
    }
    
    void CreateExplosionRings()
    {
        explosionRings = new GameObject[ringCount];
        explosionMaterials = new Material[ringCount];
        
        for (int i = 0; i < ringCount; i++)
        {
            explosionRings[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            explosionRings[i].transform.SetParent(transform);
            explosionRings[i].transform.localPosition = Vector3.zero;
            explosionRings[i].transform.localScale = Vector3.zero;
            explosionRings[i].SetActive(false);
            
            Destroy(explosionRings[i].GetComponent<Collider>());
            
            Color ringColor = Color.Lerp(explosionColor, shockwaveColor, (float)i / (ringCount - 1));
            
            explosionMaterials[i] = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            explosionMaterials[i].SetColor("_BaseColor", ringColor);
            explosionMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            explosionMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            explosionMaterials[i].SetInt("_ZWrite", 0);
            explosionMaterials[i].SetFloat("_Surface", 1);
            explosionMaterials[i].SetFloat("_Blend", 0);
            explosionMaterials[i].renderQueue = 3000;
            
            explosionRings[i].GetComponent<Renderer>().material = explosionMaterials[i];
        }
    }
    
    void StartExplosion()
    {
        hasStarted = true;
        for (int i = 0; i < ringCount; i++)
        {
            StartCoroutine(ActivateRingDelayed(i, i * ringDelay));
        }
    }
    
    System.Collections.IEnumerator ActivateRingDelayed(int ringIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (explosionRings[ringIndex] != null)
        {
            explosionRings[ringIndex].SetActive(true);
        }
    }
    
    void AnimateExplosion()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;
        
        for (int i = 0; i < ringCount; i++)
        {
            if (!explosionRings[i].activeInHierarchy) continue;
            
            float ringStartTime = i * ringDelay;
            float ringProgress = Mathf.Clamp01((timer - ringStartTime) / (duration - ringStartTime));
            
            float scale = Mathf.Lerp(0f, explosionRadius * 2 * maxScale, ringProgress);
            float height = Mathf.Lerp(0.05f, 0.01f, ringProgress);
            explosionRings[i].transform.localScale = new Vector3(scale, height, scale);
            
            float alpha = 1f - ringProgress;
            if (ringProgress > 0.7f)
            {
                alpha = Mathf.Lerp(1f, 0f, (ringProgress - 0.7f) / 0.3f);
            }
            
            Color ringColor = explosionMaterials[i].GetColor("_BaseColor");
            ringColor.a = alpha;
            explosionMaterials[i].SetColor("_BaseColor", ringColor);
        }
    }
    
    public void SetRadius(float newRadius)
    {
        explosionRadius = newRadius;
    }
    
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
        Destroy(gameObject, duration);
    }
    
    public void SetColors(Color explosion, Color shockwave)
    {
        explosionColor = explosion;
        shockwaveColor = shockwave;
        
        if (explosionMaterials != null)
        {
            for (int i = 0; i < explosionMaterials.Length; i++)
            {
                Color ringColor = Color.Lerp(explosionColor, shockwaveColor, (float)i / (ringCount - 1));
                explosionMaterials[i].SetColor("_BaseColor", ringColor);
            }
        }
    }
}
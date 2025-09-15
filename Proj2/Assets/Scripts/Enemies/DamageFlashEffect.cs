using System.Collections;
using UnityEngine;

public class DamageFlashEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.5f;
    [SerializeField] AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("References")]
    [SerializeField] SkinnedMeshRenderer[] meshRenderers;
    
    Material[][] originalMaterials;
    Color[][] originalColors;
    Coroutine currentFlashCoroutine;
    
    static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    
    void Awake()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }
        
        if (meshRenderers.Length == 0)
        {
            Debug.LogWarning($"DamageFlashEffect on {name}: No SkinnedMeshRenderer found!");
            return;
        }
        
        StoreOriginalMaterials();
    }
    
    void StoreOriginalMaterials()
    {
        originalMaterials = new Material[meshRenderers.Length][];
        originalColors = new Color[meshRenderers.Length][];
        
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            var renderer = meshRenderers[i];
            originalMaterials[i] = new Material[renderer.materials.Length];
            originalColors[i] = new Color[renderer.materials.Length];
            
            for (int j = 0; j < renderer.materials.Length; j++)
            {
                originalMaterials[i][j] = new Material(renderer.materials[j]);
                
                if (originalMaterials[i][j].HasProperty(BaseColorProperty))
                {
                    originalColors[i][j] = originalMaterials[i][j].GetColor(BaseColorProperty);
                }
                else
                {
                    originalColors[i][j] = Color.white;
                }
            }
            
            renderer.materials = originalMaterials[i];
        }
    }
    
    public void TriggerFlash()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }
        
        currentFlashCoroutine = StartCoroutine(FlashCoroutine());
    }
    
    IEnumerator FlashCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / flashDuration;
            float curveValue = flashCurve.Evaluate(normalizedTime);
            
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var materials = meshRenderers[i].materials;
                
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j].HasProperty(BaseColorProperty))
                    {
                        Color currentColor = Color.Lerp(originalColors[i][j], flashColor, curveValue);
                        materials[j].SetColor(BaseColorProperty, currentColor);
                    }
                }
            }
            
            yield return null;
        }
        
        RestoreOriginalColors();
        currentFlashCoroutine = null;
    }
    
    void RestoreOriginalColors()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            var materials = meshRenderers[i].materials;
            
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty(BaseColorProperty))
                {
                    materials[j].SetColor(BaseColorProperty, originalColors[i][j]);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (originalMaterials != null)
        {
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i] != null)
                {
                    for (int j = 0; j < originalMaterials[i].Length; j++)
                    {
                        if (originalMaterials[i][j] != null)
                        {
                            Destroy(originalMaterials[i][j]);
                        }
                    }
                }
            }
        }
    }
}
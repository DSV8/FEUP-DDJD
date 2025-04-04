// PortalIndicator.cs
using UnityEngine;

public class PortalIndicator : MonoBehaviour
{
    [SerializeField] private Transform exitPoint;
    [SerializeField] private SpriteRenderer arrowRenderer;
    [SerializeField] private float arrowDistance = 0.5f;
    [SerializeField] private float arrowPulseSpeed = 1f;
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;
    
    private bool isPlayerNearby = false;
    
    private void Awake()
    {
        if (arrowRenderer == null)
        {
            GameObject arrowObj = new GameObject("PortalArrow");
            arrowObj.transform.SetParent(transform);
            arrowObj.transform.localPosition = new Vector3(0, arrowDistance, 0);
            
            arrowRenderer = arrowObj.AddComponent<SpriteRenderer>();
            arrowRenderer.sprite = Resources.Load<Sprite>("UI/ArrowIndicator");
            
            if (arrowRenderer.sprite == null)
            {
                Texture2D arrowTexture = CreateArrowTexture();
                Sprite arrowSprite = Sprite.Create(arrowTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
                arrowRenderer.sprite = arrowSprite;
            }
            
            arrowRenderer.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        }
        
        if (exitPoint == null && GetComponent<Portal>() != null)
        {
            exitPoint = GetComponent<Portal>().exitPoint;
        }
        
        if (arrowRenderer != null)
        {
            arrowRenderer.enabled = false;
        }
    }
    
    private void Update()
    {
        if (isPlayerNearby && exitPoint != null && arrowRenderer != null)
        {
            Vector3 direction = (exitPoint.position - transform.position).normalized;
            
            arrowRenderer.transform.position = transform.position + direction * arrowDistance;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * arrowPulseSpeed) + 1) / 2);
            Color currentColor = arrowRenderer.color;
            arrowRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }
    
    public void ShowArrow()
    {
        isPlayerNearby = true;
        if (arrowRenderer != null)
        {
            arrowRenderer.enabled = true;
        }
    }
    
    public void HideArrow()
    {
        isPlayerNearby = false;
        if (arrowRenderer != null)
        {
            arrowRenderer.enabled = false;
        }
    }
    
    private Texture2D CreateArrowTexture()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color transparent = new Color(0, 0, 0, 0);
        Color arrowColor = new Color(0.2f, 0.8f, 1f, 1f);
        
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, transparent);
            }
        }
        
        for (int y = 8; y < 24; y++)
        {
            for (int x = 8; x < 32 - y + 8; x++)
            {
                if (x >= 8 && x <= 24 && y >= 8 && y <= 24)
                {
                    texture.SetPixel(x, y, arrowColor);
                }
            }
        }
        
        texture.Apply();
        return texture;
    }
}
using UnityEngine;
using System.Collections;

public class TriggerButton : MonoBehaviour
{
    [SerializeField] private GameObject blockade;
    
    [SerializeField]
    private Sprite pressedButtonSprite;
    
    [SerializeField]
    private float pressDuration = 0.5f;
    
    [SerializeField]
    private float blockadeFadeDuration = 1.0f;
    
    [SerializeField]
    private Color connectionColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    
    [SerializeField]
    private float connectionWidth = 0.1f;
    
    [SerializeField]
    private float pulsateSpeed = 1.0f;
    
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalPosition;
    private LineRenderer connectionLine;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on the button object!");
        }
        
        originalPosition = transform.position;
        
        CreateConnectionLine();
    }
    
    private void CreateConnectionLine()
    {
        if (blockade == null)
        {
            Debug.LogWarning("Cannot create connection line: blockade reference is not set.");
            return;
        }
        
        connectionLine = gameObject.AddComponent<LineRenderer>();
        
        connectionLine.startWidth = connectionWidth;
        connectionLine.endWidth = connectionWidth;
        connectionLine.material = new Material(Shader.Find("Sprites/Default"));
        
        connectionLine.sortingLayerName = "Default";
        connectionLine.sortingOrder = 5;
        
        connectionLine.startColor = connectionColor;
        connectionLine.endColor = connectionColor;
        connectionLine.positionCount = 2;
        
        UpdateConnectionLine();
    }
    
    private void Update()
    {
        if (connectionLine != null && blockade != null)
        {
            UpdateConnectionLine();
            
            float pulseValue = Mathf.PingPong(Time.time * pulsateSpeed, 0.5f) + 0.5f;
            Color pulseColor = new Color(connectionColor.r, connectionColor.g, connectionColor.b, 
                                         connectionColor.a * pulseValue);
            connectionLine.startColor = pulseColor;
            connectionLine.endColor = pulseColor;
        }
    }
    
    private void UpdateConnectionLine()
    {
        connectionLine.SetPosition(0, transform.position);
        connectionLine.SetPosition(1, blockade.transform.position);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (connectionLine != null)
        {
            Destroy(connectionLine);
        }
        
        if (blockade != null)
        {
            StartCoroutine(FadeOutBlockade());
        }
        else
        {
            Debug.LogWarning("Blockade reference is not set on the TriggerButton script.");
        }
        
        if (spriteRenderer != null && pressedButtonSprite != null)
        {
            StartCoroutine(PressButton());
        }

        Destroy(collision.gameObject);
    }
    
    private IEnumerator FadeOutBlockade()
    {
        SpriteRenderer blockadeRenderer = blockade.GetComponent<SpriteRenderer>();
        
        if (blockadeRenderer != null)
        {
            Color originalColor = blockadeRenderer.color;
            float elapsedTime = 0f;
            
            while (elapsedTime < blockadeFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / blockadeFadeDuration;
                
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(originalColor.a, 0f, normalizedTime);
                blockadeRenderer.color = newColor;
                
                yield return null;
            }
        }
        
        Destroy(blockade);
    }
    
    private IEnumerator PressButton()
    {
        spriteRenderer.sprite = pressedButtonSprite;
        transform.position = originalPosition + new Vector3(0.15f, 0f, 0f);
        
        yield return new WaitForSeconds(pressDuration);
        
        spriteRenderer.sprite = originalSprite;
        transform.position = originalPosition;
    }
}
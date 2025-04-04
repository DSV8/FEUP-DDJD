using UnityEngine;
using System;

public class CircuitConnection : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    
    [SerializeField] private float hoverExpandScale = 1.2f;
    [SerializeField] private float expandDuration = 0.2f;
    [SerializeField] private Color defaultColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    
    private Vector3 originalScale;
    private Vector3 expandedScale;
    private bool isActive = false;
    private CircuitPuzzleManager puzzleManager;
    
    public int connectionIndex;
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        originalScale = transform.localScale;
        expandedScale = originalScale * hoverExpandScale;
        
        ResetColor();
    }
    
    public void Initialize(CircuitPuzzleManager manager, int index)
    {
        puzzleManager = manager;
        connectionIndex = index;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (!active)
        {
            transform.localScale = originalScale;
            ResetColor();
        }
    }
    
    private void OnMouseEnter()
    {
        if (!isActive) return;
        
        LeanTween.scale(gameObject, expandedScale, expandDuration).setEase(LeanTweenType.easeOutQuad);
    }
    
    private void OnMouseExit()
    {
        if (!isActive) return;
        
        LeanTween.scale(gameObject, originalScale, expandDuration).setEase(LeanTweenType.easeOutQuad);
    }
    
    private void OnMouseDown()
    {
        if (!isActive) return;
        
        puzzleManager.ConnectionClicked(this);
    }
    
    public void SetCorrect()
    {
        spriteRenderer.color = correctColor;
    }
    
    public void FlashIncorrect(Action onComplete = null)
    {
        spriteRenderer.color = incorrectColor;
        
        LeanTween.value(gameObject, 0f, 1f, 0.5f)
            .setOnComplete(() => {
                ResetColor();
                onComplete?.Invoke();
            });
    }
    
    public void ResetColor()
    {
        spriteRenderer.color = defaultColor;
    }
}
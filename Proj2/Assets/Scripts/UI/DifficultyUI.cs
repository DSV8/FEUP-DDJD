using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image difficultyIcon;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI difficultyLabel;
    
    [Header("Animation Settings")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private RectTransform iconTransform;
    private float currentDifficulty = 1f;
    private bool isPulsing = false;
    
    private void Awake()
    {
        if (difficultyIcon != null)
            iconTransform = difficultyIcon.GetComponent<RectTransform>();
    }
    
    private void OnEnable()
    {
        EnemySpawnManager.OnDifficultyChanged += UpdateDifficultyDisplay;
    }
    
    private void OnDisable()
    {
        EnemySpawnManager.OnDifficultyChanged -= UpdateDifficultyDisplay;
    }
    
    private void Start()
    {
        // Initialize display
        UpdateDifficultyDisplay(1f);
        
        if (difficultyLabel != null)
            difficultyLabel.text = "Threat Level";
    }
    
    private void UpdateDifficultyDisplay(float difficulty)
    {
        currentDifficulty = difficulty;
        
        // Update text - show as integer
        if (difficultyText != null)
        {
            difficultyText.text = Mathf.RoundToInt(difficulty).ToString();
        }
        
        // Pulse animation when difficulty increases
        if (!isPulsing && difficulty > 1)
        {
            StartCoroutine(PulseAnimation());
        }
        
        // Change color based on difficulty
        UpdateDifficultyColor(difficulty);
    }
    
    private void UpdateDifficultyColor(float difficulty)
    {
        if (difficultyIcon == null && difficultyText == null) return;
        _ = Color.white;
        Color targetColor;
        if (difficulty <= 3)
        {
            // Green to Yellow
            targetColor = Color.Lerp(Color.green, Color.yellow, (difficulty - 1) / 2f);
        }
        else if (difficulty <= 6)
        {
            // Yellow to Orange
            targetColor = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (difficulty - 3) / 3f);
        }
        else if (difficulty <= 10)
        {
            // Orange to Red
            targetColor = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (difficulty - 6) / 4f);
        }
        else
        {
            // Deep red for extreme difficulty
            targetColor = Color.Lerp(Color.red, new Color(0.5f, 0f, 0f), Mathf.Min((difficulty - 10) / 10f, 1f));
        }

        if (difficultyText != null)
            difficultyText.color = targetColor;
    }
    
    private System.Collections.IEnumerator PulseAnimation()
    {
        if (iconTransform == null) yield break;
        
        isPulsing = true;
        float elapsed = 0f;
        Vector3 originalScale = iconTransform.localScale;
        
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pulseDuration;
            float curveValue = pulseCurve.Evaluate(progress);
            
            float scale = Mathf.Lerp(1f, pulseScale, curveValue);
            iconTransform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        iconTransform.localScale = originalScale;
        isPulsing = false;
    }
}
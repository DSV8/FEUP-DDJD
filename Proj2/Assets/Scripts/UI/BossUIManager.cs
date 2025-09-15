using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    
    [Header("Animation Settings")]
    [SerializeField] private float slideInDuration = 1f;
    [SerializeField] private float slideOutDuration = 0.5f;
    [SerializeField] private float healthBarFillSpeed = 2f;
    [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    [SerializeField] private Color healthBarFullColor = Color.green;
    [SerializeField] private Color healthBarMidColor = Color.yellow;
    [SerializeField] private Color healthBarLowColor = Color.red;
    [SerializeField] private string bossName = "Finn The Frogg";
    
    private FinnTheFrogBoss bossController;
    private RectTransform uiPanelRect;
    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;
    private bool isVisible = false;
    private Coroutine currentAnimation;
    private float targetHealthValue = 1f;
    
    void Start()
    {
        // Find boss controller
        bossController = FindFirstObjectByType<FinnTheFrogBoss>();
        if (bossController != null)
        {
            bossController.OnHealthChanged += UpdateHealthBar;
            bossController.OnPlayerAreaStatusChanged += OnPlayerAreaStatusChanged;
            bossController.OnBossDeath += OnBossDeath;
        }
        
        // Setup UI
        SetupUI();
    }
    
    void SetupUI()
    {
        if (bossUIPanel == null) return;
        
        uiPanelRect = bossUIPanel.GetComponent<RectTransform>();
        
        // Set boss name
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
        
        // Setup positions
        visiblePosition = uiPanelRect.anchoredPosition;
        hiddenPosition = new Vector3(visiblePosition.x, visiblePosition.y + uiPanelRect.rect.height + 50f, visiblePosition.z);
        
        // Start hidden
        uiPanelRect.anchoredPosition = hiddenPosition;
        bossUIPanel.SetActive(false);
        
        // Setup health bar
        if (healthBar != null)
        {
            healthBar.value = 1f;
            targetHealthValue = 1f;
            UpdateHealthBarColor(1f);
        }
    }
    
    void Update()
    {
        // Smooth health bar animation
        if (healthBar != null && Mathf.Abs(healthBar.value - targetHealthValue) > 0.01f)
        {
            healthBar.value = Mathf.Lerp(healthBar.value, targetHealthValue, healthBarFillSpeed * Time.deltaTime);
            UpdateHealthBarColor(healthBar.value);
        }
    }
    
    void OnPlayerAreaStatusChanged(bool playerInArea)
    {
        if (playerInArea && !isVisible)
        {
            ShowBossUI();
        }
        else if (!playerInArea && isVisible)
        {
            HideBossUI();
        }
    }
    
    void ShowBossUI()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(SlideInUI());
    }
    
    void HideBossUI()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(SlideOutUI());
    }
    
    IEnumerator SlideInUI()
    {
        bossUIPanel.SetActive(true);
        isVisible = true;
        
        float elapsedTime = 0f;
        Vector3 startPos = uiPanelRect.anchoredPosition;
        
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideInDuration;
            progress = slideInCurve.Evaluate(progress);
            
            uiPanelRect.anchoredPosition = Vector3.Lerp(startPos, visiblePosition, progress);
            
            yield return null;
        }
        
        uiPanelRect.anchoredPosition = visiblePosition;
        currentAnimation = null;
    }
    
    IEnumerator SlideOutUI()
    {
        isVisible = false;
        
        float elapsedTime = 0f;
        Vector3 startPos = uiPanelRect.anchoredPosition;
        
        while (elapsedTime < slideOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideOutDuration;
            
            uiPanelRect.anchoredPosition = Vector3.Lerp(startPos, hiddenPosition, progress);
            
            yield return null;
        }
        
        uiPanelRect.anchoredPosition = hiddenPosition;
        bossUIPanel.SetActive(false);
        currentAnimation = null;
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar == null) return;
        
        targetHealthValue = currentHealth / maxHealth;
    }
    
    void UpdateHealthBarColor(float healthPercentage)
    {
        if (healthBarFill == null) return;
        
        Color targetColor;
        
        if (healthPercentage > 0.6f)
        {
            targetColor = Color.Lerp(healthBarMidColor, healthBarFullColor, (healthPercentage - 0.6f) / 0.4f);
        }
        else if (healthPercentage > 0.3f)
        {
            targetColor = Color.Lerp(healthBarLowColor, healthBarMidColor, (healthPercentage - 0.3f) / 0.3f);
        }
        else
        {
            targetColor = healthBarLowColor;
        }
        
        healthBarFill.color = targetColor;
    }
    
    void OnBossDeath()
    {
        StartCoroutine(BossDeathSequence());
    }
    
    IEnumerator BossDeathSequence()
    {
        // Wait a moment before hiding UI
        yield return new WaitForSeconds(2f);
        
        HideBossUI();
    }
    
    void OnDestroy()
    {
        if (bossController != null)
        {
            bossController.OnHealthChanged -= UpdateHealthBar;
            bossController.OnPlayerAreaStatusChanged -= OnPlayerAreaStatusChanged;
            bossController.OnBossDeath -= OnBossDeath;
        }
    }
}
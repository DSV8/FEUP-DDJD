using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text keyStatusText;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public TMP_Text scoreText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("UI Animation")]
    public float pulseDuration = 0.5f;
    public float pulseScale = 1.2f;
    
    private GameManager gameManager;
    
    void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }
    
    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("UIManager couldn't find GameManager!");
            return;
        }
        
        UpdateKeyStatus(false);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        gameManager.OnKeyCollected += OnKeyCollected;
        gameManager.OnGameOver += OnGameOver;
        gameManager.OnTimeUpdated += OnTimeUpdated;
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnKeyCollected -= OnKeyCollected;
            gameManager.OnGameOver -= OnGameOver;
            gameManager.OnTimeUpdated -= OnTimeUpdated;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        }
    }
    
    private void OnKeyCollected()
    {
        UpdateKeyStatus(true);
        if (keyStatusText != null)
        {
            PulseText(keyStatusText);
        }
    }
    
    private void OnTimeUpdated(float currentTime)
    {
        if (timerText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
            timerText.text = string.Format("{0:00} : {1:00}", timeSpan.Minutes, timeSpan.Seconds);
            
            if (gameManager.GetRemainingTime() <= 30f)
            {
                timerText.color = Color.red;
            }
        }
    }
    
    private void OnGameOver(bool playerWon, string customLossMessage)
    {
        ShowGameOverScreen(playerWon, customLossMessage);
    }
    
    private void UpdateKeyStatus(bool hasKey)
    {
        if (keyStatusText != null)
        {
            keyStatusText.text = hasKey ? "Key found! Now open the vault!" : "Find the key!";
            keyStatusText.color = hasKey ? Color.yellow : Color.white;
        }
    }
    
    private void ShowGameOverScreen(bool playerWon, string customLossMessage = "")
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                if (playerWon)
                {
                    gameOverText.text = "EXAM STOLEN!";
                    gameOverText.color = Color.yellow;
                }
                else
                {
                    gameOverText.text = string.IsNullOrEmpty(customLossMessage) ? "TIME'S UP!" : customLossMessage;
                    gameOverText.color = Color.red;
                }
            }
            
            if (scoreText != null)
            {
                scoreText.text = $"Score: {gameManager.GetScore()}";
            }
        }
    }
    
    public void PulseText(TMP_Text textElement)
    {
        if (textElement != null)
        {
            StartCoroutine(PulseCoroutine(textElement.transform));
        }
    }
    
    private System.Collections.IEnumerator PulseCoroutine(Transform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * pulseScale;
        
        float t = 0f;
        while (t < pulseDuration / 2)
        {
            t += Time.deltaTime;
            float normalizedTime = t / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(originalScale, targetScale, normalizedTime);
            yield return null;
        }
        
        t = 0f;
        while (t < pulseDuration / 2)
        {
            t += Time.deltaTime;
            float normalizedTime = t / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(targetScale, originalScale, normalizedTime);
            yield return null;
        }
        
        target.localScale = originalScale;
    }
    
    private void OnRestartButtonClicked()
    {
        SceneManager.LoadScene("Level");
    }

    private void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int coinCount = 0;

    public Text coinText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        SaveCoins();
        UpdateCoinText();
        Debug.Log("Coins: " + coinCount);
    }

    public void SetCoinText(Text text)
    {
        coinText = text;
        UpdateCoinText();
    }

    void SaveCoins()
    {
        PlayerPrefs.SetInt("Coins", coinCount);
        PlayerPrefs.Save();
    }

    void LoadCoins()
    {
        coinCount = PlayerPrefs.GetInt("Coins", 0);
        UpdateCoinText();
    }

    void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "-" + coinCount;
        else
            Debug.LogWarning("coinText is not assigned in this scene.");
    }

}

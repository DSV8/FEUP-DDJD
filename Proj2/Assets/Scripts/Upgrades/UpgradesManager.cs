using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UpgradesManager : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    [Header("Jump Upgrades")]
    public Button JumpButton;
    public int JumpBaseCost = 5;
    private int currentJumpCost = 5;
    public int startingJumpLevel = 1;
    public int maxJumpLevel = 8;
    public TextMeshProUGUI JumpText;
    public TextMeshProUGUI JumpUI;
    private int currentJumpLevel = 1;

    [Header("Dash Upgrades")]
    public Button DashButton;
    public int DashBaseCost = 10;
    private int currentDashCost = 10;
    public int startingDashLevel = 1;
    public int maxDashLevel = 8;
    public TextMeshProUGUI DashText;
    public TextMeshProUGUI DashUI;
    private int currentDashLevel = 1;

    [Header("Health Upgrades")]
    public Button HealthButton;
    public int HealthBaseCost = 20;
    private int currentHealthCost = 20;
    public int startingHealthLevel = 1;
    public int maxHealthLevel = 8;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI HealthUI;
    private int currentHealthLevel = 1;


    [Header("Movement Upgrades")]
    public Button MovementButton;
    public int MovementBaseCost = 30;
    private int currentMovementCost = 30;
    public int startingMovementLevel = 1;
    public int maxMovementLevel = 8;
    public TextMeshProUGUI MovementText;
    public TextMeshProUGUI MovementUI;
    private int currentMovementLevel = 1;

    [Header("Fire Rate Upgrades")]
    public Button FireRateButton;
    public int FireRateBaseCost = 25;
    private int currentFireRateCost = 25;
    public int startingFireRateLevel = 1;
    public int maxFireRateLevel = 8;
    public TextMeshProUGUI FireRateText;
    public TextMeshProUGUI FireRateUI;
    private int currentFireRateLevel = 1;

    [Header("Damage Upgrades")]
    public Button DamageButton;
    public int DamageBaseCost = 40;
    private int currentDamageCost = 40;
    public int startingDamageLevel = 1;
    public int maxDamageLevel = 8;
    public TextMeshProUGUI DamageText;
    public TextMeshProUGUI DamageUI;
    private int currentDamageLevel = 1;

    void Start()
    {
        //Remove to delete all player upgrades
        // PlayerPrefs.DeleteAll();

        LoadJumpData();
        LoadDashData();
        LoadHealthData();
        LoadFireRateData();
        LoadMovementData();
        LoadDamageData();
        UpdateUI();
        UpdateJumpUI();
        UpdateDashUI();
        UpdateHealthUI();
        UpdateMovementUI();
        UpdateFireRateUI();
        UpdateDamageUI();
    }

    public void OnBuyJump()
    {
        if (GameManager.Instance.coinCount >= currentJumpCost)
        {
            GameManager.Instance.AddCoins(-currentJumpCost);
            currentJumpLevel += 1;
            currentJumpCost += currentJumpLevel * JumpBaseCost;

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Upgrade da Loja");
            
            SaveJumpData();

            Debug.Log("Jump bought!");
            UpdateUI();
            UpdateJumpUI();
        }
        else
        {
            Debug.Log("Not enough UltraCoins!");
        }
    }

    public void OnBuyDash()
    {
        if (GameManager.Instance.coinCount >= currentDashCost)
        {
            GameManager.Instance.AddCoins(-currentDashCost);
            currentDashLevel += 1;
            currentDashCost += currentDashLevel * DashBaseCost;

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Upgrade da Loja");

            SaveDashData();

            Debug.Log("Dash bought!");
            UpdateUI();
            UpdateDashUI();
        }
        else
        {
            Debug.Log("Not enough UltraCoins!");
        }
    }

    public void OnBuyHealth()
    {
        if (GameManager.Instance.coinCount >= currentHealthCost)
        {
            GameManager.Instance.AddCoins(-currentHealthCost);
            currentHealthLevel += 1;
            currentHealthCost += currentHealthLevel * HealthBaseCost;
            
            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Upgrade da Loja");

            SaveHealthData();

            Debug.Log("Health bought!");
            UpdateUI();
            UpdateHealthUI();
        }
        else
        {
            Debug.Log("Not enough UltraCoins!");
        }
    }

    public void OnBuyMovement()
    {
        if (GameManager.Instance.coinCount >= currentMovementCost)
        {
            GameManager.Instance.AddCoins(-currentMovementCost);
            currentMovementLevel += 1;
            currentMovementCost += currentMovementLevel * MovementBaseCost;

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Upgrade da Loja");

            SaveMovementData();

            Debug.Log("Movement bought!");
            UpdateUI();
            UpdateMovementUI();
        }
        else
        {
            Debug.Log("Not enough UltraCoins!");
        }
    }

    public void OnBuyFireRate()
    {
        if (GameManager.Instance.coinCount >= currentFireRateCost)
        {
            GameManager.Instance.AddCoins(-currentFireRateCost);
            currentFireRateLevel += 1;
            currentFireRateCost += currentFireRateLevel * FireRateBaseCost;

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Upgrade da Loja");

            SaveFireRateData();

            Debug.Log("FireRate bought!");
            UpdateUI();
            UpdateFireRateUI();
        }
        else
        {
            Debug.Log("Not enough UltraCoins!");
        }
    }

    public void OnBuyDamage()
    {
        if (GameManager.Instance.coinCount >= currentDamageCost)
        {
            GameManager.Instance.AddCoins(-currentDamageCost);
            currentDamageLevel += 1;
            currentDamageCost += currentDamageLevel * DamageBaseCost;

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Upgrade da Loja");

            SaveDamageData();

            Debug.Log("Damage bought!");
            UpdateUI();
            UpdateDamageUI();
        }
        else
        {
            Debug.Log("Not enough UltraCoins!");
        }
    }

    void UpdateUI()
    {
        moneyText.text = "TOTAL - x" + GameManager.Instance.coinCount;
    }

    void UpdateJumpUI()
    {
        JumpText.text = "x" + currentJumpCost.ToString();

        int remaining = maxJumpLevel - currentJumpLevel;

        string bar = "";

        for (int i = 0; i < currentJumpLevel; i++)
            bar += "| ";

        for (int i = 0; i < remaining; i++)
            bar += "- ";

        JumpUI.text = bar.Trim();
    }

    void UpdateDashUI()
    {
        DashText.text = "x" + currentDashCost.ToString();

        int remaining = maxDashLevel - currentDashLevel;

        string bar = "";

        for (int i = 0; i < currentDashLevel; i++)
            bar += "| ";

        for (int i = 0; i < remaining; i++)
            bar += "- ";

        DashUI.text = bar.Trim();
    }

    void UpdateHealthUI()
    {
        HealthText.text = "x" + currentHealthCost.ToString();

        int remaining = maxHealthLevel - currentHealthLevel;

        string bar = "";

        for (int i = 0; i < currentHealthLevel; i++)
            bar += "| ";

        for (int i = 0; i < remaining; i++)
            bar += "- ";

        HealthUI.text = bar.Trim();
    }

    void UpdateMovementUI()
    {
        MovementText.text = "x" + currentMovementCost.ToString();

        int remaining = maxMovementLevel - currentMovementLevel;

        string bar = "";

        for (int i = 0; i < currentMovementLevel; i++)
            bar += "| ";

        for (int i = 0; i < remaining; i++)
            bar += "- ";

        MovementUI.text = bar.Trim();
    }

    void UpdateFireRateUI()
    {
        FireRateText.text = "x" + currentFireRateCost.ToString();

        int remaining = maxFireRateLevel - currentFireRateLevel;

        string bar = "";

        for (int i = 0; i < currentFireRateLevel; i++)
            bar += "| ";

        for (int i = 0; i < remaining; i++)
            bar += "- ";

        FireRateUI.text = bar.Trim();
    }

    void UpdateDamageUI()
    {
        DamageText.text = "x" + currentDamageCost.ToString();

        int remaining = maxDamageLevel - currentDamageLevel;

        string bar = "";

        for (int i = 0; i < currentDamageLevel; i++)
            bar += "| ";

        for (int i = 0; i < remaining; i++)
            bar += "- ";

        DamageUI.text = bar.Trim();
    }

    void SaveJumpData()
    {
        PlayerPrefs.SetInt("JumpLevel", currentJumpLevel);
        PlayerPrefs.SetInt("JumpCost", currentJumpCost);
        PlayerPrefs.Save();
    }

    void LoadJumpData()
    {
        currentJumpLevel = PlayerPrefs.GetInt("JumpLevel", startingJumpLevel);
        currentJumpCost = PlayerPrefs.GetInt("JumpCost", JumpBaseCost);
    }

    void SaveDashData()
    {
        PlayerPrefs.SetInt("DashLevel", currentDashLevel);
        PlayerPrefs.SetInt("DashCost", currentDashCost);
        PlayerPrefs.Save();
    }

    void LoadDashData()
    {
        currentDashLevel = PlayerPrefs.GetInt("DashLevel", startingDashLevel);
        currentDashCost = PlayerPrefs.GetInt("DashCost", DashBaseCost);
    }
    void SaveHealthData()
    {
        PlayerPrefs.SetInt("HealthLevel", currentHealthLevel);
        PlayerPrefs.SetInt("HealthCost", currentHealthCost);
        PlayerPrefs.Save();
    }

    void LoadHealthData()
    {
        currentHealthLevel = PlayerPrefs.GetInt("HealthLevel", startingHealthLevel);
        currentHealthCost = PlayerPrefs.GetInt("HealthCost", HealthBaseCost);
    }

    void SaveMovementData()
    {
        PlayerPrefs.SetInt("MovementLevel", currentMovementLevel);
        PlayerPrefs.SetInt("MovementCost", currentMovementCost);
        PlayerPrefs.Save();
    }

    void LoadMovementData()
    {
        currentMovementLevel = PlayerPrefs.GetInt("MovementLevel", startingMovementLevel);
        currentMovementCost = PlayerPrefs.GetInt("MovementCost", MovementBaseCost);
    }

    void SaveFireRateData()
    {
        PlayerPrefs.SetInt("FireRateLevel", currentFireRateLevel);
        PlayerPrefs.SetInt("FireRateCost", currentFireRateCost);
        PlayerPrefs.Save();
    }

    void LoadFireRateData()
    {
        currentFireRateLevel = PlayerPrefs.GetInt("FireRateLevel", startingFireRateLevel);
        currentFireRateCost = PlayerPrefs.GetInt("FireRateCost", FireRateBaseCost);
    } 
    void SaveDamageData()
    {
        PlayerPrefs.SetInt("DamageLevel", currentDamageLevel);
        PlayerPrefs.SetInt("DamageCost", currentDamageCost);
        PlayerPrefs.Save();
    }

    void LoadDamageData()
    {
        currentDamageLevel = PlayerPrefs.GetInt("DamageLevel", startingDamageLevel);
        currentDamageCost = PlayerPrefs.GetInt("DamageCost", DamageBaseCost);
    }
}

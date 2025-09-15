using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameOverMenu : MonoBehaviour
{
    public TextMeshProUGUI runTime;
    public TextMeshProUGUI totalDmg;
    public TextMeshProUGUI totalKills;
    public TextMeshProUGUI totalCoins;

    void Start()
    {
        float runTimeSeconds = PlayerPrefs.GetFloat("runTime", 0f);
        int playerTotalDmg = PlayerPrefs.GetInt("playerTotalDmg", 0);
        int killedEnemies = PlayerPrefs.GetInt("killedEnemies", 0);
        int collectedCoins = PlayerPrefs.GetInt("collectedCoins", 0);


        int hours = (int)(runTimeSeconds / 3600);
        int minutes = (int)((runTimeSeconds % 3600) / 60);
        int seconds = (int)(runTimeSeconds % 60);
        string formattedTime = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

        runTime.text = formattedTime;
        totalDmg.text = playerTotalDmg.ToString();
        totalKills.text = killedEnemies.ToString();
        totalCoins.text = collectedCoins.ToString();

    }

    public void playGame()
    {
        Bullet.damagePowerUp = 0;
        Bullet.damageMultiplier = 1;
        Bullet.criticalChance = 5;
        Bullet.criticalDamage = 0.5f;
        Bullet.healingFactorOnHit = 0;
        
        EMPEffect.damagePowerUp = 0;
        EMPEffect.RadiusPowerUp = 0;

        DifficultyPersistence.ClearDifficultyData();
        
        PlayerPrefs.SetInt("HealthPow", 0);
        PlayerPrefs.SetInt("SelfHeal", 0);
        PlayerPrefs.SetInt("medHeal", 0);
        PlayerPrefs.SetInt("BulletHealing", 0);
        PlayerPrefs.SetFloat("MovementPower", 0f);
        PlayerPrefs.SetInt("jumpBoy", 0);
        PlayerPrefs.SetFloat("airBoy", 0f);
        PlayerPrefs.SetFloat("dashDownBoy", 0f);
        PlayerPrefs.SetFloat("dashSpeedBoy", 0f);
        PlayerPrefs.SetInt("BulletDamage", 0);
        PlayerPrefs.SetFloat("fireRateUP", 0f);
        PlayerPrefs.SetFloat("BulletCriticalDamage", 0f);
        PlayerPrefs.SetInt("BulletCriticalChance", 0);
        PlayerPrefs.SetFloat("EMPPowerUPRadius", 0f);
        PlayerPrefs.SetInt("EMPPowerUPDamage", 0);
        PlayerPrefs.SetFloat("powerUPDown", 0f);
        PlayerPrefs.SetInt("DoubleHealthHalveSpeedHealthPart", 0);
        PlayerPrefs.SetFloat("DoubleHealthHalveSpeedWalkPart", 0f);  
        PlayerPrefs.SetFloat("DoubleHealthHalveSpeedAirPart", 0f);  
        PlayerPrefs.SetFloat("DoublePowerReduceHealthPowerPart", 0f);
        PlayerPrefs.SetInt("DoublePowerReduceHealthHealthPart", 0);
        PlayerPrefs.SetFloat("DoubleMovementHalvePowerWalkPart", 0f);  
        PlayerPrefs.SetFloat("DoubleMovementHalvePowerAirPart", 0f); 
        
        SceneManager.LoadScene("Scenes/Maps/Open");
    }

    public void quitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void returnMenu()
    {
        DifficultyPersistence.ClearDifficultyData();

        Debug.Log("Return to Main Menu!");
        SceneManager.LoadScene("Scenes/Menu");
    }
}

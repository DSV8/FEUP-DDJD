using UnityEngine;

public class PlayerRunStats : MonoBehaviour
{
    private float runTime = 0f;
    public static int playerTotalDmg = 0;
    public static int killedEnemies = 0;
    public static int collectedCoins = 0;


    void Start()
    {
        runTime = 0f;
        playerTotalDmg = 0;
        killedEnemies = 0;
        collectedCoins = 0;

        PlayerPrefs.SetFloat("runTime", runTime);
        PlayerPrefs.SetInt("playerTotalDmg", playerTotalDmg);
        PlayerPrefs.SetInt("killedEnemies", killedEnemies);
        PlayerPrefs.SetInt("collectedCoins", collectedCoins);
        PlayerPrefs.Save();
    }

    void Update()
    {
        runTime += Time.deltaTime;

        // Debug.Log("Run Time: " + runTime.ToString("F2") + " seconds");

        // Debug.Log("Killed enemies: " + killedEnemies);

        // Debug.Log("damaged enemies: " + playerTotalDmg);

        UpdatedSavePlayerData();
    }

    void UpdatedSavePlayerData()
    {
        PlayerPrefs.SetFloat("runTime", runTime);
        PlayerPrefs.SetInt("playerTotalDmg", playerTotalDmg);
        PlayerPrefs.SetInt("killedEnemies", killedEnemies);
        PlayerPrefs.SetInt("collectedCoins", collectedCoins);
        PlayerPrefs.Save();
    }
}



using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int healthMultiplier = 30;
    private int _currentHealth;
    
    public event Action OnDeath;

    // Self heal vars
    private int selfHeal = 0;
    private float healTimer = 0f;
    private float healInterval = 3f;
    private int doubledHealth = 0;
    private int halvedHealth = 0;
    void Awake()
    {
        
        // Start game with health upgrade
        if (gameObject.name == "Player")
        {
            int healthLevel = PlayerPrefs.GetInt("HealthLevel", 1);
            int healthPower = PlayerPrefs.GetInt("HealthPow", 0);
            selfHeal = PlayerPrefs.GetInt("SelfHeal", 0);
            maxHealth = maxHealth + (healthLevel-1) * healthMultiplier + healthPower + PlayerPrefs.GetInt("DoubleHealthHalveSpeedHealthPart", 0) -  PlayerPrefs.GetInt("DoublePowerReduceHealthHealthPart", 0);

            Debug.Log("Updated Player Max Health!");
        }
        Debug.Log(maxHealth);

        _currentHealth = maxHealth;
    }

    // Self heal for player
    void Update()
    {
        // Start game with health upgrade
        if (gameObject.name == "Player" && selfHeal > 0 && _currentHealth < maxHealth)
        {
            healTimer += Time.deltaTime;
            if (healTimer >= healInterval)
            {
                Heal(selfHeal);
                healTimer = 0f;
                Debug.Log("Healed player by " + selfHeal);
            }
        }
    }

    public void IncreaseSelfHeal(int inc)
    {
        selfHeal += inc;
        
        PlayerPrefs.SetInt("SelfHeal", selfHeal);
    } 

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Low Priority/Dano/Dano 3");
            Die();
        }
        else
        {
            int rand = UnityEngine.Random.Range(1, 3);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Low Priority/Dano/Dano " + rand.ToString());
        }
    }

    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        PlayerPrefs.SetInt("HealthPow", amount);
        maxHealth += amount;

        // In case health upgrade also heals, currently no!
        // _currentHealth += amount;
    }

    public void DoubleMaxHealth()
    {
        int IncreaseHealth = maxHealth;
        maxHealth += IncreaseHealth;

        _currentHealth += IncreaseHealth;

        doubledHealth += IncreaseHealth;
        PlayerPrefs.SetInt("DoubleHealthHalveSpeedHealthPart", doubledHealth);
    }

    public void HalveMaxHealth()
    {
        maxHealth /= 2;

        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }

        halvedHealth += maxHealth;
        PlayerPrefs.SetInt("DoublePowerReduceHealthHealthPart", halvedHealth);
    }

    public void Die()
    {
        // Change scene when player dies.
        if (gameObject.name == "Player")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Player died, Game over screen!");
            SceneManager.LoadScene("Scenes/GameOver");
        }

        Debug.Log(gameObject.name + " died!!! :(");

        // Trigger the death event for any listeners (like the spawner)
        OnDeath?.Invoke();

        // Destroy the game object after a short delay
        Destroy(gameObject, 0.1f);
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }
}
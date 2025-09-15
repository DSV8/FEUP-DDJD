using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public PlayerHealth entityHealth;

    void Start()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            healthSlider.maxValue = entityHealth.maxHealth;
        }
    }
    void Update()
    {
        healthSlider.value = entityHealth.GetCurrentHealth();

        // health text management
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            healthText.text = healthSlider.value + "/" + entityHealth.maxHealth;
        }
    }

    public void UpdateUpgrade()
    {
        healthSlider.maxValue = entityHealth.maxHealth;
    }
}

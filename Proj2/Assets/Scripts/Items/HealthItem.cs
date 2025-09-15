using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public static int healAmount = 50;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int medHeal = PlayerPrefs.GetInt("medHeal", 0);

            other.GetComponent<PlayerHealth>()?.Heal(healAmount + medHeal);
            Debug.Log("Player healed by " + healAmount);
            Destroy(gameObject);

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Barreira de energia (uma)");
        }
    }
}

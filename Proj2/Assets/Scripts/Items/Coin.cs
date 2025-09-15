using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCoins(value);
            Destroy(gameObject);

            // Increase the total collected coins per run in gameStats
            PlayerRunStats.collectedCoins += 1;

            FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Adquirir Moedas");
        }
    }
}

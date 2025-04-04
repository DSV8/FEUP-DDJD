using UnityEngine;

public class ClockPowerUp : MonoBehaviour
{
    [SerializeField] private float secondsGiven = 30f;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {   
            gameManager.IncreaseTime(secondsGiven);
            gameObject.SetActive(false);
        }
    }
}
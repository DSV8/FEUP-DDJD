using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    public float speedMultiplier = 4f;
    public float duration = 7f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {   
            MovementController playerMovement = other.GetComponentInParent<MovementController>();
            if (playerMovement != null)
            {
                playerMovement.ActivateSpeedBoost(speedMultiplier, duration);
            }
            gameObject.SetActive(false);
        }
    }
}
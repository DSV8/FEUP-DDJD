using UnityEngine;

public class LaserTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteractionController playerInteractionController
                = other.transform.parent.gameObject.GetComponent<PlayerInteractionController>();

            if (playerInteractionController.isOnRope) {
                playerInteractionController.ExitRopeMode();
            }

            GameManager.Instance.GameOver(false, "You were hit by a laser!");
        }
    }
}

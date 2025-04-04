using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    public MovementController movementController;
    public PlayerInteractionController interactionController;

    private void Awake()
    {
        if (movementController == null)
            movementController = GetComponent<MovementController>();
            
        if (interactionController == null)
            interactionController = GetComponent<PlayerInteractionController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SlowdownTrap"))
        {
            movementController.SetSlowDown(true);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            interactionController.SetCurrentInteractable(other.gameObject);
            
            // Get the indicator component and trigger the fade-in effect.
            var indicator = other.GetComponent<InteractableIndicator>();
            if (indicator != null)
            {
                indicator.ShowIndicator();
            }

            if (other.CompareTag("Portal"))
            {
                var portalIndicator = other.GetComponent<PortalIndicator>();
                if (portalIndicator != null)
                {
                    portalIndicator.ShowArrow();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SlowdownTrap"))
        {
            movementController.SetSlowDown(false);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Interactable") && 
                interactionController.GetCurrentInteractable() == other.gameObject)
        {
            // Trigger the fade-out effect.
            var indicator = other.GetComponent<InteractableIndicator>();
            if (indicator != null)
            {
                indicator.HideIndicator();
            }
            interactionController.SetCurrentInteractable(null);

            if (other.CompareTag("Portal"))
            {
                var portalIndicator = other.GetComponent<PortalIndicator>();
                if (portalIndicator != null)
                {
                    portalIndicator.HideArrow();
                }
            }
        }
    }
}
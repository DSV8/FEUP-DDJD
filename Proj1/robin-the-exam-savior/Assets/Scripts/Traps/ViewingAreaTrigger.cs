using UnityEngine;

public class ViewingAreaTrigger : MonoBehaviour
{
    private SecurityCamera securityCamera;

    private void Awake()
    {
        securityCamera = GetComponentInParent<SecurityCamera>();
        if (securityCamera == null)
        {
            Debug.LogError("ViewingAreaTrigger: No SecurityCamera found in parent.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            securityCamera?.OnPlayerDetected(other.gameObject.transform.parent.gameObject);
        }
    }
}

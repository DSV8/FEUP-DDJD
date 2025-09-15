using UnityEngine;

public class BossAreaTrigger : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private LayerMask playerLayer = -1;
    
    public System.Action<bool> OnPlayerAreaStatusChanged;
    
    private bool playerInArea = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (!playerInArea)
            {
                playerInArea = true;
                OnPlayerAreaStatusChanged?.Invoke(true);
                Debug.Log("Player entered boss area");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (playerInArea)
            {
                playerInArea = false;
                OnPlayerAreaStatusChanged?.Invoke(false);
                Debug.Log("Player left boss area");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = playerInArea ? Color.red : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
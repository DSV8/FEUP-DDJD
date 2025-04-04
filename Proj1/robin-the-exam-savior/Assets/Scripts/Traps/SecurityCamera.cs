using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Viewing Area Settings")]
    [SerializeField] private GameObject viewingArea;
    [SerializeField] private Color idleColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private string defeatMessage = "Caught by the security camera!";

    private SpriteRenderer viewingAreaRenderer;
    private bool isActive = true;

    private void Awake()
    {
        if (viewingArea != null)
        {
            viewingAreaRenderer = viewingArea.GetComponent<SpriteRenderer>();
            if (viewingAreaRenderer != null)
            {
                viewingAreaRenderer.color = idleColor;
            }
            else
            {
                Debug.LogWarning("SecurityCamera: No SpriteRenderer found on viewing area.");
            }
        }
        else
        {
            Debug.LogError("SecurityCamera: Viewing Area not assigned.");
        }
    }

    public void OnPlayerDetected(GameObject player)
    {
        if (!isActive)
            return;

        if (viewingAreaRenderer != null)
        {
            viewingAreaRenderer.color = alertColor;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false, defeatMessage);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BubbleGum"))
        {
            Destroy(collision.gameObject);

            SpriteRenderer cameraRenderer = GetComponent<SpriteRenderer>();
            if (cameraRenderer != null)
            {
                cameraRenderer.color = Color.grey;
            }
            else
            {
                Debug.LogWarning("SecurityCamera: No SpriteRenderer found on the camera.");
            }
            
            DeactivateCamera();
        }
    }

    private void DeactivateCamera()
    {
        isActive = false;

        if (viewingArea != null)
        {
            Destroy(viewingArea);
        }

        enabled = false;
    }
}

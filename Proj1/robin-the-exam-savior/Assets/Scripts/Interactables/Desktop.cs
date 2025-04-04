using UnityEngine;

public class Desktop : MonoBehaviour
{
    [Header("Settings")]
    public bool requiresKey = true;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    [Header("Feedback")]
    public GameObject lockEffect;
    public GameObject unlockEffect;

    private SpriteRenderer spriteRenderer;
    private bool playerInRange = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && lockedSprite != null)
        {
            spriteRenderer.sprite = lockedSprite;
        }

        if (lockEffect != null)
        {
            lockEffect.SetActive(true);
        }

        if (unlockEffect != null)
        {
            unlockEffect.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.W))
        {
            TryOpenVault();
        }
    }

    private void TryOpenVault()
    {
        if (GameManager.Instance != null)
        {
            if (!requiresKey || GameManager.Instance.hasKey)
            {
                OpenVault();
            }
            else
            {
                PlayLockedFeedback();
            }
        }
    }

    private void OpenVault()
    {
        if (spriteRenderer != null && unlockedSprite != null)
        {
            spriteRenderer.sprite = unlockedSprite;
        }

        if (lockEffect != null)
        {
            lockEffect.SetActive(false);
        }

        if (unlockEffect != null)
        {
            unlockEffect.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OpenVault();
        }

        enabled = false;
    }

    private void PlayLockedFeedback()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}

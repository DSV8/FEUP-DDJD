using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float groundSpeed = 5f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField, Range(0f, 1f)] private float groundDecay = 0.9f;
    [SerializeField] private float slowdownFactor = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Slow Effect Visual")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float flashInterval = 0.2f;
    [SerializeField] private Color slowEffectColor = Color.white;
    [SerializeField] private bool showSlowEffect = true;

    private bool isSpeedBoosted = false;
    private float speedBoostMultiplier = 4f;
    private float speedBoostEndTime = 0f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool movementEnabled = true;
    private bool isSlowDownActive = false;

    private Animator animator;
    
    // Variables for flash effect
    private SpriteRenderer playerSprite;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Get the player sprite renderer (can be on a child object)
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite != null)
        {
            originalColor = playerSprite.color;
        }
        else
        {
            Debug.LogWarning($"{nameof(MovementController)} on {gameObject.name} could not find a SpriteRenderer component.");
        }
        
        if (groundCheck == null)
        {
            Debug.LogWarning($"{nameof(MovementController)} on {gameObject.name} has no groundCheck assigned.");
        }
    }

    private void Update()
    {
        if (!movementEnabled) return;

        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded() && !isSlowDownActive)
        {
            animator.SetBool("isJumping", !IsGrounded());
            Jump();
        }

        CheckSpeedBoost();
    }

    private void FixedUpdate()
    {
        if (!movementEnabled) return;

        ApplyMovement();
        ApplyFriction();

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (IsGrounded())
        {
            animator.SetBool("isJumping", false);
        }

        animator.SetFloat("isSpeedBoosted", isSpeedBoosted ? 1f : 0f);
    }

    private void ApplyMovement()
    {
        float effectiveSpeed = (isSlowDownActive ? groundSpeed * slowdownFactor : groundSpeed) * speedBoostMultiplier;
        float newVelocityX = Mathf.Clamp(rb.linearVelocity.x + horizontalInput * acceleration * Time.fixedDeltaTime,
                                         -effectiveSpeed, effectiveSpeed);
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalInput);
            transform.localScale = scale;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
        animator.SetBool("isJumping", true);
    }

    private void ApplyFriction()
    {
        if (IsGrounded() && Mathf.Approximately(horizontalInput, 0f))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * groundDecay, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        Collider2D hit = Physics2D.OverlapBox(groundCheck.bounds.center, groundCheck.bounds.size, 0f, groundMask);
        return hit != null;
    }

    private void CheckSpeedBoost()
    {
        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            speedBoostMultiplier = 1f;
        }
    }

    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        isSpeedBoosted = true;
        speedBoostMultiplier = multiplier;
        speedBoostEndTime = Time.time + duration;
    }

    public void EnableMovement() => movementEnabled = true;

    public void DisableMovement()
    {
        movementEnabled = false;
        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;

        animator.SetFloat("xVelocity", 0);
        animator.SetFloat("yVelocity", 0);
    }

    public void SetSlowDown(bool active)
    {
        // Only process if the state changes
        if (isSlowDownActive != active)
        {
            isSlowDownActive = active;
            
            // Show flashing effect
            if (showSlowEffect && playerSprite != null)
            {
                if (isSlowDownActive)
                {
                    StartFlashingEffect();
                }
                else
                {
                    StopFlashingEffect();
                }
            }
        }
    }

    private void StartFlashingEffect()
    {
        if (playerSprite == null) return;

        // Stop any existing flashing coroutine
        StopFlashingEffect();
        
        // Start the flashing coroutine
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private void StopFlashingEffect()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Reset to original color
        if (playerSprite != null)
        {
            playerSprite.color = originalColor;
        }
    }

    private IEnumerator FlashRoutine()
    {
        while (isSlowDownActive)
        {
            // Flash to white
            playerSprite.color = slowEffectColor;
            yield return new WaitForSeconds(flashDuration);
            
            // Return to original color
            playerSprite.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.bounds.center, groundCheck.bounds.size);
        }
    }
    
    private void OnDisable()
    {
        // Make sure we stop any running coroutines when disabled
        StopFlashingEffect();
    }
}
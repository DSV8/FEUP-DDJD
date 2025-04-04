using UnityEngine;

public class Throwable : MonoBehaviour
{
    [SerializeField] private float gravityScale = 1f;

    private Rigidbody2D rb;
    private Collider2D physicsCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCollider = GetComponentInChildren<Collider2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void PickUp()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var indicator = GetComponent<InteractableIndicator>();
        if (indicator != null)
        {
            indicator.HideIndicator();
        }
    }

    public void Launch(Vector2 direction, float force)
    {
        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;

        rb.linearVelocity = direction * force;
    }

    public float GetGravity()
    {
        return gravityScale * Physics2D.gravity.magnitude;
    }
}

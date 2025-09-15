using System.Collections;
using UnityEngine;

public class WeaponSlashEnemy : BaseWalkingEnemy
{
    [Header("Sword Attack")]
    [SerializeField] int   attackDamage            = 25;
    [SerializeField] float attackAnimationDuration = 1.0f;

    [Header("Ray-cast Settings")]
    [Tooltip("Moment in the animation (0-1) when the blade should hit")]
    [SerializeField, Range(0, 1)] float hitMoment = 0.4f;

    [Tooltip("Origin for the ray – drag in an empty GameObject placed at the tip of the blade")]
    [SerializeField] Transform rayOrigin;

    [Tooltip("Length of the ray that represents the blade's reach")]
    [SerializeField] float rayLength = 2f;
    
    [Tooltip("Distance from ray origin for the additional rays (left, right, up, down)")]
    [SerializeField] float raySpread = 0.5f;

    [SerializeField] LayerMask playerMask = 1 << 0;

    private int weaponSlashTriggerHash;
    private float damageMultiplier = 1f;

    protected override void Awake()
    {
        base.Awake();

        weaponSlashTriggerHash = Animator.StringToHash("WeaponSlashTrigger");
    }
    
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    protected override IEnumerator PrimaryAttack()
    {
        animator.SetTrigger(weaponSlashTriggerHash);

        yield return new WaitForSeconds(attackAnimationDuration * hitMoment);

        TryHitPlayer();

        yield return new WaitForSeconds(attackAnimationDuration * (1f - hitMoment));
    }

    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(weaponSlashTriggerHash);
        }
    }

    bool TryHitPlayer()
    {
        Vector3 centerOrigin = rayOrigin != null
            ? rayOrigin.position
            : transform.position + Vector3.up * 1f;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = Vector3.up;

        Vector3[] rayOrigins = new Vector3[]
        {
            centerOrigin,
            centerOrigin - right * raySpread,
            centerOrigin + right * raySpread,
            centerOrigin + up * raySpread,
            centerOrigin - up * raySpread
        };

        foreach (Vector3 origin in rayOrigins)
        {
            if (Physics.Raycast(origin, forward, out RaycastHit hit,
                                rayLength, playerMask,
                                QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(Mathf.RoundToInt(attackDamage * damageMultiplier));
                    return true;
                }
            }
        }
        
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector3 centerOrigin = rayOrigin != null
                ? rayOrigin.position
                : transform.position + Vector3.up * 1f;

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 up = Vector3.up;

            Vector3[] rayOrigins = new Vector3[]
            {
                centerOrigin,
                centerOrigin - right * raySpread,
                centerOrigin + right * raySpread,
                centerOrigin + up * raySpread,
                centerOrigin - up * raySpread
            };

            Color[] rayColors = new Color[]
            {
                Color.red,
                Color.blue,
                Color.green,
                Color.yellow,
                Color.magenta
            };

            for (int i = 0; i < rayOrigins.Length; i++)
            {
                Gizmos.color = rayColors[i];
                Gizmos.DrawLine(rayOrigins[i], rayOrigins[i] + forward * rayLength);
                
                Gizmos.DrawWireSphere(rayOrigins[i], 0.05f);
            }
        }
    }
#endif
}
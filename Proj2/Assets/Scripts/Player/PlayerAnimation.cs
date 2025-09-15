using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    Animator animator;
    PlayerMovement playerMovement;

    [SerializeField] private Collider weaponCollider;

    [Header("Combo Attack Settings")]
    int numAttacksSeq = 3;
    int attackSeqIndex = 0;
    float seqTimer = 0f;
    float seqResetTime = 5f; // Time to reset combo if no input

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetBool("IsCrouching", Input.GetButton("Crouch"));
        animator.SetBool("IsCrouching", playerMovement.IsCrouching());
        animator.SetInteger("JumpPhase", playerMovement.GetJumpPhase());
        animator.SetBool("IsInMeleeMode", playerMovement.GetIsInMeleeMode());

        if (playerMovement.GetIsInMeleeMode())
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (attackSeqIndex < numAttacksSeq)
                {
                    animator.SetTrigger("Attack" + (attackSeqIndex + 1));
                    attackSeqIndex++;
                }
                else
                {
                    int finisher = Random.Range(1, 4);
                    animator.SetTrigger("Finisher" + finisher);
                    attackSeqIndex = 0;
                }
                seqTimer = seqResetTime;
            }

            if (playerMovement.IsCrouching() && Input.GetKey(KeyCode.Mouse0))
            {
                animator.SetTrigger("CrouchAttack");
                attackSeqIndex = 0;
                seqTimer = seqResetTime;
            }
        }

        if (seqTimer > 0)
        {
            seqTimer -= Time.deltaTime;
            if (seqTimer <= 0)
            {
                attackSeqIndex = 0;
            }
        }
    }

    public void EnableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
    }
    public void DisableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }
    
    public void SetMeleeAttackSpeed(float speed = 1.0f)
    {
        animator.SetFloat("MeleeAttackSpeed", speed);
    }
}

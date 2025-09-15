using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinnTheFrogBoss : MonoBehaviour
{
    [Header("Boss Configuration")]
    [SerializeField] private float maxHealth = 1000f;
    [SerializeField] private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float playerDetectionRange = 20f;
    [SerializeField] private Transform areaCenter;
    [SerializeField] private float areaRadius = 25f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float meleeRange = 4f;
    [SerializeField] private float shootRange = 15f;

    [Header("Small Shot Attack")]
    [SerializeField] private GameObject smallProjectilePrefab;
    [SerializeField] private Transform[] smallShotPoints;
    [SerializeField] private int smallShotDamage = 25;
    [SerializeField] private float smallShotSpeed = 15f;
    [SerializeField] private int smallShotBurst = 3;
    [SerializeField] private float smallShotBurstDelay = 0.2f;

    [Header("Big Shot Attack")]
    [SerializeField] private GameObject bigProjectilePrefab;
    [SerializeField] private Transform bigShotPoint;
    [SerializeField] private int bigShotDamage = 75;
    [SerializeField] private float bigShotSpeed = 8f;
    [SerializeField] private float bigShotExplosionRadius = 5f;

    [Header("Kick Attack")]
    [SerializeField] private int kickDamage = 100;
    [SerializeField] private float kickRange = 5f;
    [SerializeField] private float kickKnockback = 10f;

    [Header("Jump Attack")]
    [SerializeField] private int jumpDamage = 150;
    [SerializeField] private float jumpRadius = 8f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float jumpDuration = 2f;
    [SerializeField] private float minJumpDuration = 0.8f;
    [SerializeField] private GameObject jumpWarningEffectPrefab;

    [Header("Phase Settings")]
    [SerializeField] private float phase2HealthThreshold = 0.66f;
    [SerializeField] private float phase3HealthThreshold = 0.33f;
    [SerializeField] private float phase2DamageMultiplier = 1.3f;
    [SerializeField] private float phase3DamageMultiplier = 1.6f;
    [SerializeField] private int phase2ShotBurstBonus = 1;
    [SerializeField] private int phase3ShotBurstBonus = 2;
    [SerializeField] private float enrageSpeedMultiplier = 1.5f;
    [SerializeField] private float enrageCooldownMultiplier = 0.7f;

    [Header("Visual Effects")]
    [SerializeField] private DamageFlashEffect flashEffect;

    private Transform player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private Animator animator;
    private bool isPlayerInArea = false;
    private bool canAttack = true;
    private bool isDead = false;
    private bool isAttacking = false;
    private int currentPhase = 1;
    private float currentAttackCooldown;
    private float currentMoveSpeed;

    private int originalSmallShotDamage;
    private int originalBigShotDamage;
    private int originalKickDamage;
    private int originalJumpDamage;
    private int originalSmallShotBurst;

    private int currentSmallShotDamage;
    private int currentBigShotDamage;
    private int currentKickDamage;
    private int currentJumpDamage;
    private int currentSmallShotBurst;

    private enum AttackType { SmallShot, BigShot, Kick, Jump }

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnBossDeath;
    public System.Action<bool> OnPlayerAreaStatusChanged;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();

        if (flashEffect == null)
            flashEffect = GetComponent<DamageFlashEffect>();

        currentAttackCooldown = attackCooldown;
        currentMoveSpeed = moveSpeed;

        originalSmallShotDamage = smallShotDamage;
        originalBigShotDamage = bigShotDamage;
        originalKickDamage = kickDamage;
        originalJumpDamage = jumpDamage;
        originalSmallShotBurst = smallShotBurst;

        currentSmallShotDamage = smallShotDamage;
        currentBigShotDamage = bigShotDamage;
        currentKickDamage = kickDamage;
        currentJumpDamage = jumpDamage;
        currentSmallShotBurst = smallShotBurst;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (areaCenter == null)
            areaCenter = transform;
    }

    private void Update()
    {
        if (isDead) return;

        CheckPlayerInArea();

        if (isPlayerInArea && player != null)
        {
            HandleCombat();
        }
        else
        {
            if (!isAttacking)
                animator.SetBool("IsMoving", false);
        }

        StayInBounds();
    }

    private void HandleCombat()
    {
        if (isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > meleeRange)
        {
            MoveTowardsPlayer();
        }

        if (distanceToPlayer <= meleeRange)
        {
            if (!isAttacking)
            {
                ChooseAndExecuteAttack(distanceToPlayer);
            }
            return;
        }

        if (canAttack && distanceToPlayer <= playerDetectionRange)
        {
            ChooseAndExecuteAttack(distanceToPlayer);
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * currentMoveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetBool("IsMoving", true);
    }

    private void ChooseAndExecuteAttack(float distanceToPlayer)
    {
        AttackType chosenAttack;

        if (distanceToPlayer <= meleeRange)
        {
            chosenAttack = Random.Range(0f, 1f) < 0.7f ? AttackType.Kick : AttackType.Jump;
        }
        else if (distanceToPlayer <= shootRange)
        {
            float attackChoice = Random.Range(0f, 1f);
            chosenAttack = currentPhase >= 2 ?
                (attackChoice < 0.4f ? AttackType.SmallShot : attackChoice < 0.7f ? AttackType.BigShot : AttackType.Jump) :
                (attackChoice < 0.6f ? AttackType.SmallShot : AttackType.BigShot);
        }
        else
        {
            chosenAttack = AttackType.Jump;
        }

        ExecuteAttack(chosenAttack);
    }

    private void ExecuteAttack(AttackType attackType)
    {
        canAttack = false;
        isAttacking = true;
        animator.SetBool("IsMoving", false);

        switch (attackType)
        {
            case AttackType.SmallShot:
                StartCoroutine(SmallShotAttack());
                break;
            case AttackType.BigShot:
                StartCoroutine(BigShotAttack());
                break;
            case AttackType.Kick:
                StartCoroutine(KickAttack());
                break;
            case AttackType.Jump:
                StartCoroutine(JumpAttack());
                break;
        }
    }

    private IEnumerator SmallShotAttack()
    {
        FacePlayer(true);
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < currentSmallShotBurst; i++)
        {
            FacePlayer(true);
            FireSmallProjectile();
            animator.SetTrigger("ShootSmall");
            yield return new WaitForSeconds(smallShotBurstDelay);
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator BigShotAttack()
    {
        FacePlayer(true);
        yield return new WaitForSeconds(0.5f);

        animator.SetTrigger("ShootBig");
        yield return new WaitForSeconds(0.1f);
        FireBigProjectile();

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator KickAttack()
    {
        animator.SetTrigger("Kick");
        yield return new WaitForSeconds(0.4f);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= kickRange)
        {
            playerHealth?.TakeDamage(currentKickDamage);

            if (playerMovement != null)
            {
                Vector3 knockDir = (player.position - transform.position).normalized;
                knockDir.y = 0;
                playerMovement.ApplyKnockback(knockDir * kickKnockback);
            }
        }

        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator JumpAttack()
    {
        Vector3 targetPosition = player.position;

        if (jumpWarningEffectPrefab != null)
        {
            Vector3 warningPosition = targetPosition;
            warningPosition.y = transform.position.y;
            GameObject warning = Instantiate(jumpWarningEffectPrefab, warningPosition, Quaternion.identity);
            JumpAttackWarning warningEffect = warning.GetComponent<JumpAttackWarning>();
            if (warningEffect != null)
            {
                warningEffect.SetRadius(jumpRadius);
                warningEffect.SetDuration(jumpDuration + 0.3f);
            }
        }

        animator.SetTrigger("JumpTakeoff");
        yield return new WaitForSeconds(0.3f);

        Vector3 startPos = transform.position;
        targetPosition.y = startPos.y;
        float distance = Vector3.Distance(startPos, targetPosition);
        float maxConsidered = shootRange;
        float lerpT = Mathf.Clamp01(distance / maxConsidered);
        float realDuration = Mathf.Lerp(minJumpDuration, jumpDuration, lerpT);

        float elapsed = 0f;
        while (elapsed < realDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / realDuration;

            Vector3 pos = Vector3.Lerp(startPos, targetPosition, progress);
            pos.y += Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            transform.position = pos;
            yield return null;
        }

        transform.position = targetPosition;
        animator.SetTrigger("JumpLanding");

        if (Vector3.Distance(transform.position, player.position) <= jumpRadius)
        {
            playerHealth?.TakeDamage(currentJumpDamage);
        }

        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
        StartCoroutine(AttackCooldown());
    }

    private void FireSmallProjectile()
    {
        if (smallProjectilePrefab == null || smallShotPoints.Length == 0) return;

        Transform shotPoint = smallShotPoints[Random.Range(0, smallShotPoints.Length)];
        Vector3 dir = (player.position - shotPoint.position).normalized;

        var projGO = Instantiate(smallProjectilePrefab, shotPoint.position, Quaternion.LookRotation(dir));
        var proj = projGO.GetComponent<BossProjectile>();
        proj?.Initialize(dir, smallShotSpeed, currentSmallShotDamage, false);
    }

    private void FireBigProjectile()
    {
        if (bigProjectilePrefab == null || bigShotPoint == null) return;

        Vector3 dir = (player.position - bigShotPoint.position).normalized;
        var projGO = Instantiate(bigProjectilePrefab, bigShotPoint.position, Quaternion.LookRotation(dir));
        var proj = projGO.GetComponent<BossProjectile>();
        proj?.Initialize(dir, bigShotSpeed, currentBigShotDamage, true, bigShotExplosionRadius);
    }

    private IEnumerator AttackCooldown()
    {
        float elapsed = 0f;
        while (elapsed < currentAttackCooldown)
        {
            if (player != null && Vector3.Distance(transform.position, player.position) <= meleeRange)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }
        canAttack = true;
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (flashEffect != null)
        {
            flashEffect.TriggerFlash();
        }

        float pct = currentHealth / maxHealth;
        if (pct <= phase3HealthThreshold && currentPhase < 3) EnterPhase3();
        else if (pct <= phase2HealthThreshold && currentPhase < 2) EnterPhase2();

        if (currentHealth <= 0) Die();
    }

    private void EnterPhase2()
    {
        currentPhase = 2;
        currentMoveSpeed = moveSpeed * 1.2f;
        currentAttackCooldown = attackCooldown * 0.8f;

        currentSmallShotDamage = Mathf.RoundToInt(originalSmallShotDamage * phase2DamageMultiplier);
        currentBigShotDamage = Mathf.RoundToInt(originalBigShotDamage * phase2DamageMultiplier);
        currentKickDamage = Mathf.RoundToInt(originalKickDamage * phase2DamageMultiplier);
        currentJumpDamage = Mathf.RoundToInt(originalJumpDamage * phase2DamageMultiplier);
        currentSmallShotBurst = originalSmallShotBurst + phase2ShotBurstBonus;
    }

    private void EnterPhase3()
    {
        currentPhase = 3;
        currentMoveSpeed = moveSpeed * enrageSpeedMultiplier;
        currentAttackCooldown = attackCooldown * enrageCooldownMultiplier;

        currentSmallShotDamage = Mathf.RoundToInt(originalSmallShotDamage * phase3DamageMultiplier);
        currentBigShotDamage = Mathf.RoundToInt(originalBigShotDamage * phase3DamageMultiplier);
        currentKickDamage = Mathf.RoundToInt(originalKickDamage * phase3DamageMultiplier);
        currentJumpDamage = Mathf.RoundToInt(originalJumpDamage * phase3DamageMultiplier);
        currentSmallShotBurst = originalSmallShotBurst + phase3ShotBurstBonus;
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        canAttack = false;
        isAttacking = false;
        OnBossDeath?.Invoke();

        Destroy(gameObject);

        SceneManager.LoadScene("Scenes/GameOver");
    }

    private void CheckPlayerInArea()
    {
        bool prev = isPlayerInArea;
        isPlayerInArea = player != null && Vector3.Distance(player.position, areaCenter.position) <= areaRadius;
        if (prev != isPlayerInArea) OnPlayerAreaStatusChanged?.Invoke(isPlayerInArea);
    }

    private void StayInBounds()
    {
        Vector3 offset = transform.position - areaCenter.position;
        offset.y = 0;
        if (offset.magnitude > areaRadius)
        {
            Vector3 pos = areaCenter.position + offset.normalized * areaRadius;
            pos.y = transform.position.y;
            transform.position = pos;
        }
    }

    private void FacePlayer(bool instant = false)
    {
        if (player == null) return;
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir == Vector3.zero) return;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = instant ? targetRot : Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (areaCenter == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(areaCenter.position, areaRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
    
    public void SetHealthMultiplier(float multiplier)
    {
        maxHealth *= multiplier;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetDamageMultiplier(float multiplier)
    {
        smallShotDamage = Mathf.RoundToInt(smallShotDamage * multiplier);
        bigShotDamage = Mathf.RoundToInt(bigShotDamage * multiplier);
        kickDamage = Mathf.RoundToInt(kickDamage * multiplier);
        jumpDamage = Mathf.RoundToInt(jumpDamage * multiplier);
        
        currentSmallShotDamage = Mathf.RoundToInt(currentSmallShotDamage * multiplier);
        currentBigShotDamage = Mathf.RoundToInt(currentBigShotDamage * multiplier);
        currentKickDamage = Mathf.RoundToInt(currentKickDamage * multiplier);
        currentJumpDamage = Mathf.RoundToInt(currentJumpDamage * multiplier);
        
        originalSmallShotDamage = smallShotDamage;
        originalBigShotDamage = bigShotDamage;
        originalKickDamage = kickDamage;
        originalJumpDamage = jumpDamage;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        moveSpeed *= multiplier;
        currentMoveSpeed *= multiplier;
    }

    public void SetCooldownMultiplier(float multiplier)
    {
        attackCooldown *= multiplier;
        currentAttackCooldown *= multiplier;
    }
}
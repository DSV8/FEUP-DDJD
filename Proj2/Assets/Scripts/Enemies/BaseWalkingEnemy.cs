using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseWalkingEnemy : MonoBehaviour
{
    [Header("Detection & Movement")]
    [SerializeField] float patrolRadius = 10f;
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed  = 4f;
    [SerializeField] float chaseRange  = 12f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float eyeHeight = 1f;
    
    [Header("Rotation Settings")]
    [SerializeField] float playerFacingSpeed = 20f;

    [Header("Acceleration Settings")]
    [SerializeField] float patrolAcceleration = 8f;
    [SerializeField] float chaseAcceleration = 50f;

    [Header("Patrol Idle Settings")]
    [SerializeField] float minIdleTime = 1f;
    [SerializeField] float maxIdleTime = 4f;

    [Header("Attack")]
    [SerializeField] float attackCooldown = 2f;

    [Header("Hit Reaction Settings")]
    [SerializeField] float hitReactionDuration = 0.5f;
    [SerializeField] float hitStunChance = 0.3f;
    [SerializeField] float hitStunDuration = 1.0f;
    [SerializeField] bool maintainMomentumDuringHit = true;

    [Header("Death Settings")]
    [SerializeField] float destroyDelay = 2f;

    [Header("References")]
    [Tooltip("Animator component on the child model prefab")]
    [SerializeField] protected Animator animator;
    [Tooltip("Player transform reference")]
    [SerializeField] protected Transform player;
    
    protected NavMeshAgent agent;
    
    protected enum State { Patrol, PatrolIdle, Chase, Attack, HitReact, Dead }
    protected State currentState;
    protected State previousState;

    float cooldownTimer;
    bool  attackInProgress;
    float idleTimer;
    bool isIdling;
    bool isDead = false;
    bool isInHitReaction = false;
    bool isStunned = false;

    float chaseRangeSqr;
    float attackRangeSqr;
    float hitReactionTimer;
    Vector3 hitReactionVelocity;

    protected int speedHash;
    protected int stateHash;
    protected int hitReactTriggerHash;
    protected int deathTriggerHash;

    protected Vector3 PlayerPos => player != null ? player.position : Vector3.zero;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError($"GroundEnemyBase on '{name}' requires an Animator on one of its children.");

        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                player = playerGO.transform;
        }
        
        agent.autoBraking = true;
        agent.acceleration = patrolAcceleration;
        agent.speed = patrolSpeed;

        chaseRangeSqr = chaseRange  * chaseRange;
        attackRangeSqr = attackRange * attackRange;
        currentState = State.Patrol;

        speedHash = Animator.StringToHash("Speed");
        stateHash = Animator.StringToHash("State");
        hitReactTriggerHash = Animator.StringToHash("HitReactTrigger");
        deathTriggerHash = Animator.StringToHash("DeathTrigger");
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isIdling && idleTimer > 0f)
            idleTimer -= Time.deltaTime;
            
        if (hitReactionTimer > 0f)
            hitReactionTimer -= Time.deltaTime;

        UpdateAnimationParameters();

        switch (currentState)
        {
            case State.Patrol:     HandlePatrol();     break;
            case State.PatrolIdle: HandlePatrolIdle(); break;
            case State.Chase:      HandleChase();      break;
            case State.Attack:     HandleAttack();     break;
            case State.HitReact:   HandleHitReaction(); break;
            case State.Dead:       HandleDeath();      break;
        }
    }

    void UpdateAnimationParameters()
    {
        if (isDead) return;
        
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat(speedHash, currentSpeed);
        
        animator.SetInteger(stateHash, (int)currentState);
    }

    void HandlePatrol()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            if (!isIdling)
            {
                StartIdlePeriod();
                EnterState(State.PatrolIdle);
                return;
            }
        }

        if ((transform.position - PlayerPos).sqrMagnitude <= chaseRangeSqr)
            EnterState(State.Chase);
    }

    void HandlePatrolIdle()
    {
        if (idleTimer <= 0f)
        {
            isIdling = false;
            PickNewPatrolDestination();
            EnterState(State.Patrol);
        }

        if ((transform.position - PlayerPos).sqrMagnitude <= chaseRangeSqr)
        {
            isIdling = false;
            EnterState(State.Chase);
        }
    }

    void StartIdlePeriod()
    {
        isIdling = true;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        agent.isStopped = true;
    }

    void PickNewPatrolDestination()
    {
        Vector3 rnd = Random.insideUnitSphere * patrolRadius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(rnd, out hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void HandleChase()
    {
        FacePlayer();
        
        float distSqr = (transform.position - PlayerPos).sqrMagnitude;

        if (distSqr <= attackRangeSqr && HasLineOfSight())
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            EnterState(State.Attack);
            return;
        }

        if (distSqr > chaseRangeSqr)
        {
            EnterState(State.Patrol);
            return;
        }

        if (!agent.isStopped)
        {
            agent.SetDestination(PlayerPos);
        }
    }

    void HandleAttack()
    {
        if (attackInProgress) return;

        FacePlayer();

        float distSqr = (transform.position - PlayerPos).sqrMagnitude;
        if (distSqr > attackRangeSqr || !HasLineOfSight())
        {
            EnterState(State.Chase);
        }
        else if (cooldownTimer <= 0f)
        {
            StartCoroutine(AttackWrapper());
        }
    }

    void HandleHitReaction()
    {
        if (hitReactionTimer <= 0f)
        {
            isInHitReaction = false;
            isStunned = false;
            EnterState(previousState);
        }
        else if (!isStunned)
        {
            switch (previousState)
            {
                case State.Chase:
                    float distSqr = (transform.position - PlayerPos).sqrMagnitude;
                    if (distSqr <= attackRangeSqr && HasLineOfSight())
                    {
                        agent.SetDestination(PlayerPos);
                    }
                    else if (distSqr > chaseRangeSqr)
                    {
                        // If player got too far, we'll handle this when hit reaction ends
                    }
                    else
                    {
                        agent.SetDestination(PlayerPos);
                    }
                    break;
                    
                case State.Patrol:
                    if (!agent.hasPath || agent.remainingDistance < 0.5f)
                    {
                        PickNewPatrolDestination();
                    }
                    break;
            }
        }
        else if (maintainMomentumDuringHit && hitReactionVelocity.magnitude > 0.1f)
        {
            transform.position += hitReactionVelocity * Time.deltaTime;
            hitReactionVelocity = Vector3.Lerp(hitReactionVelocity, Vector3.zero, Time.deltaTime * 2f);
        }
    }

    void HandleDeath()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    IEnumerator AttackWrapper()
    {
        print("Yooo");
        attackInProgress = true;
        
        yield return StartCoroutine(PrimaryAttack());
        
        cooldownTimer   = attackCooldown;
        attackInProgress = false;
    }

    void EnterState(State newState)
    {
        if (isDead) return;
        
        currentState = newState;
        
        switch (newState)
        {
            case State.Patrol:
                agent.isStopped       = false;
                agent.autoBraking     = true;
                agent.acceleration    = patrolAcceleration;
                agent.speed           = patrolSpeed;
                agent.stoppingDistance = 0f;
                break;
            
            case State.PatrolIdle:
                agent.isStopped       = true;
                break;
            
            case State.Chase:
                agent.isStopped       = false;
                agent.autoBraking     = false;
                agent.acceleration    = chaseAcceleration;
                agent.speed           = chaseSpeed;
                agent.stoppingDistance = 0f;
                agent.updateRotation  = false;
                isIdling = false;
                break;
            
            case State.Attack:
                agent.isStopped       = true;
                agent.updateRotation  = false;
                isIdling = false;
                break;
            
            case State.HitReact:
                if (isStunned)
                {
                    if (maintainMomentumDuringHit)
                        hitReactionVelocity = agent.velocity * 0.3f;
                    
                    agent.isStopped = true;
                }
                isIdling = false;
                break;
            
            case State.Dead:
                agent.isStopped = true;
                if (agent.enabled)
                {
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();
                }
                isIdling = false;
                attackInProgress = false;
                isStunned = false;
                break;
        }
    }

    protected void FacePlayer()
    {
        if (player == null || isDead) return;
        
        Vector3 lookDir = (PlayerPos - transform.position).WithY(0);
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * playerFacingSpeed);
        }
    }

    bool HasLineOfSight()
    {
        if (player == null || isDead) return false;
        
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = (PlayerPos - eye).normalized;
        float dist = Vector3.Distance(eye, PlayerPos);

        return !Physics.Raycast(eye, dir, dist, obstacleMask);
    }

    protected abstract IEnumerator PrimaryAttack();
    
    protected virtual void ResetAttackTriggers()
    {
    }
    
    public virtual void TriggerHitReaction()
    {
        if (isDead || isInHitReaction || currentState == State.Attack) return;
        
        previousState = currentState;
        isInHitReaction = true;
        
        float reactionDuration = hitReactionDuration;
        
        isStunned = Random.value < hitStunChance;
        if (isStunned)
        {
            reactionDuration += hitStunDuration;
        }
        
        hitReactionTimer = reactionDuration;
        
        animator.SetTrigger(hitReactTriggerHash);
        
        EnterState(State.HitReact);
    }
    
    public virtual void TriggerDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        StopAllCoroutines();
        
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerHash);
            animator.SetFloat(speedHash, 0f);
            animator.SetInteger(stateHash, (int)State.Dead);
            
            animator.ResetTrigger(hitReactTriggerHash);
            
            ResetAttackTriggers();
        }
        
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
        
        EnterState(State.Dead);
        
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(destroyDelay);
        
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        
        Destroy(gameObject);
    }
}

static class Vector3Extensions
{
    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
}
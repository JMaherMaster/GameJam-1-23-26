using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class MonsterAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;

    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float returnToStartThreshold = 0.5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackAnimationLength = 1.5f; // Total length of attack animation
    [SerializeField] private float damageAtPercent = 0.5f; // When to apply damage (0.5 = halfway through)

    [Header("Idle Settings")]
    [SerializeField] private float idleAnimationLength = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool showGizmos = true;

    // References
    private Transform player;
    private Animator animator;
    private Rigidbody rb;
    private Vector3 startPosition;

    // State tracking
    private enum State { Idle, Chasing, Attacking, Returning }
    private State currentState = State.Idle;

    private bool canSeePlayer = false;
    private bool isPlayingAnimation = false;
    private string currentAnimation = "";
    private string currentChaseAnimation = "";
    private bool hasLostPlayer = false;

    // Animation state names
    private string[] idleAnimations = { "idle1", "idle2", "idle3", "idle4" };
    private string[] chaseAnimations = { "run1", "run2", "run3" };
    private string[] walkAnimations = { "walk2", "walk3", "walk4" };
    private string[] attackAnimations = { "attack1", "attack2", "attack3", "attack4", "attack5" };

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Store starting position
        startPosition = transform.position;

        // Configure Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("No player found! Make sure player has 'Player' tag.");

        // CRITICAL: Stop animator from auto-playing
        animator.speed = 1f;

        // Start with idle
        StartCoroutine(IdleAnimationLoop());
        StartCoroutine(DetectionRoutine());
    }

    void Update()
    {
        if (player == null) return;

        // Handle state logic
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Chasing:
                HandleChaseState();
                break;
            case State.Attacking:
                HandleAttackState();
                break;
            case State.Returning:
                HandleReturningState();
                break;
        }

        // Debug info
        if (showDebugInfo)
        {
            float dist = player != null ? Vector3.Distance(transform.position, player.position) : 0f;
            Debug.Log($"[MONSTER] State: {currentState} | See: {canSeePlayer} | Dist: {dist:F1} | Anim: {currentAnimation}");
        }
    }

    void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics
        if (currentState == State.Chasing)
        {
            MoveTowardsPlayer();
        }
        else if (currentState == State.Returning)
        {
            MoveTowardsStart();
        }
    }

    IEnumerator DetectionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            CheckPlayerDetection();
        }
    }

    void CheckPlayerDetection()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= fieldOfView / 2f)
            {
                // Check for obstructions - only if obstruction layer is set
                if (obstructionLayer.value == 0)
                {
                    canSeePlayer = true;
                    return;
                }
                else
                {
                    if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionLayer))
                    {
                        canSeePlayer = true;
                        return;
                    }
                }
            }
        }

        canSeePlayer = false;
    }

    IEnumerator IdleAnimationLoop()
    {
        while (currentState == State.Idle)
        {
            if (!isPlayingAnimation)
            {
                PlayRandomIdleAnimation();
                yield return new WaitForSeconds(idleAnimationLength);
            }
            else
            {
                yield return null;
            }
        }
    }

    void HandleIdleState()
    {
        // Transition to chase if player detected
        if (canSeePlayer)
        {
            StopAllCoroutines();
            StartCoroutine(DetectionRoutine());
            TransitionToChase();
        }
    }

    void HandleChaseState()
    {
        // Lost sight of player
        if (!canSeePlayer)
        {
            hasLostPlayer = true;
            TransitionToReturning();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            TransitionToAttack();
            return;
        }

        // FORCE the chase animation to keep playing/looping
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Check if we're playing the correct animation
        if (!stateInfo.IsName(currentChaseAnimation))
        {
            // Not playing the right animation, force it
            ForcePlayAnimation(currentChaseAnimation);
        }
        else if (stateInfo.normalizedTime >= 0.95f)
        {
            // Animation is almost done, loop it back to start
            animator.Play(currentChaseAnimation, 0, 0f);
        }

        // Rotate towards player
        RotateTowardsPlayer();
    }

    void HandleReturningState()
    {
        // If we see the player again while returning, chase!
        if (canSeePlayer)
        {
            hasLostPlayer = false;
            TransitionToChase();
            return;
        }

        // Check if we're back at start position
        float distanceToStart = Vector3.Distance(transform.position, startPosition);

        if (distanceToStart <= returnToStartThreshold)
        {
            // Made it back home
            TransitionToIdle();
            return;
        }

        // FORCE walk animation to keep playing/looping
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Check if animation is almost done, loop it
        if (stateInfo.normalizedTime >= 0.95f)
        {
            animator.Play(currentAnimation, 0, 0f);
        }

        // Keep rotating towards start
        RotateTowardsStart();
    }

    void MoveTowardsPlayer()
    {
        if (player == null || !canSeePlayer) return;

        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 movement = direction * chaseSpeed * Time.fixedDeltaTime;

        // Move using Rigidbody
        rb.MovePosition(rb.position + movement);
    }

    void MoveTowardsStart()
    {
        Vector3 direction = (startPosition - transform.position).normalized;
        Vector3 movement = direction * walkSpeed * Time.fixedDeltaTime;

        // Move using Rigidbody
        rb.MovePosition(rb.position + movement);
    }

    void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep rotation on Y axis only

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
        }
    }

    void RotateTowardsStart()
    {
        Vector3 direction = (startPosition - transform.position).normalized;
        direction.y = 0; // Keep rotation on Y axis only

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
        }
    }

    void HandleAttackState()
    {
        // Lost sight of player
        if (!canSeePlayer)
        {
            hasLostPlayer = true;
            TransitionToReturning();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Player moved too far
        if (distanceToPlayer > attackRange * 1.5f)
        {
            TransitionToChase();
            return;
        }

        // Keep facing player
        RotateTowardsPlayer();

        // Perform attack if not already attacking
        if (!isPlayingAnimation)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isPlayingAnimation = true;

        // Play random attack animation
        string randomAttack = attackAnimations[Random.Range(0, attackAnimations.Length)];
        ForcePlayAnimation(randomAttack);

        if (showDebugInfo)
        {
            Debug.Log($"<color=orange>[MONSTER] Starting attack: {randomAttack}</color>");
        }

        // Wait until the damage point in the animation (e.g., halfway through)
        float damageDelay = attackAnimationLength * damageAtPercent;
        yield return new WaitForSeconds(damageDelay);

        // Check ONE TIME if we should deal damage
        if (player == null)
        {
            if (showDebugInfo) Debug.Log("<color=yellow>[MONSTER] No player found</color>");
            isPlayingAnimation = false;
            yield break;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>[MONSTER] Damage check - Distance: {distanceToPlayer:F2} | Range: {attackRange} | Can see: {canSeePlayer}</color>");
        }

        // Apply damage ONLY ONCE if player is in range
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"<color=red>[MONSTER] *** DEALING {attackDamage} DAMAGE TO PLAYER ***</color>");
                }

                playerHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("[MONSTER] Player has no PlayerHealth component!");
            }
        }
        else if (showDebugInfo)
        {
            Debug.Log($"<color=yellow>[MONSTER] Attack missed - player too far ({distanceToPlayer:F2} > {attackRange})</color>");
        }

        // Wait for rest of animation + cooldown
        float remainingTime = attackAnimationLength * (1f - damageAtPercent);
        yield return new WaitForSeconds(remainingTime + attackCooldown);

        if (showDebugInfo)
        {
            Debug.Log("<color=green>[MONSTER] Attack complete, ready for next attack</color>");
        }

        isPlayingAnimation = false;
    }

    void PlayRandomIdleAnimation()
    {
        string randomIdle = idleAnimations[Random.Range(0, idleAnimations.Length)];
        ForcePlayAnimation(randomIdle);
    }

    void ForcePlayAnimation(string animationName)
    {
        if (currentAnimation != animationName)
        {
            currentAnimation = animationName;

            // Force play the animation and prevent auto-transitions
            animator.Play(animationName, 0, 0f);
            animator.Update(0f);

            if (showDebugInfo)
            {
                Debug.Log($"<color=cyan>Playing: {animationName}</color>");
            }
        }
    }

    void TransitionToIdle()
    {
        if (currentState != State.Idle)
        {
            currentState = State.Idle;
            isPlayingAnimation = false;
            currentChaseAnimation = "";
            hasLostPlayer = false;

            StopAllCoroutines();
            StartCoroutine(DetectionRoutine());
            StartCoroutine(IdleAnimationLoop());

            if (showDebugInfo)
            {
                Debug.Log("<color=yellow>→ IDLE STATE</color>");
            }
        }
    }

    void TransitionToChase()
    {
        if (currentState != State.Chasing)
        {
            currentState = State.Chasing;
            isPlayingAnimation = false;

            // Pick ONE chase animation and stick with it for this entire chase sequence
            currentChaseAnimation = chaseAnimations[Random.Range(0, chaseAnimations.Length)];
            ForcePlayAnimation(currentChaseAnimation);

            if (showDebugInfo)
            {
                Debug.Log($"<color=green>→ CHASE STATE (using {currentChaseAnimation})</color>");
            }
        }
    }

    void TransitionToAttack()
    {
        if (currentState != State.Attacking)
        {
            currentState = State.Attacking;
            isPlayingAnimation = false;

            if (showDebugInfo)
            {
                Debug.Log("<color=red>→ ATTACK STATE</color>");
            }
        }
    }

    void TransitionToReturning()
    {
        if (currentState != State.Returning)
        {
            currentState = State.Returning;
            isPlayingAnimation = false;

            // Pick a random walk animation for returning
            string walkAnim = walkAnimations[Random.Range(0, walkAnimations.Length)];
            ForcePlayAnimation(walkAnim);

            if (showDebugInfo)
            {
                Debug.Log($"<color=blue>→ RETURNING STATE (using {walkAnim})</color>");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Field of view (blue)
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView / 2f, transform.up) * transform.forward * detectionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView / 2f, transform.up) * transform.forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

        // Draw line to player if detected
        if (Application.isPlaying && canSeePlayer && player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position);
        }

        // Forward direction (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
    }
}
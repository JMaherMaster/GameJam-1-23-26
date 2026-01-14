using UnityEngine;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;

    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 10;

    [Header("Idle Settings")]
    [SerializeField] private float idleAnimationLength = 3f; // Adjust based on your animation length

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // References
    private Transform player;
    private Animator animator;
    private CharacterController characterController;

    // State tracking
    private enum State { Idle, Chasing, Attacking }
    private State currentState = State.Idle;
    private State previousState = State.Idle;

    private bool canSeePlayer = false;
    private bool isPlayingAnimation = false;
    private string currentAnimation = "";

    // Animation state names
    private string[] idleAnimations = { "idle1", "idle2", "idle3", "idle4" };
    private string[] chaseAnimations = { "run1", "run2", "run3" };
    private string[] attackAnimations = { "attack1", "attack2", "attack3", "attack4", "attack5" };

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("No player found! Make sure player has 'Player' tag.");

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
        }

        // Debug info
        if (showDebugInfo)
        {
            Debug.Log($"State: {currentState} | Can See Player: {canSeePlayer} | Current Anim: {currentAnimation}");
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
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionLayer))
                {
                    canSeePlayer = true;
                    return;
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
            TransitionToIdle();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            TransitionToAttack();
            return;
        }

        // Move towards player
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 movement = direction * chaseSpeed * Time.deltaTime;
        movement.y = -9.81f * Time.deltaTime;

        characterController.Move(movement);

        // Rotate towards player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void HandleAttackState()
    {
        // Lost sight of player
        if (!canSeePlayer)
        {
            TransitionToIdle();
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
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

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
        PlayAnimation(randomAttack, 0);

        // Wait for damage frame (adjust timing based on your animations)
        yield return new WaitForSeconds(0.6f);

        // Deal damage if still in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && canSeePlayer)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        // Wait for attack cooldown
        yield return new WaitForSeconds(attackCooldown);

        isPlayingAnimation = false;
    }

    void PlayRandomIdleAnimation()
    {
        string randomIdle = idleAnimations[Random.Range(0, idleAnimations.Length)];
        PlayAnimation(randomIdle, 0);
    }

    void PlayRandomChaseAnimation()
    {
        string randomChase = chaseAnimations[Random.Range(0, chaseAnimations.Length)];
        PlayAnimation(randomChase, 0);
    }

    void PlayAnimation(string animationName, int layer)
    {
        if (currentAnimation != animationName)
        {
            currentAnimation = animationName;
            animator.Play(animationName, layer, 0f);

            if (showDebugInfo)
            {
                Debug.Log($"Playing animation: {animationName}");
            }
        }
    }

    void TransitionToIdle()
    {
        if (currentState != State.Idle)
        {
            previousState = currentState;
            currentState = State.Idle;
            isPlayingAnimation = false;

            StopAllCoroutines();
            StartCoroutine(DetectionRoutine());
            StartCoroutine(IdleAnimationLoop());

            if (showDebugInfo)
            {
                Debug.Log("Transitioning to IDLE");
            }
        }
    }

    void TransitionToChase()
    {
        if (currentState != State.Chasing)
        {
            previousState = currentState;
            currentState = State.Chasing;
            isPlayingAnimation = false;

            PlayRandomChaseAnimation();

            if (showDebugInfo)
            {
                Debug.Log("Transitioning to CHASE");
            }
        }
    }

    void TransitionToAttack()
    {
        if (currentState != State.Attacking)
        {
            previousState = currentState;
            currentState = State.Attacking;
            isPlayingAnimation = false;

            if (showDebugInfo)
            {
                Debug.Log("Transitioning to ATTACK");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
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
    }
}
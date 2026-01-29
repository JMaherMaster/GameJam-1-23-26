using UnityEngine;
using System.Collections;

public class BatController : MonoBehaviour
{
    [Header("Bat References")]
    [SerializeField] private Transform batTransform;
    [SerializeField] private BatWeapon batWeapon;

    [Header("Swing Settings")]
    [SerializeField] private float swingDuration = 0.4f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Bat Positions")]
    [SerializeField] private Vector3 restPosition = new Vector3(0.5f, -0.3f, 0.6f);
    [SerializeField] private Vector3 restRotation = new Vector3(0f, -30f, 0f);

    [SerializeField] private Vector3 windupPosition = new Vector3(0.7f, -0.2f, 0.5f);
    [SerializeField] private Vector3 windupRotation = new Vector3(-10f, -60f, -20f);

    [SerializeField] private Vector3 swingPosition = new Vector3(-0.5f, 0f, 0.7f);
    [SerializeField] private Vector3 swingRotation = new Vector3(10f, 60f, 30f);

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isSwinging = false;
    private bool canAttack = true;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Set bat to rest position
        if (batTransform != null)
        {
            batTransform.localPosition = restPosition;
            batTransform.localEulerAngles = restRotation;
        }
        else
        {
            Debug.LogError("[BAT CONTROLLER] Bat Transform not assigned!");
        }

        if (batWeapon == null)
        {
            Debug.LogError("[BAT CONTROLLER] Bat Weapon not assigned!");
        }

        // Find player health component
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("[BAT CONTROLLER] PlayerHealth component not found in scene!");
        }
    }

    void Update()
    {
        // Don't allow attacks if player is dead
        if (playerHealth != null && playerHealth.IsDead())
        {
            return;
        }

        // Check for attack input
        if (Input.GetMouseButtonDown(0) && canAttack && !isSwinging)
        {
            StartCoroutine(PerformSwing());
        }
    }

    IEnumerator PerformSwing()
    {
        isSwinging = true;
        canAttack = false;

        if (showDebugLogs)
        {
            Debug.Log("<color=cyan>[BAT] Starting swing!</color>");
        }

        // PHASE 1: Windup (quick)
        float windupTime = swingDuration * 0.2f;
        yield return StartCoroutine(MoveBat(restPosition, restRotation, windupPosition, windupRotation, windupTime));

        // PHASE 2: Swing (main attack)
        float swingTime = swingDuration * 0.5f;

        // Enable hitbox at the start of the swing
        if (batWeapon != null)
        {
            batWeapon.EnableHitbox();
        }

        yield return StartCoroutine(MoveBat(windupPosition, windupRotation, swingPosition, swingRotation, swingTime));

        // Disable hitbox after swing
        if (batWeapon != null)
        {
            batWeapon.DisableHitbox();
        }

        // PHASE 3: Return to rest
        float returnTime = swingDuration * 0.3f;
        yield return StartCoroutine(MoveBat(swingPosition, swingRotation, restPosition, restRotation, returnTime));

        isSwinging = false;

        if (showDebugLogs)
        {
            Debug.Log("<color=green>[BAT] Swing complete!</color>");
        }

        // Cooldown before next attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;

        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>[BAT] Ready to attack!</color>");
        }
    }

    IEnumerator MoveBat(Vector3 startPos, Vector3 startRot, Vector3 endPos, Vector3 endRot, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveT = swingCurve.Evaluate(t);

            if (batTransform != null)
            {
                batTransform.localPosition = Vector3.Lerp(startPos, endPos, curveT);
                batTransform.localEulerAngles = Vector3.Lerp(startRot, endRot, curveT);
            }

            yield return null;
        }

        // Ensure we end exactly at target
        if (batTransform != null)
        {
            batTransform.localPosition = endPos;
            batTransform.localEulerAngles = endRot;
        }
    }

    // Public methods for debugging
    public bool IsSwinging() => isSwinging;
    public bool CanAttack() => canAttack;
}
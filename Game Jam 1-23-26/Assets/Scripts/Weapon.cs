using UnityEngine;
using System.Collections.Generic;

public class BatWeapon : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 25;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Collider weaponCollider;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    void Awake()
    {
        weaponCollider = GetComponent<Collider>();

        if (weaponCollider == null)
        {
            Debug.LogError("[BAT WEAPON] No collider found! Add a collider to the bat.");
        }
        else
        {
            // Make sure it's a trigger
            weaponCollider.isTrigger = true;
            // Start disabled
            weaponCollider.enabled = false;
        }
    }

    public void EnableHitbox()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            hitEnemies.Clear(); // Clear the list of hit enemies for this swing

            if (showDebugLogs)
            {
                Debug.Log("<color=green>[BAT WEAPON] Hitbox ENABLED</color>");
            }
        }
    }

    public void DisableHitbox()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;

            if (showDebugLogs)
            {
                Debug.Log("<color=red>[BAT WEAPON] Hitbox DISABLED</color>");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we already hit this enemy in this swing
        if (hitEnemies.Contains(other.gameObject))
        {
            return;
        }

        // Check if it's an enemy
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            MonsterHealth monsterHealth = other.GetComponent<MonsterHealth>();

            if (monsterHealth != null)
            {
                // Deal damage
                monsterHealth.TakeDamage(damage);

                // Mark this enemy as hit during this swing
                hitEnemies.Add(other.gameObject);

                if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[BAT WEAPON] HIT {other.gameObject.name} for {damage} damage!</color>");
                }
            }
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}
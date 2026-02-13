using UnityEngine;

/// <summary>
/// Stats system for Monster Hunter style enemy.
/// Tracks health, stamina, rage, stagger, and action commitment.
/// </summary>
[RequireComponent(typeof(Animator))]
public class MonsterStats : MonoBehaviour
{
    [Header("Health")]
    public float health = 1000f;
    public float maxHealth = 1000f;
    public float healthPercent => health / maxHealth;

    [Header("Stamina System")]
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f; // per second
    public float staminaDrainRate = 15f; // per second when aggressive

    [Header("Rage System")]
    public float rage = 0f;
    public float maxRage = 100f;
    public float rageDecayRate = 5f; // per second
    public bool isEnraged => rage >= maxRage;
    
    [Header("Stagger System")]
    public float stagger = 0f;
    public float staggerThreshold = 100f;
    public float staggerDecayRate = 20f; // per second
    
    [Header("State Flags")]
    public bool isKnockedDown = false;
    public bool isExhausted = false;
    public bool isDead = false;
    public bool isInAction = false; // Currently committed to an attack/action
    public bool hasBeenAttacked = false; // Has monster been aggroed/entered combat?
    
    [Header("Action Commitment")]
    public string currentAction = "";
    public float actionTimer = 0f;
    
    [Header("Part Damage (Optional)")]
    public float headHealth = 200f;
    public float tailHealth = 150f;
    public bool headBroken = false;
    public bool tailSevered = false;

    [Header("Target")]
    public Transform target; // Player

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        // Stamina regeneration when not exhausted and not in heavy action
        if (!isExhausted && !isInAction)
        {
            AddStamina(staminaRegenRate * Time.deltaTime);
        }

        // Rage decay over time
        if (!isEnraged && rage > 0)
        {
            rage -= rageDecayRate * Time.deltaTime;
            rage = Mathf.Max(0, rage);
        }

        // Stagger decay over time
        if (stagger > 0)
        {
            stagger -= staggerDecayRate * Time.deltaTime;
            stagger = Mathf.Max(0, stagger);
        }

        // Action timer countdown
        if (actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0)
            {
                isInAction = false;
                currentAction = "";
            }
        }

        // Check for exhaustion
        if (stamina <= 0 && !isExhausted)
        {
            isExhausted = true;
            Debug.Log("[Monster] Exhausted!");
        }
    }

    public void TakeDamage(float damage, bool isHeadshot = false, bool isTailHit = false)
    {
        if (isDead) return;

        // Mark as having been attacked (aggro trigger)
        hasBeenAttacked = true;

        health -= damage;
        health = Mathf.Max(0, health);

        // Add rage when damaged
        AddRage(damage * 0.3f);

        // Part damage
        if (isHeadshot && !headBroken)
        {
            headHealth -= damage * 1.5f;
            if (headHealth <= 0)
            {
                headBroken = true;
                AddStagger(50f); // Extra stagger on part break
                Debug.Log("[Monster] Head broken!");
            }
        }

        if (isTailHit && !tailSevered)
        {
            tailHealth -= damage * 1.2f;
            if (tailHealth <= 0)
            {
                tailSevered = true;
                AddStagger(30f);
                Debug.Log("[Monster] Tail severed!");
            }
        }

        // Check for death
        if (health <= 0)
        {
            isDead = true;
        }

        // Visual feedback
        Debug.Log($"[Monster] Took {damage} damage! Health: {health}/{maxHealth}");
    }

    public void AddStagger(float amount)
    {
        if (isKnockedDown) return; // Can't stagger while already down

        stagger += amount;
        
        if (stagger >= staggerThreshold && !isKnockedDown)
        {
            isKnockedDown = true;
            stagger = 0;
            Debug.Log("[Monster] KNOCKED DOWN!");
        }
    }

    public void AddRage(float amount)
    {
        rage += amount;
        rage = Mathf.Clamp(rage, 0, maxRage);

        if (isEnraged)
        {
            Debug.Log("[Monster] ENRAGED!");
        }
    }

    public void AddStamina(float amount)
    {
        stamina += amount;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // Recover from exhaustion
        if (isExhausted && stamina >= maxStamina * 0.5f)
        {
            isExhausted = false;
            Debug.Log("[Monster] Recovered from exhaustion!");
        }
    }

    public void ConsumeStamina(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Max(0, stamina);
    }

    /// <summary>
    /// Commit to an action for a specific duration.
    /// During this time, isInAction will be true and the monster won't change behavior.
    /// </summary>
    public void CommitToAction(string actionName, float duration)
    {
        isInAction = true;
        currentAction = actionName;
        actionTimer = duration;
        
        // Play animation if it exists
        if (animator != null && animator.HasState(0, Animator.StringToHash(actionName)))
        {
            animator.CrossFade(actionName, 0.1f);
        }
        
        Debug.Log($"[Monster] Committed to {actionName} for {duration}s");
    }

    /// <summary>
    /// Get distance to target
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.position);
    }

    /// <summary>
    /// Check if target is in front of monster
    /// </summary>
    public bool IsTargetInFront(float angleThreshold = 60f)
    {
        if (target == null) return false;
        
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        
        return angle < angleThreshold;
    }

    /// <summary>
    /// Get position behind the monster (for tail attacks)
    /// </summary>
    public bool IsTargetBehind(float angleThreshold = 120f)
    {
        if (target == null) return false;
        
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        
        return angle > angleThreshold;
    }
}

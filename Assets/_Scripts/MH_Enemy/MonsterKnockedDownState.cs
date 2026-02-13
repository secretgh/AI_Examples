using UnityEngine;

/// <summary>
/// Knocked Down State - Monster is toppled/staggered from taking too much damage.
/// This is a critical vulnerability window where players can deal massive damage.
/// Monster recovers after a set duration.
/// </summary>
public class MonsterKnockedDownState : HybridState
{
    private MonsterAI monster;
    private float knockdownDuration = 5f; // Vulnerable for 5 seconds
    private float knockdownTimer;
    private float getUpAnimationDuration = 2f;
    private bool isGettingUp = false;

    public MonsterKnockedDownState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Knocked Down";
    }

    public override int Priority => 80; // Very high priority

    public override bool CanEnter()
    {
        return monster.stats.isKnockedDown && !monster.stats.isDead;
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("[Monster] KNOCKED DOWN! Vulnerable to attacks!");

        knockdownTimer = knockdownDuration;
        isGettingUp = false;

        // Stop all movement
        monster.agent.isStopped = true;
        monster.agent.velocity = Vector3.zero;

        // Play knockdown animation
        if (monster.animator != null)
        {
            monster.animator.CrossFade("KnockedDown", 0.1f);
        }

        // Clear action commitment
        monster.stats.isInAction = false;
        monster.stats.actionTimer = 0;

        // Reset stagger accumulation
        monster.stats.stagger = 0;
    }

    public override void Update()
    {
        knockdownTimer -= Time.deltaTime;

        // Start getting up animation near the end
        if (!isGettingUp && knockdownTimer <= getUpAnimationDuration)
        {
            isGettingUp = true;
            
            if (monster.animator != null)
            {
                monster.animator.CrossFade("GetUp", 0.2f);
            }
            
            Debug.Log("[Monster] Getting back up...");
        }

        // Recovery complete
        if (knockdownTimer <= 0)
        {
            monster.stats.isKnockedDown = false;
            
            // Small stamina cost for getting knocked down
            monster.stats.ConsumeStamina(20f);
            
            Debug.Log("[Monster] Recovered from knockdown!");
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Re-enable movement
        monster.agent.isStopped = false;
        
        // Add some rage from being knocked down (monster is angry!)
        monster.stats.AddRage(30f);
    }
}

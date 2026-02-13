using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Monster Hunter style enemy AI using Hybrid FSM + Behavior Tree system.
/// 
/// State Priority:
/// - Dead (100): Terminal state
/// - Knocked Down (80): Staggered/toppled
/// - Exhausted (60): Out of stamina
/// - Enraged (40): High rage state
/// - Hunting (0): Normal combat
/// 
/// Features:
/// - Action commitment (animations must complete)
/// - Stamina management (attacks drain, exhaustion when empty)
/// - Rage system (builds from damage, enhances attacks)
/// - Stagger system (enough damage = knockdown)
/// - Part breaking (head/tail)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterStats))]
public class MonsterAI : MonoBehaviour
{
    [Header("References")]
    public MonsterStats stats;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform lairLocation; // Where monster retreats to rest/heal

    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float enragedSpeedMultiplier = 1.3f;

    [Header("Attack Ranges")]
    public float meleeRange = 4f;
    public float chargeRange = 15f;
    public float maxEngageRange = 30f;

    public HybridStateMachine StateMachine { get; private set; }

    // State references
    public MonsterDeadState DeadState { get; private set; }
    public MonsterKnockedDownState KnockedDownState { get; private set; }
    public MonsterRetreatingState RetreatingState { get; private set; }
    public MonsterExhaustedState ExhaustedState { get; private set; }
    public MonsterEnragedState EnragedState { get; private set; }
    public MonsterRelocatingState RelocatingState { get; private set; }
    public MonsterHuntingState HuntingState { get; private set; }
    public MonsterWanderingState WanderingState { get; private set; }

    void Awake()
    {
        if (stats == null) stats = GetComponent<MonsterStats>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        InitializeStateMachine();
    }

    void Start()
    {
        // Start in wandering state (peaceful until aggroed)
        StateMachine.ChangeState(WanderingState);
    }

    void Update()
    {
        StateMachine.Tick();
        UpdateAnimator();
    }

    private void InitializeStateMachine()
    {
        StateMachine = new HybridStateMachine();

        // Create states in priority order
        DeadState = new MonsterDeadState(this);                // Priority: 100 (Critical)
        KnockedDownState = new MonsterKnockedDownState(this);  // Priority: 80 (Very High)
        RetreatingState = new MonsterRetreatingState(this);    // Priority: 70 (High)
        ExhaustedState = new MonsterExhaustedState(this);      // Priority: 60 (High)
        EnragedState = new MonsterEnragedState(this);          // Priority: 40 (Medium)
        RelocatingState = new MonsterRelocatingState(this);    // Priority: 35 (Medium)
        HuntingState = new MonsterHuntingState(this);          // Priority: 0 (Normal)
        WanderingState = new MonsterWanderingState(this);      // Priority: -10 (Low/Peaceful)

        // Register all states
        StateMachine.RegisterStates(
            DeadState,
            KnockedDownState,
            RetreatingState,
            ExhaustedState,
            EnragedState,
            RelocatingState,
            HuntingState,
            WanderingState
        );
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Set animator parameters based on state
        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("IsEnraged", stats.isEnraged);
        animator.SetBool("IsExhausted", stats.isExhausted);
        animator.SetBool("IsKnockedDown", stats.isKnockedDown);
        animator.SetBool("IsDead", stats.isDead);
    }

    /// <summary>
    /// Face toward target over time
    /// </summary>
    public void FaceTarget(float rotationSpeed = 5f)
    {
        if (stats.target == null) return;

        Vector3 direction = (stats.target.position - transform.position).normalized;
        direction.y = 0; // Keep on horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Instantly face target (for committed attacks)
    /// </summary>
    public void SnapToTarget()
    {
        if (stats.target == null) return;

        Vector3 direction = (stats.target.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxEngageRange);
    }
}

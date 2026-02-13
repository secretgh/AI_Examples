using UnityEngine;
using UnityEngine.AI;

public abstract class AIController : MonoBehaviour
{
    public NPCStats Stats;
    public NavMeshAgent agent;
    public Transform Bed;
    public Transform Bush;
}
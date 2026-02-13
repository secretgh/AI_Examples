using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using TMPro;

public class GoapDebugUI : MonoBehaviour
{
    public GoapNPCController agent;
    public TextMeshProUGUI debugText;

    void Update()
    {
        if (agent == null || debugText == null)
            return;

        var sb = new StringBuilder();

        sb.AppendLine("=== NPC Stats ===");
        sb.AppendLine($"Health: {agent.Stats.health:F0}");
        sb.AppendLine($"Hunger: {agent.Stats.hunger:F0}");
        sb.AppendLine($"Fatigue: {agent.Stats.fatigue:F0}");
        sb.AppendLine($"Food: {agent.Stats.food}");
        sb.AppendLine($"IsEating: {agent.Stats.isEating}");
        sb.AppendLine($"IsSleeping: {agent.Stats.isSleeping}");

        sb.AppendLine("\n=== Current GOAP Action ===");
        sb.AppendLine(agent.currentAction != null ? agent.currentAction.Name : "Idle");

        sb.AppendLine("\n=== Planned Actions ===");
        if (agent.currentPlan != null && agent.currentPlan.Count > 0)
        {
            foreach (var a in agent.currentPlan)
                sb.AppendLine("- " + a.Name);
        }
        else
        {
            sb.AppendLine("None");
        }

        debugText.text = sb.ToString();
    }
}
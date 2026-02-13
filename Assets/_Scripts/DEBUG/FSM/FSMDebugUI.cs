using TMPro;
using UnityEngine;

public class FSMDebugUI : MonoBehaviour
{
    public FSM_AIController controller;
    public TextMeshProUGUI currentStateText;
    public Transform possibleStatesRoot;
    public GameObject stateRowPrefab;

    void Update()
    {
        currentStateText.text = $"{controller.FSM.CurrentState.Name}";

        foreach (Transform child in possibleStatesRoot)
            Destroy(child.gameObject);

        foreach (var state in controller.FSM.AllStates)
        {
            if (state == controller.FSM.CurrentState) continue;

            var row = Instantiate(stateRowPrefab, possibleStatesRoot);
            var label = row.GetComponent<TextMeshProUGUI>();

            bool canEnter = state.CanEnter();
            label.text = $"[{state.Name}] - {(canEnter ? "Ok" : "No")}";
            label.color = canEnter ? Color.green : Color.red;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IDebugStats : MonoBehaviour
{
    [Header("References")]
    public NPCStats stats;

    [Header("UI Elements")]
    public Slider health;
    private bool h_flag = false;
    public Slider hunger;
    private bool hunger_flag = false;
    public Slider fatigue;
    private bool fatigue_flag = false;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI dayText;

    private void Start()
    {
        health.onValueChanged.AddListener((f) => { ModifyHealth(f); });
        hunger.onValueChanged.AddListener((f) => { ModifyHunger(f); });
        fatigue.onValueChanged.AddListener((f) => { ModifyFatigue(f); });
    }

    void Update()
    {
        if (!h_flag)
            health.value = stats.health;
        if (!hunger_flag)
            hunger.value = stats.hunger;
        if (!fatigue_flag)
            fatigue.value = stats.fatigue;

        foodText.text = $"Food: {stats.food:0}";
        dayText.text = $"Day Time: {Time.time % GameTime.Day:0}/{GameTime.Day}s";
    }

    // Debug buttons
    public void ModifyHunger(float value) { 
        stats.hunger = value; 
    }
    public void ModifyFatigue(float value) { 
        stats.fatigue = (value); 
    }
    public void ModifyHealth(float value) { 
        stats.health = (value); 
    }
    public void ModifyFood(int value) {
        stats.food = (value); 
    }
}

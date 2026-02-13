using UnityEngine;

[RequireComponent(typeof(NPCEventPopup))]
public class NPCStats : MonoBehaviour
{
    public float health = 100f;
    public float hunger = 0f;
    public float fatigue = 0f;
    public int food = 2;

    public const float MaxValue = 100f;

    [Header("State Flags")]
    public bool isSleeping = false;
    public bool isEating = false;
    public bool isDead = false;

    [Header("Decay Rates (per Day)")]
    public float hungerPerDay = 120f; //eat 2 times a day
    public float fatiguePerDay = 100f; // go to bed once

    private NPCEventPopup popup;

    private void Awake()
    {
        popup = GetComponent<NPCEventPopup>();
    }

    void Update()
    {
        if (!isDead)
        {
            float dayFraction = Time.deltaTime / GameTime.Day;

            if (!isEating)
                AddHunger(hungerPerDay * dayFraction);     // Needs to eat twice/day
            if (!isSleeping)
                AddFatigue(fatiguePerDay * dayFraction);
            if (hunger >= MaxValue)
                AddHealth(-(2*GameTime.Day) * dayFraction); //Gets 2 days worth of vitality
        }

    }

    public void AddHealth(float h)
    {
        if (Mathf.Abs(h) > 1)
            popup.SpawnTextPopup(
                target: transform,
                offset: new Vector3(-1f, 3f, 0),
                duration: 2f,
                text: $"{(h >= 0 ? '+' : "")}{h:0} Health",
                color: h >= 0 ? Color.green : Color.red
                );

        health = Mathf.Clamp(health+h, 0, 100f);
    }
    public void AddFatigue(float f)
    {
        if (Mathf.Abs(f) > 1)
            popup.SpawnTextPopup(
                target: transform,
                offset: new Vector3(0, 3f, 0),
                duration: 2f,
                text: $"{(f >= 0 ? '+' : "")}{f:0} Fatigue",
                color: f <= 0 ? Color.green : Color.red
                );

        fatigue = Mathf.Clamp(fatigue+f, 0, MaxValue);
    }
    public void AddHunger(float h)
    {
        if(Mathf.Abs(h) > 1)
            popup.SpawnTextPopup(
                target: transform,
                offset: new Vector3(0f, 3f, 0),
                duration: 2f,
                text: $"{(h >= 0 ? '+' : "")}{h:0} Hunger",
                color: h <= 0 ? Color.green : Color.red
                );
        hunger = Mathf.Clamp(hunger+h, 0, MaxValue);
    }

    public void AddFood(int f)
    {
        if (Mathf.Abs(f) > 1)
            popup.SpawnTextPopup(
                target: transform,
                offset: new Vector3(0, 3f, 0),
                duration: 2f,
                text: $"{(f >= 0 ? '+' : "")}{f:0} Food",
                color: f >= 0 ? Color.green : Color.red
                );
        food = Mathf.Clamp(food+f, 0, 100);
    }
}

using System;
using UnityEngine;

public class ActionNode : Node
{
    float duration;
    float timer;
    Action onStart;
    Action onComplete;
    bool started;

    public ActionNode(string name, float duration, Action onStart, Action onComplete) : base(name)
    {
        this.duration = duration;
        this.onStart = onStart;
        this.onComplete = onComplete;
    }

    public override NodeStatus Tick()
    {
        if (!started)
        {
            started = true;
            timer = duration;
            onStart?.Invoke();
        }

        timer -= Time.deltaTime;

        if (timer > 0) {
            status = NodeStatus.Running;
            return NodeStatus.Running;
        }

        onComplete?.Invoke();
        started = false;
        status = NodeStatus.Success;
        return NodeStatus.Success;
    }
}

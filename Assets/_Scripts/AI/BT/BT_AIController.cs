using UnityEngine;
using System.Collections.Generic;

public class BT_AIController : AIController
{
    public BehaviorTree BT;
    public BTDebugUI debug;

    void Start()
    {
        BT = new NPC_Normal(this);
        debug.SetTree(BT);
    }

    void Update()
    {
        if (BT != null)
            BT.Tick();
    }


}

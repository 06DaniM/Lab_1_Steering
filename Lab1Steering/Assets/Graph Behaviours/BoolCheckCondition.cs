using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Bool check", story: "Is a bool true?", category: "Conditions", id: "18c8442e51791b0a5d0c204953a9b8b4")]
public partial class BoolCheckCondition : Condition
{

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}

using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Bool check", story: "if [x]", category: "Conditions", id: "13dc219e10b55f52c9b461da43942852")]
public partial class BoolCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> X;

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

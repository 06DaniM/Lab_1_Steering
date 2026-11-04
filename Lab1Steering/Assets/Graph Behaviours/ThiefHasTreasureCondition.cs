using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Thief Has Treasure", story: "[Thief] Has [Treasure] =", category: "Conditions", id: "f211b87642eccb79cdcfcb3c0c876dbe")]
public partial class ThiefHasTreasureCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Thief;
    [SerializeReference] public BlackboardVariable<bool> Treasure;

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

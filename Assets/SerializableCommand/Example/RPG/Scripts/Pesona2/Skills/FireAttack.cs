using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class FireAttack : RPGCommandBase
{
    public string description = "This is Fire Attack";
    public AttributeType attributeType = AttributeType.Fire;

    public override IEnumerator Execute()
    {
        if (targets.Count == 0) yield break;

        targets[0].CharacterData.hp -= Mathf.Max(invoker.CharacterData.atk - targets[0].CharacterData.def, 0);

        yield break;
    }

    public override IEnumerator Undo()
    {
        if (targets.Count == 0) yield break;

        targets[0].CharacterData.hp += Mathf.Max(invoker.CharacterData.atk - targets[0].CharacterData.def, 0);

        yield break;
    }

    //public override CommandData Serialize()
    //{
    //    commandData = new CommandData();

    //    commandData.CommandType = GetType().ToString();
    //    //commandData.JsonData = JsonUtility.ToJson(this);

    //    //Debug.Log($"[FireAttack] {commandData.JsonData}");

    //    return commandData;
    //}

    public override void Deserialize(CommandData commandData)
    {
        //throw new System.NotImplementedException();
    }
}

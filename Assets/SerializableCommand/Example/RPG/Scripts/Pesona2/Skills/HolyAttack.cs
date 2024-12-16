using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyAttack : RPGCommandBase
{
    public string description = "This is Holy Attack";
    public AttributeType attributeType = AttributeType.Holy;

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
    //    CommandData data = new CommandData();

    //    data.CommandType = "HolyAttack";
    //    data.JsonData = JsonUtility.ToJson(this);

    //    Debug.Log(data.JsonData);

    //    return data;
    //}

    public override void Deserialize(CommandData commandData)
    {
        //throw new System.NotImplementedException();
    }
}

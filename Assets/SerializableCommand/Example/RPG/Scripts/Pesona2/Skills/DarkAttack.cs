using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkAttack : RPGCommandBase
{
    public string description = "This is Dark Attack";
    public AttributeType attributeType = AttributeType.Dark;

    public override IEnumerator Execute()
    {
        //Debug.Log($"{targets.Count}");
        if (targets.Count == 0) yield break;

        var value = Mathf.Max(invoker.CharacterData.atk - targets[0].CharacterData.def, 0);

        //Debug.Log($"{targets[0].CharacterData.hp} - {value}");
        targets[0].CharacterData.hp -= value;


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

    //    data.CommandType = "DarkAttack";
    //    data.JsonData = JsonUtility.ToJson(this);

    //    Debug.Log(data.JsonData);

    //    return data;
    //}

    public override void Deserialize(CommandData commandData)
    {
        //throw new System.NotImplementedException();
    }
}

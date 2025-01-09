using SimpleJSON;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;

//TODO: Data kayak traversalTime sama movementData sebaiknya diilangin, karena nantinya action ngga terbatas ke movement doang
//Untuk data penunjang di-store as string/ JSON, untuk deserialize nnti diserahin ke "factory"

/*
 * Struktur JSON
 * 
 * [type]
 * contoh: List<Vector3>, DateTime
 * [data]
 * contoh:
 * [(0.1,0.2,0.2)]
 * speed: 3
 * name: "subagyo"
*/

[System.Serializable]
public class TFGActionRecord
{
    public double time;
    public TFGCharacter character;
    public ScriptGraphAsset action;
    public double duration;
    public JSONObject data;
}

[System.Serializable]
public class TFGCharacterTransformData
{
    public TFGCharacter character;
    public Vector3 worldPosition;
    public Vector3 worldRotation;
}

[System.Serializable]
public class TFGSessionData
{
    public string name;
    public string description;
    public string startTime;
    public string endTime;
    public List<TFGCharacterTransformData> initialCharacterTransforms = new();
    public List<TFGActionRecord> actionRecords = new();

    public string SerializeInitialCharacterTransforms()
    {
        var result = string.Empty;

        var JSONArray = new JSONObject();

        foreach (var initialCharacterTransform in initialCharacterTransforms)
        {
            var JSONNode = new JSONObject();

            JSONNode.Add("worldPosition", initialCharacterTransform.worldPosition.ToString());
            JSONNode.Add("worldRotation", initialCharacterTransform.worldRotation.ToString());

            JSONArray.Add(initialCharacterTransform.character.id, JSONNode);
        }

        result = JSONArray.ToString();

        return result;
    }

    public string SerializeActionRecords()
    {
        var result = string.Empty;

        var JSONArray = new JSONArray();

        foreach (var actionRecord in actionRecords)
        {
            var JSONNode = new JSONObject();

            JSONNode.Add("time", actionRecord.time);
            JSONNode.Add("character", actionRecord.character.id);
            JSONNode.Add("action", actionRecord.action.name);
            JSONNode.Add("duration", actionRecord.duration);
            JSONNode.Add("data", actionRecord.data);

            JSONArray.Add(JSONNode);
        }

        result = JSONArray.ToString();

        return result;
    }

    public string SerializeMovementData(List<Vector3> movementDatas)
    {
        string result = string.Empty;

        var JSONArray = new JSONArray();

        foreach (var movementData in movementDatas)
        {
            var movementDataArray = new JSONArray();

            movementDataArray.Add(Math.Round(movementData.x, 2, MidpointRounding.ToEven));
            movementDataArray.Add(Math.Round(movementData.y, 2, MidpointRounding.ToEven));
            movementDataArray.Add(Math.Round(movementData.z, 2, MidpointRounding.ToEven));

            JSONArray.Add(movementDataArray);
        }

        result = JSONArray.ToString();

        return result;
    }
}
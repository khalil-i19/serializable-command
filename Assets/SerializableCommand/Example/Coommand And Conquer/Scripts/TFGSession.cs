using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using JetBrains.Annotations;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using ZXing;

[System.Serializable]
public class TFGActionRecord
{
    public string time;
    public TFGCharacter character;
    public ScriptGraphAsset action;
    public float traversalTime;
    public List<Vector3> movementData;
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
            JSONNode.Add("traversalTime", actionRecord.traversalTime);
            JSONNode.Add("movementData", SerializeMovementData(actionRecord.movementData));

            JSONArray.Add(JSONNode);
        }

        result = JSONArray.ToString();

        return result;
    }

    public string SerializeMovementData(List<Vector3> movementDatas)
    {
        string result = string.Empty;

        var JSONArray = new JSONArray();

        foreach(var movementData in movementDatas)
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

public class TFGSession : MonoBehaviour
{
    [SerializeField] TFGSessionData sessionData;

    [SerializeField] List<ScriptGraphAsset> _scriptGraphAssets = new();
    [SerializeField] List<TFGCharacter> _characters = new();

    [SerializeField] DatabaseObject sessionDataHistory;

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<ScriptMachine> OnActionExecuted;

    [Header("Receivers")]
    [SerializeField] CommunicationHandler<TFGCharacter> OnCurrentCharacterUpdated;

    Dictionary<string, ScriptGraphAsset> scriptGraphAssets = new();
    Dictionary<string, TFGCharacter> characters = new();

    public Action OnSessionStart;
    public Action OnSessionStop;
    public Action OnSessionSaved;

    int characterIndex = 0;

    public List<TFGCharacter> Characters
    {
        get
        {
            return _characters;
        }
    }

    #region METHODS
    private void Awake()
    {
        OnActionExecuted.Register(this).OnReceiveSignal += RecordCommandExecution;

        characters.Clear();
        foreach (var character in _characters) characters.Add(character.id, character);
    }

    public TFGCharacter GetCharacter(string characterId) => characters[characterId];
    public ScriptGraphAsset GetScriptGraphAsset(string key) => scriptGraphAssets[key];
    #region SESSION
    [ContextMenu("Start Session")]
    public void StartSession() => StartSession(DateTime.Now.TimeOfDay.ToString());

    public void StartSession(string sessionName)
    {
        Variables.Scene(SceneManager.GetActiveScene())["replayMode"] = false;

        sessionData = new TFGSessionData() { name = sessionName, startTime = DateTime.Now.ToString() };
        foreach (var character in _characters)
        {
            var characterTransformData = new TFGCharacterTransformData();
            characterTransformData.character = character;
            characterTransformData.worldPosition = character.transform.position;
            characterTransformData.worldRotation = character.transform.eulerAngles;

            sessionData.initialCharacterTransforms.Add(characterTransformData);
        }

        characterIndex = -1;

        NextCharacter();

        OnSessionStart?.Invoke();
    }

    [ContextMenu("Stop Session")]
    public void StopSession()
    {
        sessionData.endTime = DateTime.Now.ToString();

        Variables.Scene(SceneManager.GetActiveScene())["replayMode"] = true;

        foreach(var character in _characters)
        {
            character.ClearCommand();
        }

        OnSessionStop?.Invoke();
    }

    public void NextCharacter()
    {
        characterIndex = (characterIndex + 1) % _characters.Count;

        OnCurrentCharacterUpdated.Broadcast(_characters[characterIndex]);
    }
    #endregion

    #region SCRIPT GRAPH ASSET
    public void AddScriptGraphAsset(object obj)
    {
        var scriptGraphAsset = (ScriptGraphAsset)obj;

        _scriptGraphAssets.Add(scriptGraphAsset);

        scriptGraphAssets.TryAdd(scriptGraphAsset.name, scriptGraphAsset);
    }
    #endregion

    #region RECORDS
    public void RecordCommandExecution(ScriptMachine scriptMachine)
    {
        if (scriptMachine == null) return;
        if (scriptMachine.GetComponent<TFGCharacter>() == null) return;

        var record = new TFGActionRecord();

        var invokerObj = scriptMachine.gameObject;
        var scriptGraphAsset = scriptMachine.GetCurrentScriptGraphAsset();

        //TODO: Convert sesuai timezone Indo
        var currentTime = DateTime.Now.ToString();
        var speed = (float)Variables.Scene(SceneManager.GetActiveScene())["walkSpeed"];

        record.time = currentTime;
        record.character = scriptMachine.GetComponent<TFGCharacter>();
        record.action = scriptMachine.GetCurrentScriptGraphAsset();
        record.movementData = (List<Vector3>)Variables.Object(scriptMachine)["splineData"];
        record.traversalTime = record.movementData.GetLength() / speed;

        sessionData.actionRecords.Add(record);
    }

    public void GetRecord() => ExportRecordAsJSON();

    public string ExportRecordAsJSON()
    {
        var result = string.Empty;

        var JSONNode = new JSONObject();

        JSONNode.Add("name", sessionData.name);
        JSONNode.Add("startTime", sessionData.startTime);
        JSONNode.Add("endTime", sessionData.endTime);

        var initialJSON = JSON.Parse(sessionData.SerializeInitialCharacterTransforms());
        var recordJSON = JSON.Parse(sessionData.SerializeActionRecords());

        JSONNode.Add("initialCharacterTransforms", initialJSON);
        JSONNode.Add("actionRecords", recordJSON);

        result = JSONNode.ToString();

        var historyData = sessionDataHistory.data.AddNewChild(sessionData.startTime);
        historyData.type = DatabaseObject.DataType.String;
        historyData.stringValue = result;

        OnSessionSaved?.Invoke();

        return result;
    }
    #endregion
    #endregion
}

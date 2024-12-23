using Innoveam.Modules.Communication;
using JetBrains.Annotations;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TFGActionRecord
{
    public string time;
    public TFGCharacter character;
    public ScriptGraphAsset action;
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

            JSONArray.Add(JSONNode);
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

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<ScriptMachine> OnActionExecuted;

    [Header("Receivers")]
    [SerializeField] CommunicationHandler<TFGCharacter> OnCurrentCharacterUpdated;

    Dictionary<string, ScriptGraphAsset> scriptGraphAssets = new();
    Dictionary<string, TFGCharacter> characters = new();

    public Action OnSessionStart;
    public Action OnSessionStop;

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

        //scriptGraphAssets.Clear();
        //foreach (var scriptGraphAsset in _scriptGraphAssets) scriptGraphAssets.Add(scriptGraphAsset.name, scriptGraphAsset);
    }

    public TFGCharacter GetCharacter(string characterId) => characters[characterId];
    public ScriptGraphAsset GetScriptGraphAsset(string key) => scriptGraphAssets[key];
    #region SESSION
    [ContextMenu("Start Session")]
    public void StartSession() => StartSession(DateTime.Now.TimeOfDay.ToString());

    public void StartSession(string sessionName)
    {
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

        record.time = currentTime;
        record.character = scriptMachine.GetComponent<TFGCharacter>();
        record.action = scriptMachine.GetCurrentScriptGraphAsset();

        sessionData.actionRecords.Add(record);

        NextCharacter();
    }

    public void GetRecord() => ExportRecordAsJSON();

    public string ExportRecordAsJSON()
    {
        var result = string.Empty;

        var JSONNode = new JSONObject();

        JSONNode.Add("name", sessionData.name);
        JSONNode.Add("startTime", sessionData.startTime);

        var initialJSON = JSON.Parse(sessionData.SerializeInitialCharacterTransforms());
        var recordJSON = JSON.Parse(sessionData.SerializeActionRecords());

        JSONNode.Add("initialCharacterTransforms", initialJSON);
        JSONNode.Add("actionRecords", recordJSON);

        result = JSONNode.ToString();

        Debug.Log(result);

        return result;
    }
    #endregion
    #endregion
}

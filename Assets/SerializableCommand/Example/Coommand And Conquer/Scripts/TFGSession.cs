using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class TFGSession : MonoBehaviour
{
    static TFGSession instance;

    [SerializeField] List<ScriptGraphAsset> _scriptGraphAssets = new();
    [SerializeField] SerializableDictionary<string, TFGCharacter> characters = new();
    [SerializeField] SerializableDictionary<string, TFGObject> objects = new();

    [Header("References")]
    [SerializeField] Transform characterInteractionContainer;
    [SerializeField] bool useDatabaseObject;
    [SerializeField] DatabaseObject activeScenarioData;

    [Header("Prefabs")]
    [SerializeField] TFGCharacterInteraction characterInteractionPrefab;

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<object> OnSessionStart;
    [SerializeField] CommunicationHandler OnSessionStop;
    [SerializeField] CommunicationHandler<float> OnDurationSet;
    [SerializeField] CommunicationHandler<TFGCharacter> OnCurrentCharacterUpdated;

    [Header("Receivers")]
    [SerializeField] CommunicationHandler<object> OnCharacterStartInteracting;
    [SerializeField] CommunicationHandler<TFGCharacter> OnCharacterStopInteracting;

    Dictionary<string, ScriptGraphAsset> scriptGraphAssets = new();

    Dictionary<TFGCharacter, TFGCharacterInteraction> characterInteractionPool;

    int characterIndex = 0;

    public SerializableDictionary<string, TFGCharacter> Characters
    {
        get
        {
            return characters;
        }
    }

    #region METHODS
    private void Awake()
    {
        characterInteractionPool = new();

        instance = this;

        OnCharacterStartInteracting.Register(this).OnReceiveSignal += DisplayInteractionPanel;
        OnCharacterStopInteracting.Register(this).OnReceiveSignal += HideInteractionPanel;
    }

    public ScriptGraphAsset GetScriptGraphAsset(string key) => scriptGraphAssets[key];

    //{
    //var obj = characters[characterId];
    //return obj.GetType() == typeof(TFGCharacter) ? (TFGCharacter)obj : null;
    //}
    public static TFGCharacter GetCharacter(string characterId) => instance.characters[characterId];
    public static GameObject GetInteractableGameObject(string guid) => instance.objects[guid].gameObject;

    #region SESSION
    public void StartSession()
    {
        if (useDatabaseObject)
        {
            StartSession(activeScenarioData);
        }
        else
        {
            //string jsonData = activeScenarioData.ParseToJSON();
            //StartSession(jsonData);
        }
    }

    public void StartSession(DatabaseObject databaseObject)
    {
        characters = new();
        objects = new();

        var data = databaseObject.data;

        //Register characters
        foreach (var characterData in data["data"]["characters"].childs)
        {
            RegisterCharacter(characterData[1].stringValue, characterData[0].stringValue);
        }

        //Register objects
        foreach (var objectData in data["data"]["objects"].childs)
        {
            RegisterObjects(objectData[1].stringValue, objectData[0].stringValue);
        }

        (string sessionGUID, string scenarioGUID, int duration, Dictionary<string, TFGCharacter> characters, Dictionary<string, TFGObject> objects) sessionData = new();
        sessionData.sessionGUID = $"{data["guid"].stringValue}-{DateTime.Now.ToString()}";
        sessionData.scenarioGUID = $"{data["guid"].stringValue}";
        sessionData.duration = data["data"]["duration"].intValue;
        sessionData.characters = characters.ToDictionary();
        sessionData.objects = objects.ToDictionary();

        OnSessionStart.Broadcast(sessionData);
        OnDurationSet.Broadcast((float)sessionData.duration);
    }

    public void StartSession(string jsonData)
    {
        var json = JSON.Parse(jsonData);
        var data = json["data"].AsObject;

        characters = new();

        //Register characters
        foreach (var _character in data["characters"].Children)
        {
            characters.Add(_character[0], TFGCharacterFactory.GetCharacter(_character[1]));
        }

        //Register objects
        foreach (var _object in data["objects"].Children)
        {
            objects.Add(_object[0], TFGObjectFactory.GetObject(_object[1]));
        }

        (string sessionGUID, string scenarioGUID, int duration, Dictionary<string, TFGCharacter> characters, Dictionary<string, TFGObject> objects) sessionData = new();
        sessionData.sessionGUID = $"{json["guid"].Value}-{DateTime.Now.ToString()}";
        sessionData.scenarioGUID = $"{json["guid"].Value}";
        sessionData.duration = data["duration"].AsInt;
        sessionData.characters = characters.ToDictionary();
        sessionData.objects = objects.ToDictionary();

        OnSessionStart.Broadcast(sessionData);
    }

    public void StopSession()
    {
        Variables.Scene(SceneManager.GetActiveScene())["replayMode"] = true;

        foreach (var obj in characters.Values)
        {
            if (obj.GetType() != typeof(TFGCharacter)) continue;

            var character = (TFGCharacter)obj;
            character.ClearCommand();
        }

        OnSessionStop.Broadcast();
    }

    public void RegisterCharacter(string baseGUID, string assignedGUID)
    {
        var character = TFGCharacterFactory.GetCharacter(baseGUID);
        character.Initialize();
        character.guid = assignedGUID;
        characters.Add(assignedGUID, character);

        var characterInteraction = Instantiate(characterInteractionPrefab, characterInteractionContainer);
        characterInteraction.Initialize(character);
        characterInteraction.gameObject.SetActive(false);
        characterInteractionPool.Add(character, characterInteraction);
    }

    public void RegisterObjects(string baseGUID, string assignedGUID)
    {
        var tfgObject = TFGObjectFactory.GetObject(baseGUID);
        tfgObject.guid = assignedGUID;
        objects.Add(assignedGUID, tfgObject);
    }
    #endregion

    #region INTERACTION
    void DisplayInteractionPanel(object input)
    {
        var data = ((TFGCharacter source, TFGObjectInteractable interactedObject, List<ScriptGraphAsset> actions)) input;
        var characterInteraction = characterInteractionPool[data.source];

        characterInteraction.gameObject.SetActive(true);
        characterInteraction.Show(data.interactedObject, data.actions);
    }

    void HideInteractionPanel(TFGCharacter source) => characterInteractionPool[source].gameObject.SetActive(false);
    #endregion

    #region SCRIPT GRAPH ASSET
    //public void AddScriptGraphAsset(object obj)
    //{
    //    var scriptGraphAsset = (ScriptGraphAsset)obj;

    //    _scriptGraphAssets.Add(scriptGraphAsset);

    //    scriptGraphAssets.TryAdd(scriptGraphAsset.name, scriptGraphAsset);
    //}
    #endregion
    #endregion
}

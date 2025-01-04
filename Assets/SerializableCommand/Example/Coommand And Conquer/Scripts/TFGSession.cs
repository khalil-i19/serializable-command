using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TFGSession : MonoBehaviour
{
    [SerializeField] List<ScriptGraphAsset> _scriptGraphAssets = new();
    [SerializeField] List<TFGCharacter> _characters = new();

    [SerializeField] int sessionDurationInSeconds;

    [SerializeField] DatabaseObject sessionDataHistory;

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<object> OnSessionStart;
    [SerializeField] CommunicationHandler OnSessionStop;
    [SerializeField] CommunicationHandler<float> OnDurationSet;

    [Header("Receivers")]
    [SerializeField] CommunicationHandler<TFGCharacter> OnCurrentCharacterUpdated;

    Dictionary<string, ScriptGraphAsset> scriptGraphAssets = new();
    Dictionary<string, TFGCharacter> characters = new();


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

        characterIndex = -1;

        NextCharacter();

        (string sessionName, int duration, TFGCharacter[] characters) data = new();
        data.sessionName = sessionName;
        data.characters = _characters.ToArray();
        data.duration = sessionDurationInSeconds;

        OnSessionStart.Broadcast(data);
        OnDurationSet.Broadcast((float)sessionDurationInSeconds);
    }

    [ContextMenu("Stop Session")]
    public void StopSession()
    {
        Variables.Scene(SceneManager.GetActiveScene())["replayMode"] = true;

        foreach(var character in _characters)
        {
            character.ClearCommand();
        }

        OnSessionStop.Broadcast();
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
    #endregion
}

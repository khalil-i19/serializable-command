using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TFGSessionHistory : MonoBehaviour
{
    [SerializeField] DatabaseObject sessionHistory;

    [SerializeField] PlayableDirector playableDirector;

    [Header("Communications")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<string> OnImportData;
    [SerializeField] CommunicationHandler<TFGActionRecord> OnActionRecorded;
    [Header("Receivers")]
    [SerializeField] CommunicationHandler<object> OnSessionStart;
    [SerializeField] CommunicationHandler OnSessionStop;
    [SerializeField] CommunicationHandler<TFGActionRecord> OnRecordAction;

    [Header("Prefabs")]
    [SerializeField] ComponentLookup buttonPrefab;

    [Header("References")]
    [SerializeField] ComponentLookup componentLookup;

    [Header("Current History")]
    public TFGSessionData sessionData;

    ObjectPool<ComponentLookup> buttonPool;

    Transform ButtonContainer => componentLookup.Get<Transform>("button-container");

    private void Start()
    {
        buttonPool = new ObjectPool<ComponentLookup>(buttonPrefab);

        OnSessionStart.Register(this).OnReceiveSignal += SessionStarted;
        OnSessionStop.Register(this).OnReceiveSignal += value => SessionEnded();
        OnRecordAction.Register(this).OnReceiveSignal += Record;

        Refresh();
    }

    #region SESSION
    public void SessionStarted(object obj)
    {
        var data = ((string sessionGUID, string scenarioGUID, int duration, Dictionary<string, TFGCharacter> characters, Dictionary<string, TFGObject> objects))obj;
        sessionData = new TFGSessionData() { guid = data.sessionGUID, sessionBase = data.scenarioGUID, startTime = DateTime.Now.ToString() };

        foreach (var character in data.characters.Values)
        {
            var characterTransformData = new TFGCharacterTransformData();
            characterTransformData.character = character;
            characterTransformData.worldPosition = character.transform.position;
            characterTransformData.worldRotation = character.transform.eulerAngles;

            sessionData.initialCharacterTransforms.Add(characterTransformData);
        }
    }

    public void SessionEnded()
    {
        sessionData.endTime = DateTime.Now.ToString();

        GetRecord();
    }

    #region RECORD
    void Record(TFGActionRecord actionRecord)
    {
        actionRecord.time = GetTime();

        sessionData.actionRecords.Add(actionRecord);

        OnActionRecorded.Broadcast(actionRecord);
    }

    double GetTime() => playableDirector.time;

    public void GetRecord()
    {
        ExportRecordAsJSON();
        Refresh();
    }

    public string ExportRecordAsJSON()
    {
        var result = string.Empty;

        var JSONNode = new JSONObject();

        JSONNode.Add("guid", sessionData.guid);
        JSONNode.Add("sessionBase", sessionData.sessionBase);
        //JSONNode.Add("duration", sessionData.duration);
        JSONNode.Add("startTime", sessionData.startTime);
        JSONNode.Add("endTime", sessionData.endTime);

        var initialJSON = JSON.Parse(sessionData.SerializeInitialCharacterTransforms());
        var recordJSON = JSON.Parse(sessionData.SerializeActionRecords());

        JSONNode.Add("initialCharacterTransforms", initialJSON);
        JSONNode.Add("actionRecords", recordJSON);

        result = JSONNode.ToString();

        var historyData = sessionHistory.data.AddNewChild(sessionData.startTime);
        historyData.type = DatabaseObject.DataType.String;
        historyData.stringValue = result;

        return result;
    }
    #endregion
    #endregion

    public void Refresh()
    {
        buttonPool.Clear();

        foreach (var item in sessionHistory.data.childs)
        {
            var buttonLookup = buttonPool.Instantiate(ButtonContainer);

            var button = buttonLookup.Get<Button>("button");
            var buttonText = buttonLookup.Get<TextMeshProUGUI>("button-text");

            buttonText.text = item.id;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OnImportData.Broadcast(item.stringValue);
            });
        }
    }
}
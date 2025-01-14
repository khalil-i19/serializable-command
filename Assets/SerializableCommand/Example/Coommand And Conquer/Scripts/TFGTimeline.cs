using Innoveam;
using Innoveam.Modules.Communication;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.Timeline;

public class TFGTimeline : MonoBehaviour
{
    string playableAssetPath = "Assets/SerializableCommand/Example/Coommand And Conquer/Data/PlayableAsset";

    [Header("Attention")]
    [Header("This script should only run in runtime/ Play mode")]
    [Space(8f)]

    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] TFGSession session;
    [SerializeField] TFGSessionHistory sessionHistory;

    [Header("Communications")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler OnDataLoaded;
    [SerializeField] CommunicationHandler<float> OnTimeChangedNormalized;
    [Header("Receivers")]
    [SerializeField] CommunicationHandler OnSessionStart;
    [SerializeField] CommunicationHandler OnSessionStop;
    [SerializeField] CommunicationHandler<TFGActionRecord> OnActionRecorded;
    [SerializeField] CommunicationHandler<string> OnImportData;
    [SerializeField] CommunicationHandler OnPlay;

    public Action<PlayableDirector> OnTimelineStopped;

    float lastTime = -1;

    int debugIndex = -1;

    #region METHODS

    private void Start()
    {
        OnSessionStart.Register(this).OnReceiveSignal += value => CreateTimeline();
        OnActionRecorded.Register(this).OnReceiveSignal += Append;
        OnImportData.Register(this).OnReceiveSignal += ImportFromJSON;
        OnPlay.Register(this).OnReceiveSignal += value => Play();
    }

    #region PLAYBACK
    public void Play()
    {
        playableDirector.Play();
    }

    public void Pause()
    {
        playableDirector.Pause();
    }

    public void SetTime(float seconds)
    {
        playableDirector.Pause();
        playableDirector.time = seconds;
        playableDirector.Evaluate();

        lastTime = seconds;
    }

    public void SetTime01(float interpolant)
    {
        float time = Mathf.Lerp(0f, interpolant, (float)playableDirector.duration);
        SetTime(time);

    }

    private void Update()
    {
        if (lastTime != playableDirector.time)
        {
            lastTime = (float)playableDirector.time;

            OnTimeChangedNormalized.Broadcast(lastTime);
        }
    }
    #endregion

    #region JSON
    public void ImportFromJSON(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var sessionRecord = JSON.Parse(json);

        playableDirector.ClearAllBindings();

        CreateTimeline();

        //Events
        int i = 1;
        foreach (var sessionEvent in sessionRecord["actionRecords"].Children)
        {
            var startTime = sessionEvent["time"].AsFloat;
            var duration = sessionEvent["duration"].AsFloat;
            var character = TFGCharacterFactory.GetCharacter(sessionEvent["character"].Value);
            var scriptGraphAsset = session.GetScriptGraphAsset(sessionEvent["action"].Value);
            var data = sessionEvent["data"].AsObject;

            AddPlayableTrack(character.gameObject, scriptGraphAsset, $"{i}. {character.guid} - {sessionEvent["action"].Value}", startTime, duration, data);

            i++;
        }

        var timelineAsset = (TimelineAsset)playableDirector.playableAsset;
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;

        OnDataLoaded.Broadcast();
    }
    #endregion

    List<Vector3> DeserializeKnotsData(string JSONRaw)
    {
        List<Vector3> result = new();

        var movementsData = JSON.Parse(JSONRaw).AsArray;

        for (int i = 0; i < movementsData.Count; i++)
        {
            var movementData = movementsData[i].AsArray;

            var vector3Data = new Vector3();
            vector3Data.x = movementData[0].AsFloat;
            vector3Data.y = movementData[1].AsFloat;
            vector3Data.z = movementData[2].AsFloat;

            result.Add(vector3Data);
        }

        return result;
    }
    #endregion

    #region MODIFICATION
    public void CreateTimeline()
    {
        if (playableDirector == null) return;

        if (playableDirector.playableAsset != null)
        {
            Destroy(playableDirector.playableAsset);
        }

        TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();

        playableDirector.playableAsset = timelineAsset;
    }

    void Append(TFGActionRecord record)
    {
        var character = record.character;
        var scriptGraphAsset = record.action;
        var duration = record.duration;
        var data = record.data;

        //var characterId = character.guid;
        var characterName = TFGSession.GetCharacter(character.guid).name;

        var sessionEventRelativeTime = playableDirector.time;

        AddPlayableTrack(character.gameObject, scriptGraphAsset, $"{characterName} {scriptGraphAsset.name}", sessionEventRelativeTime, duration, data);

        playableDirector.RebuildGraph();
    }

    void AddAnimationTrack(TimelineAsset timelineAsset, GameObject targetObject, string name, double startTime, Action action)
    {
        // Create an Animation Track
        AnimationTrack animationTrack = timelineAsset.CreateTrack<AnimationTrack>(null, "Action Track");

        // Bind the track to the target GameObject
        playableDirector.SetGenericBinding(animationTrack, targetObject);

        // Create an empty Timeline Clip (placeholder for time)
        TimelineClip timelineClip = animationTrack.CreateDefaultClip();

        // Set the clip's duration (customize as needed)
        timelineClip.start = startTime;
        timelineClip.duration = 1.0f;
        timelineClip.displayName = name;

        // Create a custom PlayableAsset to execute the Action
        ActionPlayableAsset customAction = ScriptableObject.CreateInstance<ActionPlayableAsset>();
        customAction.action = action;

        // Assign the custom PlayableAsset to the clip
        timelineClip.asset = customAction;
    }

    void AddPlayableTrack(GameObject binding, ScriptGraphAsset scriptGraphAsset, string name, double startTime, double duration, JSONObject data)
    {
        var timelineAsset = (TimelineAsset)playableDirector.playableAsset;

        VisualScriptTrackAsset animationTrack = timelineAsset.FirstOrNew<VisualScriptTrackAsset>(binding.name);

        TimelineClip timelineClip = animationTrack.CreateDefaultClip();

        VisualScriptPlayableAsset visualScriptPlayableAsset = ScriptableObject.CreateInstance<VisualScriptPlayableAsset>();

        List<BezierKnot> generatedKnotsData = new List<BezierKnot>();

        playableDirector.SetGenericBinding(animationTrack, binding.GetComponent<ScriptMachine>());

        timelineClip.displayName = name;
        timelineClip.start = startTime;
        timelineClip.duration = duration;
        timelineClip.asset = visualScriptPlayableAsset;

        visualScriptPlayableAsset.name = $"{++debugIndex}. {name}";
        visualScriptPlayableAsset.bound = binding;
        visualScriptPlayableAsset.data = data;
        visualScriptPlayableAsset.scriptGraphAsset = scriptGraphAsset;
        visualScriptPlayableAsset.clip = timelineClip;
    }
    #endregion

#if UNITY_EDITOR
    [ContextMenu("Export PlayableAsset")]
    private void SavePlayableAsset()
    {
        if (playableDirector.playableAsset == null) return;

        // Save the asset to the specified path
        AssetDatabase.CreateAsset(playableDirector.playableAsset, $"{playableAssetPath}/{sessionHistory.sessionData.sessionBase}.playable");
        AssetDatabase.SaveAssets();
    }
#endif
}

using Innoveam;
using Innoveam.Modules.Communication;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.Timeline;

public class TFGTimeline : MonoBehaviour
{
    [Header("Attention")]
    [Header("This script should only run in runtime/ Play mode")]
    [Space(8f)]

    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] TFGSession session;

    [SerializeField, TextArea] string jsonTest;

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

    double lastTime = -1;

    #region METHODS

    private void Start()
    {
        OnSessionStart.Register(this).OnReceiveSignal += value => CreateTimeline();
        OnActionRecorded.Register(this).OnReceiveSignal += Append;
        OnImportData.Register(this).OnReceiveSignal += ImportFromJSON;
        OnPlay.Register(this).OnReceiveSignal += value => Play();
    }

    #region PLAYBACK

    [ContextMenu("Play")]
    public void Play()
    {
        playableDirector.Play();
    }

    [ContextMenu("Pause")]

    public void Pause()
    {
        playableDirector.Pause();
    }

    public void SetTime(float seconds)
    {
        playableDirector.Pause();
        playableDirector.time = seconds;
        playableDirector.Evaluate();
    }

    public void SetTime01(float interpolant)
    {
        float time = Mathf.Lerp(0f, interpolant, (float)playableDirector.duration);
        SetTime(time);

    }

    private void Update()
    {
        if(lastTime != playableDirector.time)
        {
            lastTime = playableDirector.time;

            var durationInterpolant = Mathf.InverseLerp(0, (float)playableDirector.duration, (float)lastTime);

            OnTimeChangedNormalized.Broadcast(durationInterpolant);
        }
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

    void Append(TFGActionRecord data)
    {
        //var sessionEventTime = DateTime.Parse(data.time);
        var character = data.character;
        var scriptGraphAsset = data.action;
        var duration = data.duration;// sessionEvent["traversalTime"].AsFloat;
        var knotsData = data.movementData;

        var characterId = character.id;

        //var sessionEventRelativeTime = (sessionEventTime - startTime).TotalSeconds;
        var sessionEventRelativeTime = playableDirector.time;

        AddSplineTrack((TimelineAsset)playableDirector.playableAsset, character.gameObject, scriptGraphAsset, $"{characterId} {scriptGraphAsset.name}", sessionEventRelativeTime, duration, knotsData);
        
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

    void AddSplineTrack(TimelineAsset timelineAsset, GameObject binding, ScriptGraphAsset scriptGraphAsset, string name, double startTime, float duration, List<Vector3> knotsData)
    {
        VisualScriptTrackAsset animationTrack = timelineAsset.FirstOrNew<VisualScriptTrackAsset>(binding.name);

        TimelineClip timelineClip = animationTrack.CreateDefaultClip();

        VisualScriptPlayableAsset visualScriptPlayableAsset = ScriptableObject.CreateInstance<VisualScriptPlayableAsset>();

        List<BezierKnot> generatedKnotsData = new List<BezierKnot>();

        playableDirector.SetGenericBinding(animationTrack, binding.GetComponent<ScriptMachine>());

        float distance = knotsData.GetLength();
        float walkSpeed = (float)Variables.Scene(SceneManager.GetActiveScene())["walkSpeed"];

        timelineClip.displayName = name;
        timelineClip.start = startTime;
        timelineClip.duration = duration;
        timelineClip.asset = visualScriptPlayableAsset;

        visualScriptPlayableAsset.bound = binding;
        visualScriptPlayableAsset.knotsData = knotsData;
        visualScriptPlayableAsset.scriptGraphAsset = scriptGraphAsset;
        visualScriptPlayableAsset.splineLength = distance;
    }
    #endregion

    #region JSON
    public void ImportFromJSON(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var sessionRecord = JSON.Parse(json);

        if (!DateTime.TryParse(sessionRecord["startTime"].Value, out DateTime startTime)) return;
        if (!DateTime.TryParse(sessionRecord["endTime"].Value, out DateTime endTime)) return;

        playableDirector.ClearAllBindings();

        CreateTimeline();

        //Initialization

        foreach (var initialTransformData in sessionRecord["initialCharacterTransforms"].Keys)
        {
            var character = session.GetCharacter(initialTransformData);
            var initialCharacterTransformData = sessionRecord["initialCharacterTransforms"][initialTransformData];

            Vector3 worldPos = new Vector3();
            Vector3 worldRot = new Vector3();

            var worldPositionData = initialCharacterTransformData["worldPosition"].Value;
            worldPositionData = worldPositionData.GetBetween("(", ")");

            var worldRotationData = initialCharacterTransformData["worldRotation"].Value;
            worldRotationData = worldRotationData.GetBetween("(", ")");

            var worldPositionValues = worldPositionData.Split(',');
            worldPos.x = float.Parse(worldPositionValues[0]);
            worldPos.y = float.Parse(worldPositionValues[1]);
            worldPos.z = float.Parse(worldPositionValues[2]);

            var worldRotationValues = worldRotationData.Split(',');
            worldRot.x = float.Parse(worldRotationValues[0]);
            worldRot.y = float.Parse(worldRotationValues[1]);
            worldRot.z = float.Parse(worldRotationValues[2]);

            character.transform.position = worldPos;
            character.transform.rotation = Quaternion.Euler(worldRot);
        }

        //Events
        int i = 1;
        foreach (var sessionEvent in sessionRecord["actionRecords"].Children)
        {
            var characterId = sessionEvent["character"].Value;
            var character = session.GetCharacter(characterId);

            var scriptGraphAsset = session.GetScriptGraphAsset(sessionEvent["action"].Value);

            var sessionEventTime = DateTime.Parse(sessionEvent["time"].Value);
            var sessionEventRelativeTime = (sessionEventTime - startTime).TotalSeconds;

            List<Vector3> knotsData = DeserializeKnotsData(sessionEvent["movementData"].Value);

            var duration = sessionEvent["traversalTime"].AsFloat;

            AddSplineTrack((TimelineAsset)playableDirector.playableAsset, character.gameObject, scriptGraphAsset, $"{i}. {characterId} {sessionEvent["action"].Value}", sessionEventRelativeTime, duration, knotsData);

            i++;
        }

        var playbackDuration = (endTime - startTime).TotalSeconds;
        var timelineAsset = (TimelineAsset)playableDirector.playableAsset;
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
        timelineAsset.fixedDuration = playbackDuration;

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
}

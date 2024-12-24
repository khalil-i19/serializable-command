using Innoveam;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TFGTimeline : MonoBehaviour
{
    [Header("Attention")]
    [Header("This script should only run in runtime/ Play mode")]
    [Space(8f)]

    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] TFGSession session;

    [SerializeField, TextArea] string jsonTest;

    [ContextMenu("Import JSON")]
    void ImportFromJSON() => ImportFromJSON(jsonTest);

    public void ImportFromJSON(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var sessionRecord = JSON.Parse(json);

        if (!DateTime.TryParse(sessionRecord["startTime"].Value, out DateTime startTime)) return;

        CreateTimeline();

        //Initialization
        Action initialAction = () =>
        {
            foreach(var initialTransformData in sessionRecord["initialCharacterTransforms"].Keys)
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
        };

        AddAnimationTrack((TimelineAsset)playableDirector.playableAsset, null, "Initialization", 0f, initialAction);

        //Events
        int i = 1;
        foreach (var sessionEvent in sessionRecord["actionRecords"].Children)
        {
            var characterId = sessionEvent["character"].Value;
            var character = session.GetCharacter(characterId);

            var scriptGraphAsset = session.GetScriptGraphAsset(sessionEvent["action"].Value);

            var sessionEventTime = DateTime.Parse(sessionEvent["time"].Value);
            var sessionEventRelativeTime = (sessionEventTime - startTime).TotalSeconds;

            Debug.Log($"[TFGTimeline] {sessionEventRelativeTime}");

            Action action = () => character.RunCommand(scriptGraphAsset);

            AddAnimationTrack((TimelineAsset)playableDirector.playableAsset, character.gameObject, $"{i}. {characterId} {sessionEvent["action"].Value}", sessionEventRelativeTime, action);

            i++;
        }
    }

    [ContextMenu("Play")]
    public void Play()
    {
        playableDirector.Play();
    }

    void CreateTimeline()
    {
        if (playableDirector == null) return;

        if (playableDirector.playableAsset != null)
        {
            Destroy(playableDirector.playableAsset);
        }

        TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();

        playableDirector.playableAsset = timelineAsset;
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
}

using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class VisualScriptPlayableAsset : PlayableAsset
{
    public string name;
    public GameObject bound;
    public ScriptGraphAsset scriptGraphAsset;
    public JSONObject data;
    public TimelineClip clip;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Create a ScriptPlayable with the CustomActionPlayableBehaviour
        var playable = ScriptPlayable<VisualScriptPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.name = name;
        behaviour.boundScriptMachine = bound.GetComponent<ScriptMachine>();
        behaviour.scriptGraphAsset = scriptGraphAsset;
        behaviour.data = data;
        behaviour.clip = clip;

        return playable;
    }
}

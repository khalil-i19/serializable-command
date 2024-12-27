using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class VisualScriptPlayableAsset : PlayableAsset
{
    public float splineLength;
    public GameObject bound;
    public ScriptGraphAsset scriptGraphAsset;
    public List<Vector3> knotsData = new();

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Create a ScriptPlayable with the CustomActionPlayableBehaviour
        var playable = ScriptPlayable<VisualScriptPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.boundScriptMachine = bound.GetComponent<ScriptMachine>();
        behaviour.knotsData = knotsData;
        behaviour.scriptGraphAsset = scriptGraphAsset;
        behaviour.splineLength = splineLength;

        return playable;
    }
}

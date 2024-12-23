using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class VisualScriptPlayableAsset : PlayableAsset
{
    public Action action; // The action to invoke

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Create a ScriptPlayable with the CustomActionPlayableBehaviour
        var playable = ScriptPlayable<ActionPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // Pass the action to the behaviour
        //behaviour.action = action;

        return playable;
    }
}

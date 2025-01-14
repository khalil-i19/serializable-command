using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[TrackBindingType(typeof(ScriptMachine))]
[TrackClipType(typeof(VisualScriptPlayableAsset))]
public class VisualScriptTrackAsset : TrackAsset
{
    Playable visualScriptMixer;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        visualScriptMixer = ScriptPlayable<VisualScriptMixer>.Create(graph, inputCount);
        return visualScriptMixer;
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
    }
}

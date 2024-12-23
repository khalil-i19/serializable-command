using UnityEngine.Playables;
using System;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Timeline;

//TODO: Nanti invoke VisualScriptEvent di sini
public class VisualScriptPlayableBehaviour : PlayableBehaviour
{
    public Action action;
    private bool actionExecuted = false;

    ScriptMachine boundScriptMachine;

    public override void OnGraphStart(Playable playable)
    {
        base.OnGraphStart(playable);

        // Get the PlayableDirector using the Playable's graph resolver
        PlayableDirector director = playable.GetGraph().GetResolver() as PlayableDirector;

        if (director != null)
        {
            // Get the track from the playable asset
            var track = director.playableAsset as TimelineAsset;

            if (track != null)
            {
                // Get the track's bindings
                foreach (var trackItem in track.GetOutputTracks())
                {
                    // Now use GetGenericBinding on the track to get the target GameObject
                    var targetGameObject = director.GetGenericBinding(trackItem) as GameObject;

                    if (targetGameObject != null)
                    {
                        // Access any component from the target GameObject
                        boundScriptMachine = targetGameObject.GetComponent<ScriptMachine>();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("PlayableDirector is null.");
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // Execute the action only once during the frame
        if (!actionExecuted && action != null)
        {
            //action.Invoke();
            actionExecuted = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // Reset the execution flag if the playable is replayed
        actionExecuted = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);


    }
}

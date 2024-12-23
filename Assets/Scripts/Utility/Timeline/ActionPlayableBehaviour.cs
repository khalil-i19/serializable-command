using UnityEngine.Playables;
using System;

public class ActionPlayableBehaviour : PlayableBehaviour
{
    public Action action;
    private bool actionExecuted = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // Execute the action only once during the frame
        if (!actionExecuted && action != null)
        {
            action.Invoke();
            actionExecuted = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // Reset the execution flag if the playable is replayed
        actionExecuted = false;
    }
}
using UnityEngine.Playables;
using System;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine.Splines;

public class VisualScriptPlayableBehaviour : PlayableBehaviour
{
    public float splineLength;
    public ScriptGraphAsset scriptGraphAsset;
    public ScriptMachine boundScriptMachine;
    public List<Vector3> knotsData = new List<Vector3>();

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);

        var currentTime = playable.GetTime();
        var duration = playable.GetDuration();

        var unitTime = Mathf.Min((float)(currentTime / duration), 1f);

        Variables.Object(boundScriptMachine)["distanceTraveled"] = unitTime * splineLength;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);

        Variables.Object(boundScriptMachine)["splineData"] = knotsData;
        Variables.Object(boundScriptMachine)["distanceTraveled"] = 0f;

        var scriptMachineUtility = boundScriptMachine.GetComponent<ScriptMachineUtility>();
        scriptMachineUtility.SetScriptGraphAsset(scriptGraphAsset);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);

        var currentTime = playable.GetTime() + info.deltaTime;
        var duration = playable.GetDuration();

        if ((info.effectivePlayState == PlayState.Paused && currentTime >= duration) || playable.GetGraph().GetRootPlayable(0).IsDone())
        {
            Variables.Object(boundScriptMachine)["distanceTraveled"] = splineLength;

            boundScriptMachine.GetComponent<TFGCharacter>().ClearCommand();
        }
    }
}

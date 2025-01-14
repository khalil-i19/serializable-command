using UnityEngine;
using UnityEngine.Playables;

//Evaluating ALL AnimationClip attached to this playable's TrackAsset
public class VisualScriptMixer : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int clipCount = playable.GetInputCount();

        string message = string.Empty;

        message += $"{playable.GetType()}\nTime: {playable.GetTime()}\nPreviousTime: {playable.GetPreviousTime()}";
        float time = (float)playable.GetTime();

        //Evaluate other TimelineClips before evaluating active TimelineClip
        //Preceeding evaluation
        for (int i = clipCount - 1; i >= 0; i--)
        {
            PreceedingEvaluation(playable, i);
        }

        //Proceeding evaluation
        for (int i = 0; i < clipCount; i++)
        {
            ProceedingEvaluation(playable, i);
        }

        //Find active TimelineClip by checking each weight; if it's more than 0, then it's an active TimelineClip
        for (int i = 0; i < clipCount; i++)
        {
            var currentWeight = playable.GetInputWeight(i);
            if (currentWeight > 0)
            {
                var childPlayable = (ScriptPlayable<VisualScriptPlayableBehaviour>)playable.GetInput(i);
                var childPlayableBehaviour = childPlayable.GetBehaviour();
                childPlayableBehaviour.Evaluate((float)childPlayable.GetTime());
                break;
            }
        }
    }

    void ProceedingEvaluation(Playable root, int index)
    {
        var visualScriptPlayable = (ScriptPlayable<VisualScriptPlayableBehaviour>)root.GetInput(index);
        var visualScriptPlayableBehaviour = visualScriptPlayable.GetBehaviour();

        var time = root.GetTime();

        var timelineClip = visualScriptPlayableBehaviour.clip;

        if (time > timelineClip.end) visualScriptPlayableBehaviour.Proceeds();

    }

    void PreceedingEvaluation(Playable root, int index)
    {
        var visualScriptPlayable = (ScriptPlayable<VisualScriptPlayableBehaviour>)root.GetInput(index);
        var visualScriptPlayableBehaviour = visualScriptPlayable.GetBehaviour();

        var time = root.GetTime();

        var timelineClip = visualScriptPlayableBehaviour.clip;

        if (time < timelineClip.start) visualScriptPlayableBehaviour.Preceedes();

    }

    bool EvaluateByWeight(Playable root, int index)
    {
        return root.GetInputWeight(index) > 0;
    }
}

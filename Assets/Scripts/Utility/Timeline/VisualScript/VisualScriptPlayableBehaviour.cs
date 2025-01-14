using SimpleJSON;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class VisualScriptPlayableBehaviour : PlayableBehaviour
{
    public string name;
    public ScriptMachine boundScriptMachine;
    public ScriptGraphAsset scriptGraphAsset;
    public JSONObject data;
    public TimelineClip clip;

    string interpolantKey = "time";
    string dataKey = "data";

    Playable playable;

    bool initialized = false;
    bool preceedeEvaluated = false;
    bool proceedEvaluated = false;

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        this.playable = playable;
    }

    public void Initialize()
    {
        //TODO: Masih sementara
        dynamic value;
        if (data["type"].Value.Contains("JSON"))
        {
            value = JSON.Parse(data["data"].ToString());
        }
        else
        {
            Type type = Type.GetType($"{data["type"].Value}, Assembly-CSharp");

            value = JsonUtility.FromJson(data["data"].ToString(), type);
        }

        Variables.Object(boundScriptMachine)[dataKey] = value;

        var scriptMachineUtility = boundScriptMachine.GetComponent<ScriptMachineUtility>();
        scriptMachineUtility.SetScriptGraphAsset(scriptGraphAsset);

        initialized = true;
    }

    public void Evaluate(float time)
    {
        Initialize();

        preceedeEvaluated = false;
        proceedEvaluated = false;

        SetTime(time);
        //Debug.Log($"{time} {name} => Evaluated");
    }

    public void Preceedes()
    {
        if (preceedeEvaluated) return;

        Initialize();

        proceedEvaluated = false;
        preceedeEvaluated = true;

        SetTime(-1f);
        //Debug.Log($"{-1} {name} => Preceedes");
    }

    public void Proceeds()
    {
        if (proceedEvaluated) return;

        Initialize();

        proceedEvaluated = true;
        preceedeEvaluated = false;

        var duration = playable.GetDuration();

        SetTime((float)duration);
        //Debug.Log($"{duration} {name} => Proceeds");
    }

    void SetTime(float time)
    {
        Variables.Object(boundScriptMachine)[interpolantKey] = time;
        CustomEvent.Trigger(boundScriptMachine.gameObject, "TimelineUpdate", time);
    }
}

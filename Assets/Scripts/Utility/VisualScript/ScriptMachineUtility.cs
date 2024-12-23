using Innoveam.Modules.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScriptMachineUtility : MonoBehaviour
{
    public ScriptMachine scriptMachine;

    GraphReference graphReference;

    //public Action<ScriptGraphAsset> OnStartExecutingScriptGraphAsset;
    [SerializeField] CommunicationHandler<ScriptMachine> OnStartExecutingScriptMachine;

    public void SetScriptGraphAsset(ScriptGraphAsset asset)
    {
        scriptMachine.nest.SwitchToMacro(asset);
    }

    public void SetAndStartScriptGraphAsset(ScriptGraphAsset asset)
    {
        StopScriptGraphAsset();
        SetScriptGraphAsset(asset);
        StartScriptGraphAsset();
    }

    public void StartScriptGraphAsset()
    {
        var graphReference = GraphReference.New(scriptMachine, true);
        scriptMachine.nest.graph.StartListening(graphReference);

        //OnStartExecutingScriptGraphAsset?.Invoke(scriptMachine.GetCurrentScriptGraphAsset());
        OnStartExecutingScriptMachine.Broadcast(scriptMachine);
    }

    public void StopScriptGraphAsset()
    {
        if (scriptMachine == null) return;
        if (graphReference == null) return;

        scriptMachine.nest.graph.StopListening(graphReference);
    }
}

public static class ScriptMachineExtension
{
    public static ScriptGraphAsset GetCurrentScriptGraphAsset(this ScriptMachine scriptMachine) => scriptMachine.nest.macro;
}
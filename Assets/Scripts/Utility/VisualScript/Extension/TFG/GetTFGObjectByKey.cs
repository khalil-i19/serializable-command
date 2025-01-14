using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

[UnitCategory("TFG Utility")]
[UnitTitle("Get GameObject By GUID")]
public class GetGameObjectByGUID : Unit
{
    // Input and Output Ports
    [DoNotSerialize]
    public ControlInput inputTrigger { get; private set; }

    [DoNotSerialize]
    public ControlOutput outputTrigger { get; private set; }

    [DoNotSerialize]
    public ValueInput keyInput { get; private set; }

    [DoNotSerialize]
    public ValueOutput valueOutput { get; private set; }

    protected override void Definition()
    {
        // Define Ports
        inputTrigger = ControlInput("inputTrigger", Execute);
        outputTrigger = ControlOutput("outputTrigger");

        keyInput = ValueInput<string>("Key");

        valueOutput = ValueOutput<GameObject>("Value", RetrieveValue);

        // Link flow between ports
        Succession(inputTrigger, outputTrigger);
    }

    private ControlOutput Execute(Flow flow)
    {
        var value = RetrieveValue(flow);

        return outputTrigger;
    }

    private GameObject RetrieveValue(Flow flow)
    {
        var key = flow.GetValue<string>(keyInput);

        var value = TFGSession.GetInteractableGameObject(key);

        return value;
    }
}
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

[UnitCategory("JSON")]
[UnitTitle("Get Value By Key")]
public class GetValueByKey : Unit
{
    // Input and Output Ports
    [DoNotSerialize]
    public ControlInput inputTrigger { get; private set; }

    [DoNotSerialize]
    public ControlOutput outputTrigger { get; private set; }

    [DoNotSerialize]
    public ValueInput jsonInput { get; private set; }

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
        jsonInput = ValueInput<JSONObject>("JSON Raw");

        valueOutput = ValueOutput<object>("Value", RetrieveValue);

        // Link flow between ports
        Succession(inputTrigger, outputTrigger);
    }

    private ControlOutput Execute(Flow flow)
    {
        var value = RetrieveValue(flow);

        return outputTrigger;
    }

    private object RetrieveValue(Flow flow)
    {
        var key = flow.GetValue<string>(keyInput);
        var jsonRaw = flow.GetValue<JSONObject>(jsonInput);

        var value = jsonRaw[key].Value;

        return value;
    }
}
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[UnitCategory("Splines")]
[UnitTitle("GetSplineTraversalTime")]
public class GetSplineTraversalTime : Unit
{
    // Input and Output Ports
    [DoNotSerialize]
    public ControlInput inputTrigger { get; private set; }

    [DoNotSerialize]
    public ControlOutput outputTrigger { get; private set; }

    [DoNotSerialize]
    public ValueInput knotInput { get; private set; }

    [DoNotSerialize]
    public ValueInput speedInput { get; private set; }

    [DoNotSerialize]
    public ValueOutput result { get; private set; }

    protected override void Definition()
    {
        // Define Ports
        inputTrigger = ControlInput("inputTrigger", Execute);
        outputTrigger = ControlOutput("outputTrigger");

        knotInput = ValueInput<List<Vector3>>("knots");
        speedInput = ValueInput<float>("speed");

        result = ValueOutput<float>("result", CalculateTraversalTime);

        // Link flow between ports
        Succession(inputTrigger, outputTrigger);
    }

    private ControlOutput Execute(Flow flow)
    {
        // Retrieve the value of controlPoints during execution
        var controlPointsValue = CalculateTraversalTime(flow);

        return outputTrigger;
    }

    private float CalculateTraversalTime(Flow flow)
    {
        var knots = flow.GetValue<List<Vector3>>(knotInput);
        var speed = flow.GetValue<float>(speedInput);

        List<BezierKnot> bezierKnots = new List<BezierKnot>();
        foreach (var knot in knots)
        {
            var bezierKnot = new BezierKnot(new float3(knot.x, knot.y, knot.z));
            bezierKnots.Add(bezierKnot);
        }

        Spline spline = new Spline(bezierKnots);

        var distance = spline.GetCurveLength(0);

        float result = distance / speed;

        return result;
    }
}
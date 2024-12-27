using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using ZXing;

[UnitCategory("Splines")]
[UnitTitle("Evaluate By Distance")]
public class EvaluateByDistance : Unit
{
    // Input and Output Ports
    [DoNotSerialize]
    public ControlInput inputTrigger { get; private set; }

    [DoNotSerialize]
    public ControlOutput outputTrigger { get; private set; }

    [DoNotSerialize]
    public ValueInput knotInput { get; private set; }

    [DoNotSerialize]
    public ValueInput distanceInput { get; private set; }

    [DoNotSerialize]
    public ValueOutput result { get; private set; }

    protected override void Definition()
    {
        // Define Ports
        inputTrigger = ControlInput("inputTrigger", Execute);
        outputTrigger = ControlOutput("outputTrigger");

        knotInput = ValueInput<List<Vector3>>("knots");
        distanceInput = ValueInput<float>("distance");

        result = ValueOutput<Vector3>("result", Evaluate);

        // Link flow between ports
        Succession(inputTrigger, outputTrigger);
    }

    private ControlOutput Execute(Flow flow)
    {
        // Retrieve the value of controlPoints during execution
        var controlPointsValue = Evaluate(flow);

        return outputTrigger;
    }

    private Vector3 Evaluate(Flow flow)
    {
        var knots = flow.GetValue<List<Vector3>>(knotInput);
        var distance = flow.GetValue<float>(distanceInput);

        Vector3 result = Vector3.zero;

        if (distance > knots.GetLength())
        {
            result = knots[^1];
        }
        else
        {
            List<BezierKnot> bezierKnots = new List<BezierKnot>();
            foreach (var knot in knots)
            {
                var bezierKnot = new BezierKnot(new float3(knot.x, knot.y, knot.z));
                bezierKnots.Add(bezierKnot);
            }

            Spline spline = new Spline(bezierKnots);

            var t = Mathf.Clamp01(distance / spline.GetLength());
            spline.Evaluate(t, out float3 position, out float3 tangent, out float3 upVector);
            //var position = spline.GetPointAtLinearDistance(0.001f, distance, out float resultT);

            result = new Vector3(position.x, position.y, position.z);
        }

        return result;
    }
}
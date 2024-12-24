using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[UnitCategory("Splines")]
[UnitTitle("Evaluate")]
public class EvaluateSpline : Unit
{
    // Input and Output Ports
    [DoNotSerialize]
    public ControlInput inputTrigger { get; private set; }

    [DoNotSerialize]
    public ControlOutput outputTrigger { get; private set; }

    //[DoNotSerialize]
    //public ValueInput splineInput { get; private set; }

    [DoNotSerialize]
    public ValueInput knotInput { get; private set; }

    [DoNotSerialize]
    public ValueInput timeInput { get; private set; }

    [DoNotSerialize]
    public ValueOutput result { get; private set; }

    protected override void Definition()
    {
        // Define Ports
        inputTrigger = ControlInput("inputTrigger", Execute);
        outputTrigger = ControlOutput("outputTrigger");

        knotInput = ValueInput<List<Vector3>>("knots");
        timeInput = ValueInput<float>("time");

        result = ValueOutput<Vector3>("result", Evaluate);

        // Link flow between ports
        Succession(inputTrigger, outputTrigger);
    }

    private ControlOutput Execute(Flow flow)
    {
        // Retrieve the value of controlPoints during execution
        var controlPointsValue = Evaluate(flow);

        // (Optional) Do something with controlPointsValue here if needed

        // Continue to the next node in the flow
        return outputTrigger;
    }

    private Vector3 Evaluate(Flow flow)
    {
        //var spline = flow.GetValue<Spline>(splineInput);

        //if (spline == null)
        //{
        //    Debug.LogWarning("Spline is null.");
        //    return null;
        //}

        var knots = flow.GetValue<List<Vector3>>(knotInput);
        var time = flow.GetValue<float>(timeInput);

        List<BezierKnot> bezierKnots = new List<BezierKnot>();
        foreach(var knot in knots)
        {
            var bezierKnot = new BezierKnot(new float3(knot.x, knot.y, knot.z));
            bezierKnots.Add(bezierKnot);
        }

        Spline spline = new Spline(bezierKnots);

        spline.Evaluate(time, out float3 position, out float3 tangent, out float3 upVector);

        Vector3 result = new Vector3(position.x, position.y, position.z);

        //Debug.Log($"[SplineVirtualScript] {result}");

        // Extract control points
        return result;// spline.Knots.Select(x => x.Position);
    }
}
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

[UnitCategory("Splines")]
[UnitTitle("Evaluate By Time")]
public class EvaluateByTime : Unit
{
    // Input and Output Ports
    [DoNotSerialize]
    public ControlInput inputTrigger { get; private set; }

    [DoNotSerialize]
    public ControlOutput outputTrigger { get; private set; }

    [DoNotSerialize]
    public ValueInput knotInput { get; private set; }

    [DoNotSerialize]
    public ValueInput timeInput { get; private set; }

    [DoNotSerialize]
    public ValueOutput positionResult { get; private set; }

    [DoNotSerialize]
    public ValueOutput forward { get; private set; }

    VariableDeclarations graphVariables;

    float interpolant = .25f;

    Spline _spline
    {
        get
        {
            if (graphVariables.IsDefined("spline")) return (Spline)graphVariables.Get("spline");
            else return null;
        }
    }

    protected override void Definition()
    {
        // Define Ports
        inputTrigger = ControlInput("inputTrigger", Execute);
        outputTrigger = ControlOutput("outputTrigger");

        knotInput = ValueInput<List<Vector3>>("data");
        timeInput = ValueInput<float>("time");

        positionResult = ValueOutput<Vector3>("positionResult", EvaluatePosition);
        forward = ValueOutput<Quaternion>("forward", EvaluateForward);

        // Link flow between ports
        Succession(inputTrigger, outputTrigger);
    }

    private ControlOutput Execute(Flow flow)
    {
        var knots = flow.GetValue<ListWrapper<Vector3>>(knotInput);

        InitializeGraphReference(flow);

        Populate(flow, knots.list);
        Evaluate(flow);

        // Retrieve the value of controlPoints during execution
        var positionValue = EvaluatePosition(flow);
        var forwardValue = EvaluateForward(flow);

        return outputTrigger;
    }

    private void InitializeGraphReference(Flow flow)
    {
        var graphReference = GraphReference.New(flow.stack.root, true);
        graphVariables = Variables.Graph(graphReference);
    }

    private Vector3 EvaluatePosition(Flow flow)
    {
        //var graphData = Variables.GraphInstance(flow.stack);
        Vector3 result = (Vector3)graphVariables.Get("position");

        return result;
    }

    private Quaternion EvaluateForward(Flow flow)
    {
        //var graphData = Variables.GraphInstance(flow.stack);
        if ((Vector3)graphVariables.Get("tangent") != Vector3.zero) return Quaternion.LookRotation((Vector3)graphVariables.Get("tangent"), Vector3.up);
        else return Quaternion.identity;

        //Quaternion result = Quaternion.LookRotation((Vector3)graphVariables.Get("tangent"), Vector3.up);

        //return result;
    }

    void Populate(Flow flow, List<Vector3> data)
    {
        List<BezierKnot> bezierKnots = new List<BezierKnot>();

        if (!graphVariables.IsDefined("spline"))
        {
            foreach (var knot in data)
            {
                var bezierKnot = new BezierKnot(new float3(knot.x, knot.y, knot.z));
                bezierKnots.Add(bezierKnot);
            }

            var spline = new Spline(bezierKnots);

            var speed = (float)Variables.Scene(SceneManager.GetActiveScene())["walkSpeed"];
            var duration = spline.GetLength() / speed;

            graphVariables.Set("spline", spline);
            graphVariables.Set("splineVectors", data);
            graphVariables.Set("splineDuration", duration);
        }
    }

    void Evaluate(Flow flow)
    {
        var time = flow.GetValue<float>(timeInput);
        var duration = (float)Variables.GraphInstance(flow.stack).Get("splineDuration");

        time = time / duration;

        _spline.Evaluate(time, out float3 position, out float3 tangent, out float3 upVector);

        graphVariables.Set("position", new Vector3(position.x, position.y, position.z));

        if (!graphVariables.IsDefined("tangent"))
        {
            graphVariables.Set("tangent", new Vector3(tangent.x, 0, tangent.z));
        }
        else
        {
            var currentTangent = (Vector3)graphVariables.Get("tangent");
            var newTangent = new Vector3(tangent.x, 0, tangent.z);
            newTangent = Vector3.Lerp(currentTangent, newTangent, interpolant);
            graphVariables.Set("tangent", newTangent);
        }

    }
}
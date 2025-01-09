using Innoveam.Modules.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class FreestyleVisualizer : MonoBehaviour
{
    SplineContainer splineContainer;

    [Header("Renderer")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int resolution;

    [Header("Communication")]
    [Header("Receivers")]
    [SerializeField] CommunicationHandler OnFreestyleStarted;
    [SerializeField] CommunicationHandler<List<Vector3>> OnFreestyleStopped;
    [SerializeField] CommunicationHandler<Vector3> OnAddPoint;

    private void Start()
    {
        splineContainer = GetComponent<SplineContainer>();

        OnAddPoint.Register(this).OnReceiveSignal += AddPoint;

        OnFreestyleStarted.Register(this).OnReceiveSignal += value =>
        {
            splineContainer.Spline.Clear();
            lineRenderer.positionCount = 0;
            Render();
        };
    }

    public void AddPoint(Vector3 point)
    {
        // Get the Spline object
        Spline spline = splineContainer.Spline;

        BezierKnot knot = new BezierKnot(point); // Create a knot at the given point
        spline.Add(knot); // Add the knot to the spline

        Render();
    }

    void Render()
    {
        // Get the SplineContainer and Spline
        SplineContainer container = GetComponent<SplineContainer>();
        if (container == null || container.Spline == null)
        {
            Debug.LogError("SplineContainer or Spline is missing!");
            return;
        }

        Spline spline = container.Spline;

        lineRenderer.positionCount = spline.Count;

        Vector3[] positions = new Vector3[spline.Count];
        int i = 0;

        foreach(var knot in spline.Knots)
        {
            Vector3 position = Vector3.zero;

            var knotPosition = spline.Knots.ToArray()[i].Position;

            position.x = knotPosition.x;
            position.y = knotPosition.y;
            position.z = knotPosition.z;

            positions[i] = position;

            i++;
        }

        lineRenderer.SetPositions(positions);
    }
}
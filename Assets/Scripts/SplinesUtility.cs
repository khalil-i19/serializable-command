using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class SplinesUtility
{
    public static float GetLength(this List<Vector3> knots)
    {
        float length = 0f;

        List<BezierKnot> generatedKnotsData = new List<BezierKnot>();

        foreach (var knotData in knots)
        {
            var float3Data = new float3 { x = knotData.x, y = knotData.y, z = knotData.z };
            BezierKnot knot = new BezierKnot(float3Data);

            generatedKnotsData.Add(knot);
        }

        Spline spline = new Spline(generatedKnotsData);

        length = spline.GetLength();

        return length;
    }
}

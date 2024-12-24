using Innoveam.Modules.Communication;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FreestyleUtility : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField, TextArea] string freestyleJSON;

    [Header("Communication")]
    [Header("Receivers")]
    [SerializeField] CommunicationHandler<List<Vector3>> OnReceiveData;

    private void Start()
    {
        OnReceiveData.Register(this).OnReceiveSignal += GetFreestylePoints;
    }

    public void GetFreestylePoints(List<Vector3> points)
    {
        var jsonArray = new JSONArray();

        foreach(var point in points)
        {
            var data = new JSONArray();
            data.Add(point.x.ToString("0.00"));
            data.Add(point.y.ToString("0.00"));
            data.Add(point.z.ToString("0.00"));

            jsonArray.Add(data);
        }

        freestyleJSON = jsonArray.ToString();

        //Debug.Log($"[FreestyleUtility] Actual size of list of points: {points.Count * Marshal.SizeOf(typeof(Vector3))}");
    }
}

using Innoveam.Modules.Communication;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FreestyleCapture : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] LayerMask drawingLayer;
    [SerializeField] bool onlyCaptureFromGameObject;
    [SerializeField] bool clearOnDisable;
    [SerializeField] float yOffset;
    [SerializeField] float minMagnitude;

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler OnFreestyleStarted;
    [SerializeField] CommunicationHandler<List<Vector3>> OnFreestyleUpdated;
    [SerializeField] CommunicationHandler<List<Vector3>> OnFreestyleStopped;
    [SerializeField] CommunicationHandler<Vector3> OnFreestylePointAdded;

    public bool validDrawing = true;

    public Action<List<Vector3>> OnValidPointsAvailable;
    public Action<GameObject> OnValidPointsForTrackedGameObjectAvailable;

    bool isDrawing = false;
    Camera drawingCamera;

    List<Vector3> points = new List<Vector3>();

    Vector3 _lastPoint = Vector3.positiveInfinity;

    GameObject trackObject;

    void Start()
    {
        drawingCamera = Camera.main; // Default to main camera
    }

    private void OnDisable()
    {
        if (clearOnDisable) points.Clear();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Start drawing on left mouse click
        {
            isDrawing = true;
            points.Clear();

            OnFreestyleStarted.Broadcast();
        }

        if (Input.GetMouseButton(0) && validDrawing /*&& isDrawing*/) // While holding left mouse button
        {
            if (onlyCaptureFromGameObject && trackObject == null && isDrawing) return;
            //Vector3 mousePos = Input.mousePosition;
            //Vector3 worldPos = drawingCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f)); // Adjust Z for depth
            //AddPoint(worldPos);
            AddPointFromRaycast();
        }

        if (Input.GetMouseButtonUp(0)) // Stop drawing on release
        {
            isDrawing = false;

            ValidateDrawing();

            ClearTrackGameObject();

            OnFreestyleStopped.Broadcast(points);
        }
    }

    void AddPointFromRaycast()
    {
        // Perform a raycast from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, drawingLayer))
        {
            Vector3 hitPoint = hit.point;

            // Add the point only if it's far enough from the last point
            if (points.Count > 0 && Vector3.Distance(points[points.Count - 1], hitPoint) < minMagnitude)
            {
                return;
            }

            hitPoint.y += yOffset;

            _lastPoint = hitPoint;

            AddPoint(hitPoint);
        }
    }

    public void AddPointFromGameObject(GameObject target)
    {
        if (!isDrawing) return;

        //Debug.Log($"[FreestyleCapture] Received new input from GameObject \"{target.name}\"");
        AddPoint(target.transform.position);
    }

    void AddPoint(Vector3 value)
    {
        points.Add(value);

        OnFreestylePointAdded.Broadcast(value);
        OnFreestyleUpdated.Broadcast(points);
    }

    public void TrackGameObject(GameObject target)
    {
        if (isDrawing) return;

        trackObject = target;
    }

    public void ClearTrackGameObject()
    {
        if (isDrawing) return;

        trackObject = null;
    }

    void ValidateDrawing()
    {
        if (points.Count == 0) return;

        //Debug.Log($"[FreestyleCapture] Valid points retrieved!");

        OnValidPointsAvailable?.Invoke(points);
        OnValidPointsForTrackedGameObjectAvailable.Invoke(trackObject);
    }
}
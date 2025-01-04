using Innoveam.Modules.Communication;
using Innoveam.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

public class TFGMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] ScriptGraphAsset freestyleMovementAction;

    [Header("Communications")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<TFGActionRecord> OnRecordAction;
    //[Header("Receivers")]
    //[SerializeField] CommunicationHandler<GameObject> OnValidPointsForTrackedGameObjectAvailable;

    Spline cachedSplineData;

    private void Start()
    {
        //OnValidPointsForTrackedGameObjectAvailable.Register(this).OnReceiveSignal += Process;
    }

    public void Process(GameObject target)
    {
        PromptWindow.PromptContent promptContent = new PromptWindow.PromptContent();

        var yesEvent = new PromptWindow.PromptButtonEvent();
        var noEvent = new PromptWindow.PromptButtonEvent();

        yesEvent.name = "Yes";
        noEvent.name = "No";

        cachedSplineData = new Spline(splineContainer.Spline);

        yesEvent.callback = () =>
        {
            var knotsData = new List<Vector3>();

            foreach (var knot in cachedSplineData.Knots.Select(x => x.Position))
            {
                var knotItem = new Vector3(knot.x, knot.y, knot.z);

                knotsData.Add(knotItem);
            }

            var tfgCharacter = target.GetComponent<TFGCharacter>();
            if (tfgCharacter == null) return;

            var speed = (float)Variables.Scene(SceneManager.GetActiveScene())["walkSpeed"];

            var actionRecord = new TFGActionRecord();

            actionRecord.action = freestyleMovementAction;
            actionRecord.character = tfgCharacter;
            actionRecord.movementData = knotsData;
            actionRecord.duration = knotsData.GetLength() / speed;

            OnRecordAction.Broadcast(actionRecord);
        };
        noEvent.callback = null;

        promptContent.title = string.Empty;
        promptContent.details = $"You will move \"{target.gameObject.name}\" to this position.\nDo you want to proceed?";
        promptContent.options = new PromptWindow.PromptButtonEvent[] { yesEvent, noEvent };

        PromptWindow.instance.Show(promptContent);
    }
}

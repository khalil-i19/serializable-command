using Innoveam.Modules.Communication;
using Innoveam.Templates;
using SimpleJSON;
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

    Spline cachedSplineData;

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
            List<Vector3> splineKnots = new List<Vector3>();

            JSONObject scriptGraphData = new JSONObject();
            JSONArray knotData = new JSONArray();

            foreach (var knot in cachedSplineData.Knots.Select(x => x.Position))
            {
                JSONObject vector3Data = new JSONObject();
                vector3Data.Add("x", knot.x);
                vector3Data.Add("y", knot.y);
                vector3Data.Add("z", knot.z);

                knotData.Add(vector3Data);
                splineKnots.Add(new Vector3(knot.x, knot.y, knot.z));
            }

            var jsonData = JsonUtility.ToJson(new ListWrapper<Vector3>(splineKnots));
            var jsonObject = JSON.Parse(jsonData);

            scriptGraphData.Add("type", typeof(ListWrapper<Vector3>).AssemblyQualifiedName);
            scriptGraphData.Add("data", jsonObject);

            var tfgCharacter = target.GetComponent<TFGCharacter>();
            if (tfgCharacter == null) return;

            var speed = (float)Variables.Scene(SceneManager.GetActiveScene())["walkSpeed"];

            var actionRecord = new TFGActionRecord();

            actionRecord.action = freestyleMovementAction;
            actionRecord.character = tfgCharacter;
            actionRecord.data = scriptGraphData;
            actionRecord.duration = splineKnots.GetLength() / speed;

            OnRecordAction.Broadcast(actionRecord);
        };
        noEvent.callback = null;

        promptContent.title = string.Empty;
        promptContent.details = $"You will move \"{target.gameObject.name}\" to this position.\nDo you want to proceed?";
        promptContent.options = new PromptWindow.PromptButtonEvent[] { yesEvent, noEvent };

        PromptWindow.instance.Show(promptContent);
    }
}

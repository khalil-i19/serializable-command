using Innoveam.Modules.Communication;
using Innoveam.Templates;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class TFGMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] ScriptGraphAsset freestyleMovementAction;

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<(GameObject, Spline)> OnMoveCharacter;

    Spline cachedSplineData;

    public void PromptCharacterMovement(GameObject target)
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

            foreach(var knot in cachedSplineData.Knots.Select(x => x.Position))
            {
                var knotItem = new Vector3(knot.x, knot.y, knot.z);

                knotsData.Add(knotItem);
            }

            var tfgCharacter = target.GetComponent<TFGCharacter>();
            if (tfgCharacter == null) return;

            var variableContainer = target.GetComponent<Variables>();
            if (variableContainer == null) return;

            Variables.Object(target)["splineData"] = knotsData;
            Variables.Object(target)["distanceTraveled"] = 0f;
            tfgCharacter.RunCommand(freestyleMovementAction);
        };
        noEvent.callback = null;

        promptContent.title = string.Empty;
        promptContent.details = $"You will move \"{target.gameObject.name}\" to this position.\nDo you want to proceed?";
        promptContent.options = new PromptWindow.PromptButtonEvent[] { noEvent, yesEvent };

        PromptWindow.instance.Show(promptContent);
    }
}

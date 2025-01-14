using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using Innoveam.Modules.UIExtension;
using Innoveam.Templates;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//TODO: Reference name u/ object jgn pake dari GameObject
public class TFGCharacterInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TFGCharacter source;
    [SerializeField] TFGObjectInteractable target;
    [SerializeField] UILabel UILabel;
    [SerializeField] Transform actionButtonContainer;

    [Header("Prefabs")]
    [SerializeField] ComponentLookup actionButtonPrefab;

    [Header("Communication")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<TFGActionRecord> OnRecordAction;

    List<ScriptGraphAsset> actionList = new();

    ObjectPool<ComponentLookup> actionButtonPool;

    bool initialized = false;

    public void Initialize(TFGCharacter _source)
    {
        actionButtonPool = new(actionButtonPrefab);
        source = _source;
        UILabel.target = source.gameObject;
    }

    public void Show(TFGObjectInteractable _target, IEnumerable<ScriptGraphAsset> actionList)
    {
        target = _target;

        Populate(actionList);
    }

    void Populate(IEnumerable<ScriptGraphAsset> actionList)
    {
        this.actionList = new(actionList);

        actionButtonPool.Clear();

        foreach (var action in this.actionList)
        {
            var lookup = actionButtonPool.Instantiate(actionButtonContainer);

            var nameDisplay = lookup.Get<TextMeshProUGUI>("name");
            var button = lookup.Get<Button>("button");

            nameDisplay.text = action.name;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                PromptWindow.PromptContent promptContent = new PromptWindow.PromptContent();

                var yesEvent = new PromptWindow.PromptButtonEvent();
                var noEvent = new PromptWindow.PromptButtonEvent();

                yesEvent.name = "Yes";
                noEvent.name = "No";

                yesEvent.callback = () =>
                {
                    JSONObject scriptGraphData = new JSONObject();
                    
                    var jsonObject = new JSONObject();

                    jsonObject.Add("target", target.guid);
                    
                    scriptGraphData.Add("type", typeof(JSONNode).AssemblyQualifiedName);
                    scriptGraphData.Add("data", jsonObject);

                    var actionRecord = new TFGActionRecord();

                    actionRecord.action = action;
                    actionRecord.character = source;
                    actionRecord.data = scriptGraphData;
                    actionRecord.duration = 1f;

                    OnRecordAction.Broadcast(actionRecord);
                };
                noEvent.callback = null;

                promptContent.title = string.Empty;
                promptContent.details = $"You will <b>{action.name.ToLower()}</b> {target.gameObject.name.GetFormattedArticle("{0} <b>{1}</b>")}.\nDo you want to proceed?";
                promptContent.options = new PromptWindow.PromptButtonEvent[] { yesEvent, noEvent };

                PromptWindow.instance.Show(promptContent);
            });
        }
    }
}

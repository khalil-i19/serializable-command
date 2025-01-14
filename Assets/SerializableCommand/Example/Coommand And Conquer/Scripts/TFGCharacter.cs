using Innoveam;
using Innoveam.Modules.Communication;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public struct TagActionsPair
{
    public string tag;
    public List<string> actions;
}

public class TFGCharacter : TFGObject
{

    [Header("Action Sets")]
    [Header("Action sets are collection of other object's tags-action name pair. Define what tag(s) this object can interact and what action they can do")]
    [SerializeField] List<TagActionsPair> tagActionsPairs = new List<TagActionsPair>();

    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<object> OnCharacterStartInteracting;
    [SerializeField] CommunicationHandler<TFGCharacter> OnCharacterStopInteracting;

    ScriptMachineUtility scriptMachineUtility;
    Dictionary<string, List<ScriptGraphAsset>> tagScriptGraphAssetPair = new Dictionary<string, List<ScriptGraphAsset>>();

    bool initialized = false;

    #region MONO
    private void Start()
    {
        //if (!initialized) Initialize();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;
        if (!other.TryGetComponent<TFGObjectInteractable>(out var objectInteraction)) return;

        var actions = GetActions(objectInteraction);

        OnCharacterStartInteracting?.Broadcast((this, objectInteraction, actions));
    }

    private void OnTriggerExit(Collider other)
    {
        OnCharacterStopInteracting?.Broadcast(this);
    }
    #endregion

    #region METHODS
    public void Initialize()
    {
        if (initialized) return;

        scriptMachineUtility = GetComponent<ScriptMachineUtility>();

        InitializeActions();

        initialized = true;
    }

    public void RunCommand(ScriptGraphAsset scriptGraphAsset)
    {
        scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAsset);
    }

    public void ClearCommand()
    {
        scriptMachineUtility.SetScriptGraphAsset(null);
    }

    void InitializeActions()
    {
        tagScriptGraphAssetPair.Clear();
        PopulateActions();
    }

    void PopulateActions()
    {
        foreach (var tagActionsPair in tagActionsPairs)
        {
            var actions = tagActionsPair.actions;
            foreach (var action in actions)
            {
                RegisterAction(tagActionsPair.tag, action);
            }
        }
    }

    void RegisterAction(string interactableTag, string actionName)
    {
        if (!tagScriptGraphAssetPair.ContainsKey(interactableTag))
        {
            tagScriptGraphAssetPair.Add(interactableTag, new());
        }

        var scriptGraphAssetList = tagScriptGraphAssetPair[interactableTag];
        var scriptGraphAsset = TFGActionLibrary.Get(actionName);
        scriptGraphAssetList.Add(scriptGraphAsset);

        Debug.Log($"Registered {actionName} to {interactableTag}");
    }

    List<ScriptGraphAsset> GetActions(TFGObjectInteractable interactableObject)
    {
        var result = new List<ScriptGraphAsset>();

        foreach (var tag in interactableObject.tags)
        {
            result.AddRange(tagScriptGraphAssetPair[tag]);
        }

        return result;
    }
    #endregion
}

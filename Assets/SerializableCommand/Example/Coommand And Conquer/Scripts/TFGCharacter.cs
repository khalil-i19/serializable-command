using Innoveam.Modules.Communication;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TFGCharacter : TFGObject, Innoveam.IInitializable
{
    public string id;

    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<(TFGCharacter, TFGObjectInteractable)> OnCharacterStartInteracting;
    [SerializeField] CommunicationHandler<TFGCharacter> OnCharacterStopInteracting;

    ScriptMachineUtility scriptMachineUtility;

    bool initialized = false;

    #region MONO
    private void Start()
    {
        if (!initialized) Initialize();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;
        if (!other.TryGetComponent<TFGObjectInteractable>(out var objectInteraction)) return;

        OnCharacterStartInteracting?.Broadcast((this, objectInteraction));
        //Debug.Log($"{gameObject.name} entered interaction with {other.gameObject.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;

        OnCharacterStopInteracting?.Broadcast(this);
        //Debug.Log($"{gameObject.name} exited interaction with {other.gameObject.name}");
    }
    #endregion

    #region METHODS
    public void Initialize()
    {
        if (initialized) return;

        scriptMachineUtility = GetComponent<ScriptMachineUtility>();

        initialized = true;
    }

    public void RunCommand(ScriptGraphAsset scriptGraphAsset)
    {
        if (!actions.Contains(scriptGraphAsset)) return;

        scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAsset);
    }

    public void ClearCommand()
    {
        scriptMachineUtility.SetScriptGraphAsset(null);
    }

    public void LoadAddressableScriptGraphAssets(List<object> addressableScriptGraphAssets)
    {
        actions = new();

        foreach (var asset in addressableScriptGraphAssets)
        {
            try
            {
                var scriptGraphAsset = (ScriptGraphAsset)asset;
                actions.Add(scriptGraphAsset);
            }
            catch
            {
                Debug.Log($"[ScriptMachineTester] Error while parsing AddressableAsset to ScriptGraphAsset");
                continue;
            }
        }
    }
    #endregion
}

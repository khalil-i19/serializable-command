using Innoveam;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TFGCharacter : MonoBehaviour, Innoveam.IInitializable
{
    public string id;

    public List<ScriptGraphAsset> actionSet = new();

    ScriptMachineUtility scriptMachineUtility;

    bool initialized = false;

    private void Start()
    {
        if (!initialized) Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;

        scriptMachineUtility = GetComponent<ScriptMachineUtility>();

        initialized = true;
    }

    public void RunCommand(ScriptGraphAsset scriptGraphAsset)
    {
        if (!actionSet.Contains(scriptGraphAsset)) return;

        scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAsset);
    }

    public void LoadAddressableScriptGraphAssets(List<object> addressableScriptGraphAssets)
    {
        actionSet = new();

        foreach (var asset in addressableScriptGraphAssets)
        {
            try
            {
                var scriptGraphAsset = (ScriptGraphAsset)asset;
                actionSet.Add(scriptGraphAsset);
            }
            catch
            {
                Debug.Log($"[ScriptMachineTester] Error while parsing AddressableAsset to ScriptGraphAsset");
                continue;
            }
        }
    }
}

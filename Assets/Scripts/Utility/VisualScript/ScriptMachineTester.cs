using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class ScriptMachineTester : MonoBehaviour
{
    public ScriptMachineUtility scriptMachineUtility;

    public List<ScriptGraphAsset> scriptGraphAssets;

    [ContextMenu("Set to \"Set to First ScriptGraphAsset\"")]
    void SetToFirst() => scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAssets[0]);
    [ContextMenu("Set to \"Set to Second ScriptGraphAsset\"")]
    void SetToSecond() => scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAssets[1]);
    [ContextMenu("Set to \"Set to Third ScriptGraphAsset\"")]
    void SetToThird() => scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAssets[2]);
    [ContextMenu("Set to \"Set to Fourth ScriptGraphAsset\"")]
    void SetToFourth() => scriptMachineUtility.SetAndStartScriptGraphAsset(scriptGraphAssets[3]);

    public void LoadAddressableScriptGraphAssets(List<object> addressableScriptGraphAssets)
    {
        Debug.Log($"[ScriptMachineTester] Begin populating AddressableAssets");
        Debug.Log(addressableScriptGraphAssets.Count);

        scriptGraphAssets = new();

        foreach (var asset in addressableScriptGraphAssets)
        {
            try
            {
                var scriptGraphAsset = (ScriptGraphAsset)asset;
                scriptGraphAssets.Add(scriptGraphAsset);
            }
            catch
            {
                Debug.Log($"[ScriptMachineTester] Error while parsing AddressableAsset to ScriptGraphAsset");
                continue;
            }
        }
    }
}

using Innoveam;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TFGActionLibrary : MonoBehaviour
{
    public static TFGActionLibrary Instance;

    public SerializableDictionary<string, ScriptGraphAsset> scriptGraphAssets = new();

    private void Start()
    {
        Instance = this;
    }

    public void OnActionAssetsLoaded(List<object> data)
    {
        scriptGraphAssets.Clear();
        foreach (object obj in data)
        {
            var scriptGraphAsset = obj as ScriptGraphAsset;
            scriptGraphAssets.Add(scriptGraphAsset.name, scriptGraphAsset);
        }
    }

    public static ScriptGraphAsset Get(string name) => Instance.scriptGraphAssets[name];
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableUtility : MonoBehaviour
{
    public string label;

    public bool loadOnAwake;

    public Action<object> OnAddressableAssetLoaded;
    public Action<object> OnEachAddressableAssetsLoaded;
    public Action<List<object>> OnAddressableAssetsLoaded;

    AsyncOperationHandle loadHandler;

    private void Awake()
    {
        if (!loadOnAwake) return;
        if (string.IsNullOrEmpty(label)) return;

        LoadAddressables();
    }

    private void OnDestroy()
    {
        Addressables.Release(loadHandler);
    }

    [ContextMenu("Load Addressables")]
    public void LoadAddressables()
    {
        StartCoroutine(LoadingAddressables());
    }

    IEnumerator LoadingAddressables()
    {
        Debug.Log($"Start loading Addressable with label: {label}");

        List<object> loadedAddressableAssets = new List<object>();
        loadHandler = Addressables.LoadAssetsAsync<object>(label, asset =>
        {
            Debug.Log($"Loaded Addressable asset with type \"{asset.GetType()}\"");

            loadedAddressableAssets.Add(asset);
            OnEachAddressableAssetsLoaded?.Invoke(asset);
        });

        yield return loadHandler;

        OnAddressableAssetsLoaded?.Invoke(loadedAddressableAssets);

        Debug.Log($"Stop loading Addressable with label: {label}");
    }

}

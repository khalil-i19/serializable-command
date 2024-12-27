using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TFGSessionHistory : MonoBehaviour
{
    [SerializeField] DatabaseObject sessionHistory;

    [Header("Communications")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<string> OnImportData;

    [Header("Prefabs")]
    [SerializeField] ComponentLookup buttonPrefab;

    [Header("References")]
    [SerializeField] ComponentLookup componentLookup;

    ObjectPool<ComponentLookup> buttonPool;

    Transform ButtonContainer => componentLookup.Get<Transform>("button-container");

    private void Start()
    {
        buttonPool = new ObjectPool<ComponentLookup>(buttonPrefab);

        Refresh();
    }

    public void Refresh()
    {
        buttonPool.Clear();

        foreach(var item in sessionHistory.data.childs)
        {
            var buttonLookup = buttonPool.Instantiate(ButtonContainer);

            var button = buttonLookup.Get<Button>("button");
            var buttonText = buttonLookup.Get<TextMeshProUGUI>("button-text");

            buttonText.text = item.id;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OnImportData.Broadcast(item.stringValue);
                Debug.Log(item.stringValue);
            });
        }
    }
}
using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TFGUI : MonoBehaviour, IInitializable
{
    [Header("Prefabs")]
    [SerializeField] ComponentLookup currentCharacterActionButtonLookup;

    [Header("Reference")]
    [SerializeField] Transform currentCharacterActionButtonsContainer;
    [SerializeField] ComponentLookup currentCharacterPanel;

    [Header("Communications")]
    [Header("Receiver")]
    [SerializeField] CommunicationHandler<TFGCharacter> OnUpdateCurrentCharacter;

    ObjectPool<ComponentLookup> currentCharacterActionButtons;

    bool initialized = false;

    private void Start()
    {
        if (!initialized) Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;

        OnUpdateCurrentCharacter.Register(this).OnReceiveSignal += UpdateCurrentCharacterPanel;

        currentCharacterActionButtons = new(currentCharacterActionButtonLookup);

        initialized = true;
    }

    private void UpdateCurrentCharacterPanel(TFGCharacter character)
    {
        var currentCharacterName = currentCharacterPanel.Get<TextMeshProUGUI>("current-character-text");
        currentCharacterName.text = character.name;

        currentCharacterActionButtons.Clear();

        //foreach (var action in character.GetActions())
        //{
        //    var currentCharacterActionButton = currentCharacterActionButtons.Instantiate(currentCharacterActionButtonsContainer);

        //    var button = currentCharacterActionButton.Get<Button>("button");
        //    var buttonText = currentCharacterActionButton.Get<TextMeshProUGUI>("button-text");

        //    buttonText.text = action.name.InsertSpace();
        //    button.onClick.RemoveAllListeners();
        //    button.onClick.AddListener(() => { character.RunCommand(action); });
        //}
    }
}

using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RPGBattleSessionViewer : MonoBehaviour, IInitializable
{
    #region PROPERTIES
    [SerializeField] DatabaseObject sessionSetting;

    [Header("Settings")]
    [SerializeField] Color playerColor;
    [SerializeField] Color enemyColor;

    [Header("Prefabs")]
    [SerializeField] ComponentLookup characterQueueItemPrefab;
    [SerializeField] ComponentLookup actionButton;

    [Header("References")]
    [SerializeField] Transform characterQueueContainer;
    [SerializeField] ComponentLookup commandDisplay;

    [Header("Communications")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler OnEndTurn;
    [SerializeField] CommunicationHandler<string> OnCommandSelected;
    [SerializeField] CommunicationHandler<RPGCharacter> OnActiveCharacterChanged;
    [SerializeField] CommunicationHandler<List<RPGCharacter>> OnTargetsSelected;
    [Header("Receivers")]
    [SerializeField] CommunicationHandler<List<RPGCharacter>> OnCharacterQueueUpdated;
    [SerializeField] CommunicationHandler<List<RPGCharacter>> OnEnemyUpdated;
    [SerializeField] CommunicationHandler<RPGCharacter> OnActiveCharacterUpdated;
    [SerializeField] CommunicationHandler<RPGCommandBase> OnCommandExecuted;

    [Header("Active Character Menu")]
    [SerializeField] ComponentLookup activeCharacterMenu;

    int queueCapacity;
    RPGCommandData commandData = new();
    List<ComponentLookup> characterQueueItems = new();
    ObjectPool<ComponentLookup> skillButtons;
    ObjectPool<ComponentLookup> targetButtons;

    bool initialized = false;

    Button AttackButton => activeCharacterMenu.Get<Button>("attack-button");
    TextMeshProUGUI AttackButtonDisplay => activeCharacterMenu.Get<TextMeshProUGUI>("attack-display");
    Button SkillButton => activeCharacterMenu.Get<Button>("skill-button");
    TextMeshProUGUI SkillButtonDisplay => activeCharacterMenu.Get<TextMeshProUGUI>("skill-display");
    Transform ActionButtonContainer => activeCharacterMenu.Get<Transform>("action-container");
    Transform SkillButtonContainer => activeCharacterMenu.Get<Transform>("skill-container");
    Transform TargetButtonContainer => activeCharacterMenu.Get<Transform>("target-container");

    GameObject CommandPanel => commandDisplay.Get<GameObject>("command-panel");
    TextMeshProUGUI CommandDisplay => commandDisplay.Get<TextMeshProUGUI>("command-display");

    #endregion

    #region METHODS
    private void Start()
    {
        if (!initialized) Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;

        OnCharacterQueueUpdated.Register(this).OnReceiveSignal += UpdateQueue;
        OnActiveCharacterUpdated.Register(this).OnReceiveSignal += UpdateActiveCharacterView;
        OnEnemyUpdated.Register(this).OnReceiveSignal += UpdateTargets;

        OnCommandExecuted.Register(this).OnReceiveSignal += DisplayCommand;

        skillButtons = new(actionButton);
        targetButtons = new(actionButton);

        InitializeCharacterQueueViewer();

        initialized = true;
    }

    public void ShowActionPanel()
    {
        ActionButtonContainer.gameObject.SetActive(true);
        SkillButtonContainer.gameObject.SetActive(false);
        TargetButtonContainer.gameObject.SetActive(false);
    }

    public void HideActionPanel()
    {
        ActionButtonContainer.gameObject.SetActive(false);
        SkillButtonContainer.gameObject.SetActive(false);
        TargetButtonContainer.gameObject.SetActive(false);
    }

    #region PRIVATE
    void UpdateQueue(List<RPGCharacter> characterQueue)
    {
        //if (characterQueue.Count != characterQueueItems.Count) return;

        if (characterQueueItems.Count != queueCapacity) InitializeCharacterQueueViewer();

        characterQueueContainer.gameObject.SetActive(true);

        for (int i = 0; i < queueCapacity; i++)
        {
            var item = characterQueueItems[i];

            if (i >= characterQueue.Count)
            {
                characterQueueItems[i].gameObject.SetActive(false);
                continue;
            }

            var characterData = characterQueue[i];

            item.Get<TextMeshProUGUI>("name").text = characterData.CharacterData.name;
            item.Get<Image>("portrait").sprite = characterData.CharacterData.portrait;
            item.Get<Image>("background").color = characterData.CharacterData.isNPC ? enemyColor : playerColor;
        }
    }

    void UpdateActiveCharacterView(RPGCharacter character)
    {
        activeCharacterMenu.gameObject.SetActive(!character.CharacterData.isNPC);

        if (character.CharacterData.isNPC) return;

        ShowActionPanel();
        HideCommandPanel();

        OnActiveCharacterChanged.Broadcast(character);

        var skills = character.actionSets.data["Skills"];

        AttackButton.gameObject.SetActive(character.actionSets.data.ContainsKey("Attack"));

        SkillButton.gameObject.SetActive(skills.childs.Count > 0);
        if (skills.childs.Count > 0)
        {
            skillButtons.Clear();
            foreach (var skill in skills.childs)
            {
                var skillName = skill.id.InsertSpace();

                var skillButtonLookup = skillButtons.Instantiate(SkillButtonContainer);
                var skillButtonName = skillButtonLookup.Get<TextMeshProUGUI>("button-name");
                var skillButton = skillButtonLookup.Get<Button>("button");

                skillButtonName.text = skillName;
                skillButton.onClick.RemoveAllListeners();
                skillButton.onClick.AddListener(() =>
                {
                    TargetButtonContainer.gameObject.SetActive(true);

                    OnCommandSelected.Broadcast(skill.id);
                });
            }
        }
    }

    void UpdateTargets(List<RPGCharacter> enemies)
    {
        targetButtons.Clear();

        foreach (var enemy in enemies)
        {
            var enemyButtonLookup = targetButtons.Instantiate(TargetButtonContainer);
            var enemyButtonName = enemyButtonLookup.Get<TextMeshProUGUI>("button-name");
            var enemyButton = enemyButtonLookup.Get<Button>("button");

            enemyButtonName.text = enemy.CharacterData.name.InsertSpace();
            enemyButton.onClick.RemoveAllListeners();
            enemyButton.onClick.AddListener(() =>
            {
                OnTargetsSelected.Broadcast(new List<RPGCharacter> { enemy });
            });
        }
    }

    void InitializeCharacterQueueViewer()
    {
        queueCapacity = sessionSetting.data["battleSession"]["queueCapacity"].intValue;

        if (characterQueueItems.Count > 0)
        {
            for (int i = 0; i < characterQueueItems.Count; i++)
            {
                var active = characterQueueItems[0];
                Destroy(active);
            }
        }

        characterQueueItems = new();

        for (int i = 0; i < queueCapacity; i++)
        {
            characterQueueItems.Add(Instantiate(characterQueueItemPrefab, characterQueueContainer));
        }

        characterQueueContainer.gameObject.SetActive(false);
    }

    void DisplayCommand(RPGCommandBase command)
    {
        CommandDisplay.text = command.GetType().ToString().InsertSpace();
        CommandPanel.SetActive(true);
    }

    public void HideCommandPanel()
    {
        CommandPanel.SetActive(false);
    }
    #endregion
    #endregion
}

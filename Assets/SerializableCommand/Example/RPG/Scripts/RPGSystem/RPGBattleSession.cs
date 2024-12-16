using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RPGBattleSession : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] List<RPGCharacter> _players = new List<RPGCharacter>();
    [SerializeField] List<RPGCharacter> _enemies = new List<RPGCharacter>();

    [SerializeField] DatabaseObject _sessionSetting;

    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler<List<RPGCharacter>> OnCharacterQueueUpdated;
    [SerializeField] CommunicationHandler<List<RPGCharacter>> OnEnemyUpdated;
    [SerializeField] CommunicationHandler<RPGCharacter> OnActiveCharacterUpdated;
    [SerializeField] CommunicationHandler<GameObject> OnActiveCharacterObjectUpdated;
    [SerializeField] CommunicationHandler<RPGCommandBase> OnCommandExecuted;
    [Header("Receivers")]
    [SerializeField] CommunicationHandler OnNextCharacter;
    [SerializeField] CommunicationHandler<RPGCommandBase> OnRegisterCommand;

    [Header("Runtime")]
    [Header("Session")]
    [SerializeField] int _queueCapacity;
    [SerializeField] int _requiredTurnPoint;
    [SerializeField] List<RPGCharacter> _characterQueue;

    int lastCheckedIndex = 0;
    bool _started = false;
    RPGCharacter _currentCharacter;
    
    List<RPGCharacter> _characters = new List<RPGCharacter>();
    #endregion

    #region METHODS
    #region PUBLIC
    private void Start()
    {
        _started = false;

        OnNextCharacter.Register(this).OnReceiveSignal += value => NextCharacter();
        OnRegisterCommand.Register(this).OnReceiveSignal += value => StartCoroutine(ExecutingCommand(value));
    }

    [ContextMenu("Begin Session")]
    public void BeginSession()
    {
        _characters = new List<RPGCharacter>();
        _characters.AddRange(_players);
        _characters.AddRange(_enemies);
        BeginSession(_characters);
    }

    public void BeginSession(List<RPGCharacter> characters)
    {
        if (_started) return;

        if (characters.Any(x => x.CharacterData.spd == 0))
        {
            Debug.LogError("[RPGBattleSession] One of listed RPGCharacter's SPD is 0");
            return;
        }

        _queueCapacity = _sessionSetting.data["battleSession"]["queueCapacity"].intValue;
        _requiredTurnPoint = _sessionSetting.data["battleSession"]["requiredTurnPoint"].intValue;

        _characterQueue = new List<RPGCharacter>();

        foreach (var character in characters)
        {
            character.ResetTurnPoint();
        }

        PopulateQueue();

        OnCharacterQueueUpdated.Broadcast(_characterQueue);
        OnEnemyUpdated.Broadcast(_enemies);

        NextCharacter();
    }
    #endregion

    #region PRIVATE
    RPGCharacter[] CalculateNextTurn()
    {
        //TODO: Cara nentuin siapa yang duluan masih salah

        int needsToBeFilled = _queueCapacity - _characterQueue.Count;
        int currentResult = 0;

        RPGCharacter[] result = new RPGCharacter[needsToBeFilled];

        while (currentResult < needsToBeFilled)
        {
            Debug.Log($"[RPGBattleSession] Current populating progress: {currentResult}:{needsToBeFilled}");
            for (int i = lastCheckedIndex; i < _characters.Count; i++)
            {
                var character = _characters[i];

                character.Advance();
                Debug.Log($"[RPGBattleSession] {character.CharacterData.name} => {character.currentTurnPoint}");
                if (character.currentTurnPoint >= _requiredTurnPoint)
                {
                    character.ResetTurnPoint(_requiredTurnPoint);
                    result[currentResult] = character;
                    currentResult++;

                    if (currentResult >= needsToBeFilled)
                    {
                        lastCheckedIndex = (_characters.IndexOf(character) + 1) % _characters.Count;
                        break;
                    }
                }
                lastCheckedIndex = 0;
            }
        }

        return result;
    }

    void PopulateQueue()
    {
        _characterQueue.AddRange(CalculateNextTurn());
    }

    [ContextMenu("Next Character")]
    void NextCharacter()
    {
        _currentCharacter = _characterQueue.First();
        _characterQueue.RemoveAt(0);

        PopulateQueue();

        OnCharacterQueueUpdated.Broadcast(_characterQueue);
        OnActiveCharacterObjectUpdated.Broadcast(_currentCharacter.gameObject);
        OnActiveCharacterUpdated.Broadcast(_currentCharacter);
    }

    IEnumerator ExecutingCommand(RPGCommandBase command)
    {
        Debug.Log($"Executing {command.GetType().ToString()}");

        yield return command.Execute();

        OnCommandExecuted.Broadcast(command);

        yield return new WaitForSeconds(2f);

        NextCharacter();

        yield break;
    }
    #endregion
    #endregion
}
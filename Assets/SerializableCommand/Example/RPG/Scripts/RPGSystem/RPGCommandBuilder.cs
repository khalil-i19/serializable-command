using Innoveam;
using Innoveam.Modules.Communication;
using Innoveam.Modules.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RPGCommandData
{
    public RPGCharacter invoker;
    public List<RPGCharacter> targets;
    public string command;
}

public class RPGCommandBuilder : MonoBehaviour, IInitializable
{
    [Header("Communications")]
    [Header("Broadcasters")]
    [SerializeField] CommunicationHandler OnEndTurn;
    [SerializeField] CommunicationHandler<RPGCommandBase> SendCommand;
    [Header("Receivers")]
    [SerializeField] CommunicationHandler<RPGCharacter> OnRegisterInvoker;
    [SerializeField] CommunicationHandler<List<RPGCharacter>> OnRegisterTargets;
    [SerializeField] CommunicationHandler<string> OnRegisterCommand;

    bool initialized = false;

    [SerializeField] RPGCommandData commandData;

    void Start()
    {
        if (!initialized) Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;

        OnRegisterInvoker.Register(this).OnReceiveSignal += RegisterInvoker;
        OnRegisterTargets.Register(this).OnReceiveSignal += RegisterTargets;
        OnRegisterCommand.Register(this).OnReceiveSignal += RegisterCommand;

        initialized = true;
    }

    public void BuildCommand()
    {
        if (commandData.invoker == null) //return;
        {
            Debug.Log("Invoker null");

            return;
        }
        if (string.IsNullOrEmpty(commandData.command)) //return;
        {
            Debug.Log("Command empty");

            return;
        }
        if (commandData.targets == null) //return;
        {
            Debug.Log("Targets null");

            return;
        }
        if (commandData.targets.Count == 0) //return;
        {
            Debug.Log("Targets empty");

            return;
        }

        Type commandType = Type.GetType(commandData.command);
        RPGCommandBase command = (RPGCommandBase)JsonUtility.FromJson("{}", commandType);
        command.invoker = commandData.invoker;
        command.targets = commandData.targets;

        Debug.Log(command.Serialize());
        SendCommand.Broadcast(command);
        //OnEndTurn.Broadcast();
    }

    private void RegisterCommand(string command)
    {
        commandData.command = command;
    }

    private void RegisterTargets(List<RPGCharacter> targets)
    {
        commandData.targets = targets;
    }

    private void RegisterInvoker(RPGCharacter character)
    {
        commandData = new();
        commandData.invoker = character;
    }
}

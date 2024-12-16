using Innoveam;
using Innoveam.Modules.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RPGCharacter : MonoBehaviour
{
    #region PROPERTIES
    [System.Serializable]
    public class Data
    {
        public Sprite portrait;
        public string name;
        public int hp;
        public int sp;
        public int spd;
        public int def;
        public int atk;
        public bool isNPC;
    }

    public DatabaseObject actionSets;

    [SerializeField] Data _characterData;

    public Data CharacterData => _characterData;

    //public Dictionary<string, List<RPGCommandBase>> _commands;
    //public CommandBase basicAttack;
    //public List<CommandBase> skills;
    //public CommandBase defend;

    public int currentTurnPoint { get; private set; }
    #endregion

    #region METHODS
    #region PUBLIC
    public void ResetTurnPoint() => currentTurnPoint = 0;
    public void ResetTurnPoint(int modulo) => currentTurnPoint %= modulo;
    public void Advance(int value) => currentTurnPoint += value;
    public void Advance() => currentTurnPoint += _characterData.spd;

    private void Start()
    {
        //_commands = new Dictionary<string, List<RPGCommandBase>>();

        if (actionSets == null) return;

        foreach (var command in actionSets.data.childs)
        {
            if (command.childs.Count > 0)
            {
                //var commands = new List<RPGCommandBase>();

                //foreach (var child in command.childs)
                //{
                //    var commandItem = RPGCommandFactory.Get(child.id);
                //    if (commandItem == null) continue;

                //    commands.Add(commandItem);
                //}
                //_commands.Add(command.id, commands);
            }
            else
            {
                //_commands.Add(command.id, new() { RPGCommandFactory.Get(command.id) });
            }
        }
    }
    #endregion
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeType { Holy, Dark, Fire, Ice, Wind }

public class RPGCommandJSON
{
    public string invoker;
    public string[] targets;
    public string commandType;
    public string commandData;
}

public abstract class RPGCommandBase : CommandBase
{
    public RPGCharacter invoker;
    public List<RPGCharacter> targets;
}

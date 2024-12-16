using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public interface CommandBase
//{
//    IEnumerator Execute();
//    IEnumerator Undo();
//    CommandData Serialize();
//}

public abstract class CommandBase
{
    //public CommandData commandData;

    public virtual IEnumerator Execute()
    {
        yield break;
    }

    public abstract IEnumerator Undo();

    public abstract void Deserialize(CommandData commandData);
    public virtual string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
}
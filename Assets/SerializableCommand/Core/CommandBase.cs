using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    IEnumerator Execute();
    IEnumerator Undo();
    CommandData Serialize();
}

public abstract class CommandBase : ICommand
{
    public abstract IEnumerator Execute();

    public abstract IEnumerator Undo();

    public abstract CommandData Serialize();
}
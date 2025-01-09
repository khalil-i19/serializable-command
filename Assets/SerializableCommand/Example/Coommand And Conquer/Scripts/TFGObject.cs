using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TFGObject : MonoBehaviour
{
    [SerializeField] protected List<ScriptGraphAsset> actions;

    public List<ScriptGraphAsset> GetActions() => actions;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandSerializer : MonoBehaviour
{
    [ContextMenu("Try Serialize")]
    public void TrySerialize()
    {
        var fireAttack = new FireAttack();
        Debug.Log(fireAttack.Serialize());

        var darkAttack = new DarkAttack();
        Debug.Log(darkAttack.Serialize());
    }
}

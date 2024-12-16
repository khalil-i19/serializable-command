using Innoveam;
using Innoveam.Modules.Communication;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RPGCommandManager : MonoBehaviour, IInitializable
{
    #region PROPERTIES
    [Header("Communications")]
    [Header("Receiver")]
    [SerializeField] CommunicationHandler<RPGCommandBase> OnRegister;

    List<RPGCommandBase> commands = new List<RPGCommandBase>();

    bool initialized = false;
    #endregion

    #region METHODS
    #region PUBLIC
    private void Start()
    {
        if (!initialized) Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;

        OnRegister.Register(this).OnReceiveSignal += Register;

        initialized = true;
    }

    public void Register(RPGCommandBase command)
    {
        commands.Add(command);
    }

    [ContextMenu("Export History")]
    public void ExportHistory()
    {
        var jsonNode = new JSONObject();
        var RPGCommandsJSON = new JSONArray();

        foreach (var command in commands)
        {
            var commandJSON = JSON.Parse(JsonUtility.ToJson(command));
            commandJSON.Remove("invoker");
            commandJSON.Remove("targets");

            var targetsJSON = new JSONArray();
            foreach (var target in command.targets)
            {
                targetsJSON.Add(target.CharacterData.name);
            }

            var RPGCommandData = new JSONObject();
            RPGCommandData.Add("invoker", command.invoker.CharacterData.name);
            RPGCommandData.Add("targets", targetsJSON);
            RPGCommandData.Add("commandType", command.GetType().ToString());
            RPGCommandData.Add("commandData", commandJSON);

            RPGCommandsJSON.Add(RPGCommandData);
        }

        jsonNode.Add("data", RPGCommandsJSON);

        Debug.Log(jsonNode.ToString());
    }
    #endregion
    #region PRIVATE
    void ResetCommandHistory() => commands.Clear();
    #endregion
    #endregion
}

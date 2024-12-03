using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    private List<ICommand> _commands = new List<ICommand>();

    public void AddCommand(ICommand command)
    {
        _commands.Add(command);
    }

    public void ExecuteAll()
    {
        foreach (var command in _commands)
        {
            command.Execute();
        }
    }

    public string SerializeToJson()
    {
        List<CommandData> serializedCommands = _commands
            .Select(cmd => cmd.Serialize())
            .ToList();
        return JsonUtility.ToJson(new CommandWrapper { Commands = serializedCommands });
    }

    public void DeserializeFromJson(string json)
    {
        var deserialized = JsonUtility.FromJson<CommandWrapper>(json);
        _commands.Clear();

        foreach (var data in deserialized.Commands)
        {
            ICommand command = CommandFactory.CreateCommand(data);
            if (command != null)
            {
                _commands.Add(command);
            }
        }
    }

    // Optionally save the serialized commands to a file (persistent storage)
    public void SaveCommandsToFile(string fileName)
    {
        string json = SerializeToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileName, json);
        Debug.Log($"Commands saved to {Application.persistentDataPath}/{fileName}");
    }

    // Optionally load commands from a file
    public void LoadCommandsFromFile(string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName;
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            DeserializeFromJson(json);
            Debug.Log("Commands loaded successfully.");
        }
        else
        {
            Debug.LogError("Command file not found!");
        }
    }

    [System.Serializable]
    private class CommandWrapper
    {
        public List<CommandData> Commands;
    }
}

using UnityEngine;

public static class CommandFactory
{
    public static CommandBase CreateCommand(CommandData data)
    {
        switch (data.CommandType)
        {
            default:
                Debug.LogError($"Unknown command type: {data.CommandType}");
                return null;
        }
    }
}
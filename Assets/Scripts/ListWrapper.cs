using System;
using System.Collections.Generic;

[System.Serializable]
public class ListWrapper<T>
{
    public List<T> list;

    public ListWrapper(List<T> list)
    {
        this.list = list;
    }
}

public static class TypeHelper
{
    public static Type GetTypeFromAllAssemblies(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }
}
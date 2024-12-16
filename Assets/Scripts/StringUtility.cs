using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public static class StringUtility
{ 
    public static string InsertSpace(this string value)
    {
        return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    }
}

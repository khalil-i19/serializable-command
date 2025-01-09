using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringUtility
{
    static HashSet<char> Vowels = new HashSet<char> { 'A', 'E', 'I', 'O', 'U', 'a', 'e', 'i', 'o', 'u' };

    public static string InsertSpace(this string value)
    {
        return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    }

    public static string GetArticle(this string value)
    {
        // Check if the first letter is a vowel
        char firstLetter = value[0];
        return $"{(Vowels.Contains(firstLetter) ? "an" : "a")} {value}";
    }

    /// <summary>
    /// "format" must contain two references; the article and the original string value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetFormattedArticle(this string value, string format = null)
    {
        char firstLetter = value[0];
        var article = Vowels.Contains(firstLetter) ? "an" : "a";

        return string.Format(format, article, value);
    }
}

using Innoveam;
using UnityEngine;
using UnityEngine.Rendering;

public class TFGCharacterFactory : MonoBehaviour
{
    static TFGCharacterFactory instance;

    [SerializeField] SerializableDictionary<string, TFGCharacter> characters;

    private void Start()
    {
        instance = this;
    }

    public static TFGCharacter GetCharacter(string guid) => instance.characters[guid];
}

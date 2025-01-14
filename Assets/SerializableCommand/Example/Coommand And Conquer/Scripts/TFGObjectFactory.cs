using Innoveam;
using UnityEngine;
using UnityEngine.Rendering;

public class TFGObjectFactory : MonoBehaviour
{
    static TFGObjectFactory instance;

    [SerializeField] SerializableDictionary<string, TFGObject> objects;

    private void Start()
    {
        instance = this;
    }

    public static TFGObject GetObject(string guid) => instance.objects[guid];
    public static GameObject GetGameObject(string guid) => GetObject(guid).gameObject;
}

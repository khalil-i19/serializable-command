using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public static class PlayableDirectorUtility
{
    public static void ClearAllBindings(this PlayableDirector playableDirector)
    {
        if (playableDirector.playableAsset == null) return;

        foreach (var output in playableDirector.playableAsset.outputs)
        {
            // Clear the binding for each output
            playableDirector.ClearGenericBinding(output.sourceObject);
        }
    }
}

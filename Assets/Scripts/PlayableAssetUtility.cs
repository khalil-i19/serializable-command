using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class PlayableAssetUtility
{
    public static T GetTrackAsset<T>(this TimelineAsset timelineAsset, string name) where T : TrackAsset
    {
        return (T)timelineAsset.GetRootTracks().Select(x => x.name == name);
    }

    public static T FirstOrNew<T>(this TimelineAsset timelineAsset, string name) where T : TrackAsset
    {
        var result = timelineAsset.GetRootTracks().FirstOrDefault(x => x.name == name);

        if (result == null)
        {
            result = timelineAsset.CreateTrack(typeof(T), null, name);
        }

        return (T)result;
    }
}

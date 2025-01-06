using System.Collections;
using System.Collections.Generic;
using Innoveam.Modules.Data;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class TimelineManagerUI : MonoBehaviour
{
    [SerializeField] ComponentLookup componPrefab;
    [SerializeField] private List<PlayableAssetData> storedPlayableAssets = new();
    public PlayableDirector PlayableDirector;
    public float timelineScale = 100f;
    public float timebarLength = 1000f;

    public void PopulatePlayableAssets()
    {
        if (PlayableDirector != null)
        {
            TimelineAsset timelineAsset = PlayableDirector.playableAsset as TimelineAsset;

            if (timelineAsset != null)
            {
                foreach (var track in timelineAsset.GetOutputTracks())
                {
                    foreach (var clip in track.GetClips())
                    {
                        PlayableAsset playableAsset = clip.asset as PlayableAsset;
                        storedPlayableAssets.Add(new PlayableAssetData
                        {
                            playable = playableAsset,
                            name = clip.displayName,
                            duration = clip.duration,
                        });

                        Debug.Log($"Stored PlayableAsset: {clip.displayName}");
                    }
                }

                Debug.Log($"Total PlayableAssets stored: {storedPlayableAssets.Count}");
            }
            else
            {
                Debug.LogError("PlayableAsset is not a TimelineAsset.");
            }
        }
        else
        {
            Debug.LogError("PlayableDirector is not assigned.");
        }
    }

    [ContextMenu("CreateAssetsTimeline")]
    public void CreateAssetsTimeline()
    {
        foreach (var playAsset in storedPlayableAssets)
        {
            GameObject assetPlayable = Instantiate(componPrefab.Get<GameObject>("prefabObject"));
            assetPlayable.transform.SetParent(componPrefab.Get<GameObject>("container").transform, false);

            ComponentLookup compoObject = assetPlayable.GetComponent<ComponentLookup>();
            TextMeshProUGUI textnameAssets = compoObject.Get<TextMeshProUGUI>("clip_title");
            LayoutElement layoutElement = compoObject.Get<LayoutElement>("timeline");

            string nameAssets = playAsset.name;
            textnameAssets.text = nameAssets;

            assetPlayable.gameObject.name = nameAssets;
            assetPlayable.SetActive(true);

            float clipDurationInSeconds = (float)playAsset.duration;
            float scaledDuration = clipDurationInSeconds * timelineScale;

            layoutElement.preferredWidth = scaledDuration;
        }
    }
}

[System.Serializable]
public struct PlayableAssetData
{
    public PlayableAsset playable;
    public string name;
    public double duration;
}

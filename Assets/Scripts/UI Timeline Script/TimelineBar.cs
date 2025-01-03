using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineBar : MonoBehaviour
{
    [Header("ScrollView Components")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("Tick Settings")]
    public GameObject tickPrefab;
    public int initialTickCount = 20;
    public float tickSpacing = 50f;

    [Header("Zoom Settings")]
    public float minSpacing = 10f;
    public float maxSpacing = 100f;

    [Header("Time Label Settings")]
    public GameObject timeLabelPrefab;
    public float timeLabelInterval = 30;

    private float currentSpacing;

    // Object Pooling for ticks and labels
    private List<GameObject> tickPool = new List<GameObject>();
    private List<GameObject> labelPool = new List<GameObject>();

    void Start()
    {
        currentSpacing = tickSpacing;
        InitializeTicks();

        var layoutGroup = content.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }

        content.pivot = new Vector2(0, 0);
        scrollRect.onValueChanged.AddListener(UpdateSpacing);
        UpdateContent();
    }

    void InitializeTicks()
    {
        // Clear the content before initializing new ticks
        ClearContent();

        // Calculate tick count based on the width of the content
        int tickCount = Mathf.CeilToInt(content.rect.width / tickSpacing);

        for (int i = 0; i < tickCount; i++)
        {
            // Reuse tick from pool or instantiate a new one if necessary
            GameObject tick = GetPooledTick();
            tick.SetActive(true);
            tick.transform.SetParent(content);
            tick.transform.localPosition = new Vector2(i * tickSpacing, 0f);

            if (i % timeLabelInterval == 0)
            {
                CreateTimeLabel(i);
            }
        }
    }

    void CreateTimeLabel(int tickIndex)
    {
        GameObject timeLabel = GetPooledLabel();
        timeLabel.SetActive(true);
        timeLabel.transform.SetParent(content);

        // Calculate time in seconds and set the text
        float timeInSeconds = tickIndex * currentSpacing / tickSpacing;
        timeLabel.GetComponent<TextMeshProUGUI>().text = $"{timeInSeconds:F2}s";

        // Set the position of the label above the tick
        timeLabel.transform.localPosition = new Vector2(tickIndex * currentSpacing, 30f);
    }

    void UpdateSpacing(Vector2 scrollPosition)
    {
        currentSpacing = Mathf.Lerp(minSpacing, maxSpacing, scrollPosition.x);
        currentSpacing = Mathf.Clamp(currentSpacing, minSpacing, maxSpacing);

        // After spacing update, adjust ticks and content layout
        UpdateTicks();
        UpdateContent();
    }

    void UpdateTicks()
    {
        // Reuse and reset the ticks in the pool instead of destroying them
        foreach (Transform child in content)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Reinitialize ticks based on the new spacing
        InitializeTicks();
    }

    void UpdateContent()
    {
        float xOffset = 0f;
        foreach (Transform child in content)
        {
            child.localPosition = new Vector2(xOffset, 0f);
            xOffset += currentSpacing;
        }
    }

    // Object Pooling Helpers
    GameObject GetPooledTick()
    {
        GameObject tick = tickPool.Find(t => !t.activeSelf);
        if (tick == null)
        {
            // If no inactive tick found, instantiate a new one
            tick = Instantiate(tickPrefab);
            tickPool.Add(tick);
        }
        return tick;
    }

    GameObject GetPooledLabel()
    {
        GameObject label = labelPool.Find(l => !l.activeSelf);
        if (label == null)
        {
            // If no inactive label found, instantiate a new one
            label = Instantiate(timeLabelPrefab);
            labelPool.Add(label);
        }
        return label;
    }

    void ClearContent()
    {
        // Clear all objects in the content (but without destroying them)
        foreach (Transform child in content)
        {
            child.gameObject.SetActive(false);
        }
    }
}

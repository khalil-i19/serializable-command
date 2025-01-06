using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Innoveam.Modules.Data;

public class TimelineBar : MonoBehaviour
{
    [Header("Component")]
    public ComponentLookup timeCompo;

    [Header("Pengaturan Tick dan Penggaris")]
    public int rulerDuration = 300;
    public int majorTickInterval = 10;
    public int minorTickPerMajor = 5;
    public int labelInterval = 30;

    [Header("Pengaturan Zoom")]
    public float minTickSpacing = 10f;
    public float maxTickSpacing = 100f;
    public float zoomSensitivity = 5f;
    private float currentSpacing;
    private List<GameObject> tickPool = new List<GameObject>();
    private List<GameObject> labelPool = new List<GameObject>();

    void Start()
    {
        currentSpacing = maxTickSpacing / 2;
        GenerateTimeline();
    }

    void Update()
    {
        HandleMouseScrollZoom();
    }

    void HandleMouseScrollZoom()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            currentSpacing = Mathf.Clamp(currentSpacing - scrollDelta * zoomSensitivity, minTickSpacing, maxTickSpacing);

            GenerateTimeline();
        }
    }

    void GenerateTimeline()
    {
        ClearContent();

        int totalTicks = Mathf.CeilToInt((float)rulerDuration * minorTickPerMajor / majorTickInterval);

        for (int i = 0; i <= totalTicks; i++)
        {
            float tickPosition = i * currentSpacing;

            GameObject tick = GetPooledTick();
            tick.transform.SetParent(timeCompo.Get<RectTransform>("time_Content"));
            tick.transform.localScale = Vector3.one;
            tick.GetComponent<RectTransform>().anchoredPosition = new Vector2(tickPosition, 0);
            tick.SetActive(true);

            RectTransform tickRect = tick.GetComponent<RectTransform>();

            if (i % minorTickPerMajor == 0)
            {
                tickRect.sizeDelta = new Vector2(2, 15);

                if (i % (labelInterval / majorTickInterval) == 0)
                {
                    CreateTimeLabel(i, tickPosition);
                }
            }
            else
            {
                tickRect.sizeDelta = new Vector2(1, 10);
            }
        }

        UpdateContentWidth();
    }

    void CreateTimeLabel(int tickIndex, float tickPosition)
    {
        GameObject label = GetPooledLabel();
        label.transform.SetParent(timeCompo.Get<RectTransform>("time_Content"));
        label.transform.localScale = Vector3.one;
        label.GetComponent<RectTransform>().anchoredPosition = new Vector2(tickPosition, -12);

        int timeInSeconds = tickIndex * majorTickInterval;

        label.GetComponent<TextMeshProUGUI>().text = $"{timeInSeconds}s";
        label.SetActive(true);
    }

    void UpdateContentWidth()
    {
        ScrollRect scrollRect = timeCompo.Get<ScrollRect>("time_scroll");
        RectTransform content = timeCompo.Get<RectTransform>("time_Content");
        float totalWidth = content.rect.width;
        content.sizeDelta = new Vector2(totalWidth, content.sizeDelta.y);
        scrollRect.horizontalNormalizedPosition = 0f;
    }

    GameObject GetPooledTick()
    {
        GameObject tick = tickPool.Find(t => !t.activeSelf);
        if (tick == null)
        {
            tick = Instantiate(timeCompo.Get<GameObject>("line_Ruler"));
            tickPool.Add(tick);
        }
        return tick;
    }

    GameObject GetPooledLabel()
    {
        GameObject label = labelPool.Find(l => !l.activeSelf);
        if (label == null)
        {
            label = Instantiate(timeCompo.Get<GameObject>("time_Text"));
            labelPool.Add(label);
        }
        return label;
    }

    void ClearContent()
    {
        foreach (GameObject tick in tickPool)
        {
            tick.SetActive(false);
        }

        foreach (GameObject label in labelPool)
        {
            label.SetActive(false);
        }
    }
}

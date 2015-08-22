﻿using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public RectTransform selectionPanel;
    public UiFollow2D selectedIndicatorPrefab;
    public RectTransform selectedIndicatorParent;
    public RectTransform targetsParent;
    public float selectedIndicatorExpansion = 1.05f;

    [Header("Debug stuff")]
    public bool isSelecting = false;
    public Vector2 selectionStart = Vector2.zero;
    public Vector2 selectionEnd = Vector2.zero;

    public List<Agent> SelectedAgents { get; set; }
    public static SelectionManager Instance { get; set; }

    public void Start()
    {
        SelectedAgents = new List<Agent>();
        Instance = this;
    }

    public void Update()
    {
        Instance = this;

        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            selectionStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (isSelecting)
        {
            selectionEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            isSelecting = false;
            AreaSelected();
        }

        DrawSelection();
    }

    private Rect GetSelectionRect()
    {
        Vector2 min = Vector2.Min(selectionStart, selectionEnd);
        Vector2 max = Vector2.Max(selectionStart, selectionEnd);
        Rect rect = new Rect(min, max - min);
        return rect;
    }

    private void DrawSelection()
    {
        selectionPanel.gameObject.SetActive(isSelecting);
        if (isSelecting)
        {
            Rect rect = GetSelectionRect();
            selectionPanel.position = Camera.main.WorldToScreenPoint(rect.position);
            selectionPanel.sizeDelta = Camera.main.WorldToScreenPoint(rect.size) - Camera.main.WorldToScreenPoint(Vector2.zero);
        }
    }

    private void AreaSelected()
    {
        ClearSelectedAgents();

        Rect rect = GetSelectionRect();
        foreach (Agent agent in Level.Instance.GetAgents())
        {
            if (rect.Contains(agent.Position))
            {
                SelectedAgents.Add(agent);
                UiFollow2D uiFollow2D = (UiFollow2D)Instantiate(selectedIndicatorPrefab, Vector2.zero, Quaternion.identity);
                uiFollow2D.sizeExpansion = selectedIndicatorExpansion;
                uiFollow2D.transform.SetParent(selectedIndicatorParent, true);
                uiFollow2D.follow = agent.transform;
                agent.Selected = true;
            }
        }
    }

    private void ClearSelectedAgents()
    {
        foreach (Agent agent in SelectedAgents)
        {
            if (agent != null)
            {
                agent.Selected = false;
            }
        }
        SelectedAgents.Clear();
        Helpers.DeleteAllChildren(selectedIndicatorParent);
    }
}

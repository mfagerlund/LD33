using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public RectTransform selectionPanel;
    public UiFollow2D selectedIndicatorPrefab;
    public RectTransform selectedIndicatorParent;
    public RectTransform targetsParent;
    public float selectedIndicatorExpansion = 1.05f;
    public float agentClickSize = 0.2f;
    [Header("Debug stuff")]
    public bool isSelecting = false;
    public Vector2 selectionStart = Vector2.zero;
    public Vector2 selectionEnd = Vector2.zero;
    public AudioClip selectedSound;
    public AudioClip tooFar;
    public AudioClip selectSomeone;
    public AudioClip[] hypnotics;

    public List<Agent> SelectedAgents { get; set; }
    public static SelectionManager Instance { get; set; }

    public List<Agent> SelectedControlledAgents { get { return SelectedAgents.Where(a => a.IsPlayerControlled).ToList(); } }

    public void Start()
    {
        SelectedAgents = new List<Agent>();
        Instance = this;
    }

    public void Update()
    {
        // Some will have been removed
        SelectedAgents.RemoveAll(s => s == null);

        if (Input.GetKeyDown(KeyCode.H))
        {
            HypnotizeSelected();
        }

        Instance = this;
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

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

    public void HypnotizeSelected()
    {
        bool potential=false;
        List<Agent> hypnotized = new List<Agent>();
        foreach (Agent agent in SelectedAgents)
        {
            if (agent.agentType == AgentType.PassiveMonster)
            {
                potential = true;
                if (agent.TryToHypnotize())
                {
                    hypnotized.Add(agent);
                }
            }
        }
        if (hypnotized.Any())
        {
            AudioSource.PlayClipAtPoint(hypnotics.RandomItem(), SelectedControlledAgents.First().Position, 0.6f);
            Hypnotizer.Instance.ShowHypnotization(hypnotized);
        }
        else if (potential)
        {
            AudioSource.PlayClipAtPoint(tooFar, Vector2.zero, 0.6f);
        }
        else
        {
            AudioSource.PlayClipAtPoint(selectSomeone, Vector2.zero, 0.6f);
        }
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
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            ClearSelectedAgents();
        }
        Rect rect = GetSelectionRect();
        foreach (Agent agent in Level.Instance.GetAgents())
        {
            if (rect.Contains(agent.Position) || Vector2.Distance(rect.center, agent.Position) < Agent.AgentRadius)
            {
                SelectAgent(agent);
            }
        }
        if (SelectedControlledAgents.Any())
        {
            AudioSource.PlayClipAtPoint(selectedSound, SelectedControlledAgents.First().Position, 1.5f);
        }
    }

    private void SelectAgent(Agent agent)
    {
        SelectedAgents.Add(agent);
        UiFollow2D uiFollow2D = (UiFollow2D)Instantiate(selectedIndicatorPrefab, Vector2.zero, Quaternion.identity);
        uiFollow2D.sizeExpansion = selectedIndicatorExpansion;
        uiFollow2D.transform.SetParent(selectedIndicatorParent, true);
        uiFollow2D.follow = agent.transform;
        agent.Selected = true;
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

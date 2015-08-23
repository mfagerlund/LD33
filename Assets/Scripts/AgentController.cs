using System.Linq;
using Assets.Scripts.Utilities;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    private Vector2 _min;
    private Vector2 _max;
    private Tool[] tools;
    public LocationTarget locationTargetPrefab;
    public AudioClip[] acks;

    public float clickDistance = 10;


    public Tool currentTool;
    public static AgentController Instance { get; set; }

    public void Start()
    {
        Instance = this;
        tools = new[] { new NavigationTool() };
        currentTool = tools[0];
    }

    public void Update()
    {
        Instance = this;
        HandleDragging();
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _min = Input.mousePosition;
            _max = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            _min = Vector2.Min(_min, Input.mousePosition);
            _max = Vector2.Max(_max, Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(1))
        {
            float delta = (_max - _min).magnitude;
            if (delta <= clickDistance)
            {
                if (currentTool != null)
                {
                    PlayAck();
                    currentTool.RightClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
            }
        }
    }

    private void PlayAck()
    {
        if (SelectionManager.Instance.SelectedControlledAgents.Any())
        {
            AudioClip audio = acks.RandomItem();
            if (audio != null)
            {
                AudioSource.PlayClipAtPoint(audio, SelectionManager.Instance.SelectedControlledAgents.First().Position, 0.6f);
            }
        }
    }
}
using UnityEngine;

namespace DisasterPR.Client.Unity;

public delegate bool TickingTaskDelegate();

public class TaskManager : MonoBehaviour
{
    private static TaskManager _instance;
    public static TaskManager Instance => _instance;
    
    private List<TickingTaskDelegate> _tickables = new();

    void Awake()
    {
        _instance = this;
    }
    
    void Update()
    {
        var removals = _tickables.Where(f => f());
        _tickables.RemoveAll(f => removals.Contains(f));
    }

    public void AddTickable(TickingTaskDelegate tickable)
    {
        _tickables.Add(tickable);
    }
}
using UnityEngine;

/// <summary>
/// Minimal job holder for pawns; registers with JobService.
/// </summary>
public class PawnJob : MonoBehaviour
{
    [SerializeField] private JobType current = JobType.None;
    [SerializeField] private Building assignedBy;

    public bool IsIdle => current == JobType.None;
    public JobType Current => current;
    public Building AssignedBy => assignedBy;

    private void OnEnable()
    {
        var js = FindObjectOfType<JobService>();
        if (js != null) js.RegisterPawn(this);
    }

    private void OnDisable()
    {
        var js = FindObjectOfType<JobService>();
        if (js != null) js.UnregisterPawn(this);
    }

    public void SetJob(JobType t, Building by)
    {
        current = t;
        assignedBy = by;
        name = gameObject.name; // keep name stable; could add suffix if desired
    }
}

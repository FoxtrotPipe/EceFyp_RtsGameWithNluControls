using UnityEngine;
using RedBjorn.ProtoTiles;

/// <summary>
/// Handles asynchronous task execution of building structures. Responsible for Unity-side communication and synchronization.
/// </summary>
public class BuildingStructureTaskHandler : TaskHandler
{
    protected Quaternion TargetRotation; // To implement smooth rotation
    protected float RotateSpeed = 5f; // To control speed of smooth rotation

    public Transform RotationNode;
    public Outline HighlightOutline;
    protected BuildingStructure Structure { get; private set; }

    public void Init(BuildingStructure structure, MapEntity map)
    {   
        base.Init(map);
        Structure = structure;
    }

    public override void Update()
    {
        if (RotationNode != null && RotationNode.rotation != TargetRotation)
        {
            RotationNode.rotation = Quaternion.Lerp(RotationNode.rotation, TargetRotation, RotateSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Handle despawn and destruction of unit
    /// </summary>
    public override void HandleDespawn(GameObject spawnedObject)
    {
        if (TaskCoroutine != null) StopCoroutine(TaskCoroutine);
        CurrentTask = "despawn";

        TaskCoroutine = StartCoroutine(Despawning(spawnedObject, 10, true));
    }
}
using System.Collections;
using RedBjorn.ProtoTiles;
using UnityEngine;

/// <summary>
/// Handles asynchronous task execution of building structures. Responsible for Unity-side communication and synchronization.
/// </summary>
abstract public class TaskHandler : MonoBehaviour
{
    protected MapEntity Map { get; private set;}
    protected Coroutine TaskCoroutine;
    protected string CurrentTask = "idle";

    public void Init(MapEntity map)
    {   
        Map = map;
    }

    public abstract void Update();

    protected IEnumerator Despawning(GameObject gObject, float waitTime, bool sinkToGround = false)
    {
        float elapsed = 0f;
        
        while (elapsed < waitTime)
        {
            yield return null;
            elapsed += Time.deltaTime;

            if (sinkToGround)
            {
                gObject.transform.position = Vector3.MoveTowards(gObject.transform.position, gObject.transform.position + new Vector3(0, -10, 0), (float)(0.25 * Time.deltaTime));
            }
        }

        Destroy(gObject);
    }

    /// <summary>
    /// Handle despawn and destruction of unit
    /// </summary>
    public virtual void HandleDespawn(GameObject spawnedObject)
    {
        if (TaskCoroutine != null) StopCoroutine(TaskCoroutine);
        CurrentTask = "despawn";

        TaskCoroutine = StartCoroutine(Despawning(spawnedObject, 2));
    }
}
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles asynchronous task execution of unit. Responsible for reference to unit components, Unity-side communication and synchronization of unit.
/// </summary>
public class ResourceTaskHandler : MonoBehaviour
{
    private Coroutine _taskCoroutine;

    private IEnumerator Despawning(GameObject gObject, float waitTime)
    {
        float elapsed = 0f;

        while (elapsed < waitTime)
        {
            yield return null;
            elapsed += Time.deltaTime;
            gObject.transform.position = Vector3.MoveTowards(gObject.transform.position, gObject.transform.position + new Vector3(0, -10, 0), (float)(0.25 * Time.deltaTime));
        }

        Destroy(gObject);
    }

    /// <summary>
    /// Handle despawn and destruction of unit
    /// </summary>
    public void HandleDespawn(GameObject spawnedObject)
    {
        if (_taskCoroutine != null) StopCoroutine(_taskCoroutine);

        _taskCoroutine = StartCoroutine(Despawning(spawnedObject, 10));
    }
}
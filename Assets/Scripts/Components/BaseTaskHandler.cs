using System;
using System.Collections;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

public class BaseTaskHandler : BuildingStructureTaskHandler
{   
    private Base Base;

    public void Init(Base baseBuilding, MapEntity map)
    {   
        base.Init(baseBuilding, map);
        Base = baseBuilding;
    }

    private IEnumerator Spawning(float healRate, Action onSpawn)
    {
        var elapsed = 0f;

        while (Base.Alive)
        {
            if (elapsed >= healRate)
            {
                onSpawn.SafeInvoke();
                elapsed = 0f;
            }
            elapsed += Time.deltaTime;

            yield return null;
        }
    }

    public void HandleSpawning(float spawnRate, Action onSpawn = null)
    {
        if (CurrentTask != "despawn" && TaskCoroutine != null) StopCoroutine(TaskCoroutine);
        CurrentTask = "spawning";

        TaskCoroutine = StartCoroutine(Spawning(spawnRate, onSpawn));
    }
}
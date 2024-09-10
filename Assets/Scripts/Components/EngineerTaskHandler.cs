using System;
using System.Collections;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

/// <summary>
/// Handles asynchronous task execution of unit. Responsible for reference to unit components, Unity-side communication and synchronization of unit.
/// </summary>
public class EngineerTaskHandler : UnitTaskHandler
{
    private Engineer Engineer;

    public void Init(Engineer engineer, MapEntity map)
    {
        base.Init(engineer, map);
        Engineer = engineer;
    }

    private IEnumerator Building(BuildingStructure building, float buildRate, Action onBuildProgress, Action onBuildEnd)
    {
        var elapsed = 0f;
        var lookDir = building.Center - transform.position;
        TargetRotation = Quaternion.LookRotation(lookDir, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
        Engineer.Animate("TriggerConstruct"); 
        while (building.Alive && !building.Complete)
        {
            if (elapsed >= buildRate)
            {
                onBuildProgress.SafeInvoke();;
                elapsed = 0;
            }  
            elapsed += Time.deltaTime;

            yield return null;
        }
        Engineer.Animate("TriggerIdle");
        onBuildEnd.SafeInvoke();
    }

    private IEnumerator Harvesting(ResourceStructure resource, float rate, Action onHarvestProgress, Action onHarvestEnd)
    {
        var elapsed = 0f;
        var lookDir = resource.Center - transform.position;
        TargetRotation = Quaternion.LookRotation(lookDir, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
        Engineer.Animate("TriggerConstruct"); 
        while (!resource.Empty)
        {
            if (elapsed >= rate)
            {
                onHarvestProgress.SafeInvoke();
                elapsed = 0;
            }
            elapsed += Time.deltaTime;

            yield return null;
        }
        Engineer.Animate("TriggerIdle");
        onHarvestEnd.SafeInvoke();
    }

    public void HandleBuild(BuildingStructure building, float buildRate, Action onBuildProgress = null, Action onBuildEnd = null)
    {
        if (building != null)
        {
            if (CurrentTask != "despawn" && TaskCoroutine != null) StopCoroutine(TaskCoroutine);
            CurrentTask = "build";
            TaskCoroutine = StartCoroutine(Building(building, buildRate, onBuildProgress, () => {
                onBuildEnd.SafeInvoke();
                CurrentTask = "idle";
            }));
        }
        else
        {
            onBuildEnd.SafeInvoke();
        }
    }

    public void HandleHarvest(ResourceStructure resource, float harvestRate, Action onHarvestProgress = null, Action onHarvestEnd = null)
    {
        if (resource != null)
        {
            if (CurrentTask != "despawn" && TaskCoroutine != null) StopCoroutine(TaskCoroutine);
            CurrentTask = "build";
            TaskCoroutine = StartCoroutine(Harvesting(resource, harvestRate, onHarvestProgress, () => {
                onHarvestEnd.SafeInvoke();
                CurrentTask = "idle";
            }));
        }
        else
        {
            onHarvestEnd.SafeInvoke();
        }
    }
}


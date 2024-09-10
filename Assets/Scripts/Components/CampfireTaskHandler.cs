using System;
using System.Collections;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

public class CampfireTaskHandler : BuildingStructureTaskHandler
{   
    private Campfire Campfire;

    public void Init(Campfire campfire, MapEntity map)
    {   
        base.Init(campfire, map);
        Campfire = campfire;
    }

    private IEnumerator Healing(float healRate, Action onHeal)
    {
        var elapsed = 0f;

        while (Campfire.Alive)
        {
            if (elapsed >= healRate)
            {
                onHeal.SafeInvoke();
                elapsed = 0f;
            }
            elapsed += Time.deltaTime;

            yield return null;
        }
    }

    public void HandleHeal(float healRate, Action onHeal = null)
    {
        if (CurrentTask != "despawn" && TaskCoroutine != null) StopCoroutine(TaskCoroutine);
        CurrentTask = "heal";

        TaskCoroutine = StartCoroutine(Healing(healRate, onHeal));
    }
}
using System;
using System.Collections;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

public class CannonTaskHandler : BuildingStructureTaskHandler
{   
    private Cannon Cannon;

    public void Init(Cannon cannon, MapEntity map)
    {   
        base.Init(cannon, map);
        Cannon = cannon;
    }

    public override void Update()
    {
        base.Update();

        if (CurrentTask == "idle")
        {
            var o = Cannon.NearestHostile;

            if (o != null)
            {
                Cannon.Attack(o);
            }
        }
    }

    private IEnumerator Attacking(TileOccupant target, float attackRate, Action<TileOccupant> onHit, Action onEngageEnd)
    {
        var elapsed = 0f;

        while (target.Alive)
        {
            TargetRotation = Quaternion.LookRotation(target.Center - transform.position, Vector3.up) * Quaternion.Euler(0f, 0f, 0f);

            if (elapsed >= attackRate)
            {
                onHit.SafeInvoke(target);
                elapsed = 0;
            }
            elapsed += Time.deltaTime;

            yield return null;
        }
        onEngageEnd.SafeInvoke();
    }

    public void HandleEngageBattle(TileOccupant target, float attackRate, Action<TileOccupant> onHit = null, Action onEngageEnd = null)
    {
        if (target != null)
        {
            if (CurrentTask != "despawn" && TaskCoroutine != null) StopCoroutine(TaskCoroutine);
            CurrentTask = "attack";
            TaskCoroutine = StartCoroutine(Attacking(target, attackRate, onHit, () => {
                onEngageEnd.SafeInvoke();
                CurrentTask = "idle";
            }));
        }
        else
        {
            onEngageEnd.SafeInvoke();
        }
    }
}
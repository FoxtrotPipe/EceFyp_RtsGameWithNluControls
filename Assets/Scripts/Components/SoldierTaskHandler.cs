using System;
using System.Collections;
using System.Linq;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

/// <summary>
/// Handles asynchronous task execution of unit. Responsible for reference to unit components, Unity-side communication and synchronization of unit.
/// </summary>
public class SoldierTaskHandler : UnitTaskHandler
{
    private Soldier Soldier;

    public void Init(Soldier soldier, MapEntity map)
    {
        base.Init(soldier, map);
        Soldier = soldier;
    }

    public override void Update()
    {
        base.Update();

        if (CurrentTask == "idle")
        {
            var o = Soldier.NearestHostile;

            if (o != null)
            {
                Soldier.Attack(o);
            }
        }
    }

    private IEnumerator Attacking(TileOccupant target, float startAttackDelay, float attackRate, Action<TileOccupant> onHit, Action onEngageEnd)
    {
        // Standing on vacant tile. Then start attack
        var elapsed = 0f;
        TargetRotation = Quaternion.LookRotation(target.Center - transform.position, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
        Soldier.Animate("TriggerLiftGun");
        yield return new WaitForSeconds(startAttackDelay);
        while (target.Alive && Soldier.WithinRange(target))
        {
            TargetRotation = Quaternion.LookRotation(target.Center - transform.position, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);

            if (elapsed >= attackRate)
            {
                onHit.SafeInvoke(target);
                elapsed = 0;
            }
            elapsed += Time.deltaTime;
            
            yield return null;
        }
        Soldier.Animate("TriggerIdle");
        onEngageEnd.SafeInvoke();
    }

    public void HandleEngageBattle(TileOccupant target, float startAttackDelay, float attackRate, Action<TileOccupant> onHit = null, Action onEngageEnd = null)
    {
        // if (CurrentTask == "make-spot") return;

        if (target != null)
        {
            if (CurrentTask != "despawn" && TaskCoroutine != null) 
            {
                StopCoroutine(TaskCoroutine);
            }
            
            CurrentTask = "attack";
            // if (Soldier.Tile.Occupant != Soldier)
            // {
            //     Debug.Log("Standing on tile occupied by others. Navigating to nearest vacant tile" + Soldier.NearestVacantTile);
            //     HandleMakeSpot(null, () => {
            //         HandleEngageBattle(target, startAttackDelay, attackRate, onHit, onEngageEnd);
            //         Debug.Log("Finish spot making now attack");
            //         CurrentTask = "attack";
            //     });
            // }
            // else
            // {
            //     TaskCoroutine = StartCoroutine(Attacking(target, startAttackDelay, attackRate, onHit, () => {
            //         onEngageEnd.SafeInvoke();
            //         CurrentTask = "idle";
            //     }));
            // }
            TaskCoroutine = StartCoroutine(Attacking(target, startAttackDelay, attackRate, onHit, () => {
                onEngageEnd.SafeInvoke();
                CurrentTask = "idle";
            }));
        }
        else
        {
            onEngageEnd.SafeInvoke();
        }
    }

    // public void HandleMakeSpot(Func<TileEntity, bool> onCompleteStep = null, Action onCompletePath = null)
    // {
    //     var tile = Soldier.NearestVacantTile;

    //     if (tile != null)
    //     {
    //         var path = Map.PathTiles(transform.position, Map.WorldPosition(tile), _walkRange, Soldier.Group.Occupants);
    //         Debug.Log("Make spot path: from " + path.First() + " to " + path.Last());

    //         if (path != null && path.Any())
    //         {
    //             Debug.Log("Valid path!");
    //             if (CurrentTask != "despawn" && TaskCoroutine != null) 
    //             {
    //                 StopCoroutine(TaskCoroutine);
    //             }
    //             CurrentTask = "make-spot";
    //             TaskCoroutine = StartCoroutine(Moving(path, onCompleteStep, () => {
    //                 onCompletePath.SafeInvoke();
    //                 CurrentTask = "idle";
    //             }));
    //         }
    //         else
    //         {
    //             Debug.Log("Invalid path!");
    //             onCompletePath.SafeInvoke();
    //         }
    //     }
    // }
}
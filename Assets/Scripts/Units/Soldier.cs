using System;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

public class Soldier : Unit
{
    private float _damage = 3f;
    private float _weaponDrawTime = 1f;      // This value is determined by lift gun animation of soldier
    private float _weaponShootTime = 1.833f; // This value is determined by shoot animation of soldier

    protected override GameObject SpawnedObject 
    { 
        get => base.SpawnedObject;
        set
        {
            base.SpawnedObject = value;
            SoldierTaskHandler = SpawnedObject.GetComponent<SoldierTaskHandler>();
            SoldierTaskHandler.Init(this, _tileManager.Map);
            UnitTaskHandler = SoldierTaskHandler;
        }
    }

    private SoldierTaskHandler SoldierTaskHandler { get; set; }

    public Soldier(TileEntity tile, Party party = Party.Player) : base(6f * 6, 2, tile, party) 
    {
        SpawnedObject = GameObject.Instantiate(_unitManager.GetPrefab(UnitType.Soldier, party));
    }

    // Move to a specific tile
    public override void MoveTo(TileEntity targetTile, Action onMoveToEnd = null, bool ignoreGroupMembers = true)
    {
        SoldierTaskHandler.HandleMoveToTile 
        (
            targetTile, 
            ignoreGroupMembers ? Group.Occupants : null,
            (TileEntity nextTile) => 
            {
                Tile = nextTile;

                var o = NearestHostile;

                if (o != null) 
                {
                    Attack(o);
                    return true;
                }
                else 
                {
                    return false;
                }
            }, 
            () => 
            {
                onMoveToEnd.SafeInvoke();
            }
        );
    }

    // public void MakeSpot(Action onMoveToEnd = null)
    // {
    //     Animate("TriggerWalk");
    //     SoldierTaskHandler.HandleMakeSpot
    //     (
    //         (TileEntity nextTile) => 
    //         {
    //             Debug.Log("Walked step");
    //             Tile = nextTile;
    //             return false;   
    //         },
    //         () =>
    //         {
    //             Animate("TriggerIdle");
    //             onMoveToEnd.SafeInvoke();
    //         }
    //     );
    // }

    // Perform attack order
    public void Attack(TileOccupant target) 
    {
        // if (Tile.Occupant != this)
        // {
        //     Debug.Log("Current tile occupant isn't itself. Attempt to make spot.");
        //     MakeSpot(() => Attack(target));
        // }
        // else
        // {
        //     Debug.Log("Finish spot making. Start attack");
        //     Animate("TriggerLiftGun");
        //     SoldierTaskHandler.HandleEngageBattle
        //     (
        //         target, _weaponDrawTime, _weaponShootTime, 
        //         (TileOccupant target) => target.Health -= _damage, 
        //         () => Animate("TriggerIdle")
        //     );
        // }
        SoldierTaskHandler.HandleEngageBattle
        (
            target, _weaponDrawTime, _weaponShootTime, 
            (TileOccupant target) => target.Health -= _damage, 
            () => {}
        );
    }
}

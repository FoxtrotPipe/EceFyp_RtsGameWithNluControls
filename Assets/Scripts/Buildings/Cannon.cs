using System.Collections.Generic;
using RedBjorn.ProtoTiles;
using UnityEngine;

public class Cannon : BuildingStructure
{
    public override float BuildProgress 
    { 
        get { return base.BuildProgress; }
        set
        {
            base.BuildProgress = value; 
            if (Complete)
            { 
                SpawnedObject = GameObject.Instantiate(_buildingManager.GetPrefab(BuildingType.Cannon, Party));
                // TODO: Implement event of Cannon upon construction completion
            }
        }
    }
    private float _damage = 8f;      // This value is preset
    private float _attackRate = 5f;  // This value is preset

    protected override GameObject SpawnedObject 
    { 
        get { return base.SpawnedObject; }
        set
        {
            base.SpawnedObject = value;
            CannonTaskHandler = SpawnedObject.GetComponent<CannonTaskHandler>();
            CannonTaskHandler.Init(this, _tileManager.Map);
            StructureTaskHandler = CannonTaskHandler;
        }
    }

    private CannonTaskHandler CannonTaskHandler { get; set; }
    
    public Cannon(List<TileEntity> tileSet, int direction, Party party = Party.Player) : base(6f * 10, 10, 5, tileSet, direction, party) {}

    /// <summary>
    /// Perform attack order
    /// </summary>
    /// <param name="target"></param>
    public void Attack(TileOccupant target) 
    { 
        // Animate("TriggerIdle");
        CannonTaskHandler.HandleEngageBattle
        (
            target, 
            _attackRate, 
            (TileOccupant target) =>
            {
                // Damage every enemies in radius (length 1)
                foreach(var t in _tileManager.Area(target.Tile, 1))
                {
                    var o = t.Occupant;

                    if (o != null && IsHostile(o))
                    {
                        o.Health -= _damage;
                        // Debug.Log("[Cannon]: deal " + _damage + " damage point to party unit at " + _tileManager.GetAlias(target.Tile));
                    }
                }
            }, 
            () =>
            {
                // Debug.Log("Battle engagement complete");
            }
        );
    }
}
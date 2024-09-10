using System.Collections.Generic;
using RedBjorn.ProtoTiles;
using UnityEngine;

public class Campfire : BuildingStructure
{
    public override float BuildProgress 
    { 
        get => base.BuildProgress;
        set
        {
            base.BuildProgress = value; 
            if (Complete) 
            {
                SpawnedObject = GameObject.Instantiate(_buildingManager.GetPrefab(BuildingType.Campfire, Party));
                CampfireTaskHandler.HandleHeal(_healRate, HealAllFriendliesInRange);
            }
        }
    }

    protected override GameObject SpawnedObject 
    { 
        get => base.SpawnedObject;
        set
        {
            base.SpawnedObject = value;
            CampfireTaskHandler = SpawnedObject.GetComponent<CampfireTaskHandler>();
            CampfireTaskHandler.Init(this, _tileManager.Map);
            StructureTaskHandler = CampfireTaskHandler;
        }
    }

    private float _healAmount = 1f; // This value is preset
    private float _healRate = 1f;   // This value is preset

    private CampfireTaskHandler CampfireTaskHandler { get; set; }

    public Campfire(List<TileEntity> tileSet, int direction, Party party = Party.Player) : base(6f * 8, 10, 5, tileSet, direction, party) {}

    /// <summary>
    /// Heal all friendly units within detection range
    /// </summary>
    /// <param name="target"></param>
    public void HealAllFriendliesInRange() 
    {
        foreach(TileOccupant o in DetectableFriendlies)
        {
            if (o is Unit unit)
            {
                unit.Health += _healAmount;

                if (unit.Health < unit.MaxHealth) unit.TriggerHealEffect();
            }
        }
    }
}
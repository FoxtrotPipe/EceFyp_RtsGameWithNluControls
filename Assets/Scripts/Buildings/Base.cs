using System.Collections.Generic;
using RedBjorn.ProtoTiles;
using Unity.VisualScripting;
using UnityEngine;

public class Base : BuildingStructure
{
    public override float Health 
    { 
        get => base.Health;
        set 
        {
            base.Health = value; 
            UIManager.instance.UpdateBaseHealth(Health, Party);
            if (!Alive)
            {
                GameManager.instance.EndGame(Party != Party.Player);
            }
        }
    }

    protected override GameObject SpawnedObject 
    { 
        get => base.SpawnedObject;
        set
        {
            base.SpawnedObject = value;
            BaseTaskHandler = SpawnedObject.GetComponent<BaseTaskHandler>();
            BaseTaskHandler.Init(this, _tileManager.Map);
            StructureTaskHandler = BaseTaskHandler;
        }
    }

    [SerializeField] private float _spawnRate = 12f;
    private BaseTaskHandler BaseTaskHandler { get; set; }

    public Base(List<TileEntity> tileSet, int direction, Party party = Party.Player) : base(6f * 20, 0, 0, tileSet, direction, party, true)
    {
        SpawnedObject = GameObject.Instantiate(_buildingManager.GetPrefab(BuildingType.Base, party));
        BaseTaskHandler.HandleSpawning(_spawnRate, SpawnUnitToNonfullGroup);
    }

    public void SpawnUnit(UnitType type, UnitGroup mergeToGroup)
    {
        Unit unit = null;
        var t = _tileManager.VacantAreaFixedSize(Tile, 1)[0];

        if (t != null)
        {
            if (ResourceManager.instance.ChargeForUnit(Party))
            {
                switch (type)
                {
                    case UnitType.Soldier:
                        unit = new Soldier(t, Party);
                        break;
                    case UnitType.Engineer:
                        unit = new Engineer(t, Party);
                        break;
                }

                mergeToGroup.Assign(unit);
                mergeToGroup.Rally(mergeToGroup.RallyPoint);
            }
            else
            {
                Debug.LogWarning("[Base]: SpawnUnit terminated due to insufficient resource (party: " + Party + ")");
            }
        }
        else
        {
            Debug.LogWarning("[Base]: Attempt to spawn unit when there is no vacant spot to spawn unit (party: " + Party + ")");
        }
    }

    public void SpawnUnitToNonfullGroup()
    {
        var gp = UnitGroupManager.instance.GetFirstNonfullGroup(Party);

        if (gp != null)
        {
            SpawnUnit(gp.UnitType, gp);
        }
        else
        {
            // Debug.LogWarning("[Base]: Attempt to spawn unit when all unit groups are full (party: " + Party + ")");
        }
    }
}
using System.Collections.Generic;
using RedBjorn.ProtoTiles;
using UnityEngine;

public class Engineer : Unit
{
    private float _buildAmount = 1f;
    private float _harvestAmount = 1f;
    private float _workRate = 2f; // This value is determined by construct animation of engineer

    protected override GameObject SpawnedObject 
    { 
        get => base.SpawnedObject;
        set
        {
            base.SpawnedObject = value;
            EngineerTaskHandler = SpawnedObject.GetComponent<EngineerTaskHandler>();
            EngineerTaskHandler.Init(this, _tileManager.Map);
            UnitTaskHandler = EngineerTaskHandler;
        }
    }

    public List<ResourceStructure> DetectableResources
    {
        get
        {
            List<ResourceStructure> result = new();

            foreach (var o in DetectableOccupants)
            {
                if (o is ResourceStructure resource)
                {
                    result.Add(resource);
                }
            }

            return result;
        }
    }

    private EngineerTaskHandler EngineerTaskHandler { get; set; }

    public Engineer(TileEntity tile, Party party = Party.Player) : base(6f * 5, 2, tile, party) 
    {
        SpawnedObject = GameObject.Instantiate(_unitManager.GetPrefab(UnitType.Engineer, party));
    }

    public void LayBlueprint(List<TileEntity> tileSet, int dir, BuildingType buildingType)
    {
        if (ResourceManager.instance.ChargeForBuilding(Party))
        {
            // Only allow engineer to build either campfire or Cannon
            switch (buildingType)
            {
                case BuildingType.Campfire:
                    Build(new Campfire(tileSet, dir, Party));
                    break;
                case BuildingType.Cannon:
                    Build(new Cannon(tileSet, dir, Party));
                    break;
            }

            Debug.Log("[Engineer]: Deploy " + buildingType + " building plan on " + _tileManager.ToString(tileSet));
        }
        else
        {
            Debug.LogWarning("[Engineer]: lay blueprint terminated due to insufficient resource (party: " + Party + ")");
        }
    }

    public void Build(BuildingStructure building) 
    {
        EngineerTaskHandler.HandleBuild(building, _workRate,
            () => {
                building.BuildProgress += _buildAmount;
                // Debug.Log("[Engineer]: contribute " + _buildAmount + " build progress to building at " + _tileManager.GetAlias(building.Tile));
            },
            () => {
                // Debug.Log("Construction complete");
            }
        );
    }
    
    public void Harvest(ResourceStructure resource)
    {
        EngineerTaskHandler.HandleHarvest(resource, _workRate, 
            () => {
                resource.HarvestBy(Party, _harvestAmount);
            },
            () => {
                // Debug.Log("Harvest complete");

                var resources = DetectableResources;
                if (resources.Count > 0)
                {
                    var t = _tileManager.VacantAreaFixedSize(resources[0].Tile, 1, new List<TileEntity>(){ resources[0].Tile }, Group.Occupants)[0];

                    MoveTo(t, () => Harvest(resources[0]));
                }
            }
        );
    }
}

using System.Collections.Generic;
using System.Threading;
using RedBjorn.ProtoTiles;
using UnityEngine;

abstract public class BuildingStructure : TileOccupant
{
    static protected BuildingManager _buildingManager = BuildingManager.instance;

    public override float Health 
    { 
        get => base.Health;
        set 
        {
            base.Health = value; 
            if (!Alive)
            {
                Occupied = null;
                StructureTaskHandler.HandleDespawn(SpawnedObject);
            }
        }
    }
    private float _buildProgress;
    public virtual float BuildProgress
    {
        get { return _buildProgress; }
        set
        {
            if (Alive)
            {
                Interlocked.Exchange(ref _buildProgress, (value < 0) ? 0 : (value > _maxBuildProgress) ? _maxBuildProgress : value);
            }
            // Apply build progress effect
        }
    }
    private float _maxBuildProgress;
    public bool Complete { get => _buildProgress == _maxBuildProgress; }
    protected int _range;
    private int _direction;
    
    protected override GameObject SpawnedObject 
    { 
        get { return base.SpawnedObject; }
        set
        {
            base.SpawnedObject = value;
            SpawnedObject.GetComponent<Transform>().rotation = Quaternion.Euler(0, 60 * _direction, 0);
        }
    }
    protected BuildingStructureTaskHandler StructureTaskHandler { get; set; }

    // Assign a random tile from tileset to "Tile" member in TileOccupant base class
    public BuildingStructure(float maxHealth, float maxBuildProgress, int range, List<TileEntity> tileSet, int direction, Party party, bool preBuilt = false) : base(maxHealth, maxHealth, range, tileSet, party)
    {
        _maxBuildProgress = maxBuildProgress;
        _range = range;
        Occupied = tileSet;
        _direction = direction;

        if (!preBuilt)
        {
            SpawnedObject = GameObject.Instantiate(_buildingManager.GetPrefab(BuildingType.ConstructionSite, party));
        }
    }

    public void UpdateSelectEffect(bool enabled)
    {
        StructureTaskHandler.HighlightOutline.enabled = enabled;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using UnityEngine;

public class UnitGroup
{
    public string Alias { get; private set; }
    public Party Party { get; private set; }
    public UnitType UnitType { get; private set; }
    public List<Unit> Units { get; private set; } = new();
    public List<TileOccupant> Occupants { get => Units.Cast<TileOccupant>().ToList(); }
    public List<TileEntity> Occupied { get => Occupants.Select(o => o.Tile).ToList(); }
    public int Size { get => Units.Count; }
    public int MaxSize { get; private set; }
    public bool Full { get => Size >= MaxSize; }
    public bool Selected { get => _manager.SelectedGroups.Contains(this); }
    public TileEntity RallyPoint;

    private TileEntity _defaultRallyPoint;
    private UnitGroupManager _manager = UnitGroupManager.instance;
    private TileManager _tileManager = TileManager.instance;
    
    public UnitGroup(string alias, TileEntity rallyPoint, int size, int maxSize, UnitType unitType, Party party = Party.Player)
    {
        this.Alias = alias;
        this.Party = party;
        this.UnitType = unitType;

        if (size > 0)
        {
            var tileSet = _tileManager.VacantAreaFixedSize(rallyPoint, size);
            foreach (var t in tileSet)
            {
                switch (unitType)
                {
                    case UnitType.Soldier:
                        Assign(new Soldier(t, party));
                        break;
                    case UnitType.Engineer:
                        Assign(new Engineer(t, party));
                        break;
                }
            }
        }
        
        MaxSize = maxSize;  
        _defaultRallyPoint = rallyPoint;
        RallyPoint = rallyPoint;
        _manager.Register(alias, this);
    }

    public void Assign(Unit unit) 
    {
        Units.Add(unit);
        unit.Group = this;

        // Cpu unit group auto target player's base
        if (Party == Party.CPU && Full)
        {
            RallyPoint = _tileManager.Map.BaseTile;
        }

        _manager.RenderSelectedUnits();
    }

    public void Unassign(Unit unit) 
    {
        if (Units.Remove(unit)) 
        {
            if (unit.Group.Size == 1)
            {
                _manager.DeselectGroup(unit.Group);
            }

            unit.Group = null;

            if (!Units.Any())
            {
                RallyPoint = _defaultRallyPoint;
            }

            _manager.RenderSelectedUnits();
        }
    }

    public void Rally(TileEntity target, List<TileEntity> dontAllowTiles = null, Action<Unit> onUnitMoveToEnd = null)
    {
        List<TileEntity> tileSet = _tileManager.RequestUnitTiles(this, target, dontAllowTiles);

        if (tileSet.Count > 0)
        {
            RallyPoint = target;

            for (var index = 0; index < tileSet.Count; index++)
            {
                var unit = Units[index];
                var tile = tileSet[index];   
                
                unit.MoveTo(tile, () => onUnitMoveToEnd.SafeInvoke(unit));
            }
        }
        else
        {
            Debug.LogWarning("[UnitGroup]: Cannot satisfy requested tiles for unit group " + Alias);
        }
    }

    public void Rally(List<TileEntity> tileSet, Action<Unit> onUnitMoveToEnd = null)
    {
        if (tileSet.Count > 0)
        {
            RallyPoint = tileSet[0];

            for (var index = 0; index < tileSet.Count; index++)
            {
                var unit = Units[index];
                var tile = tileSet[index];   
                
                unit.MoveTo(tile, () => onUnitMoveToEnd.SafeInvoke(unit));
            }
        }
        else
        {
            Debug.LogWarning("[UnitGroup]: Cannot satisfy requested tiles for unit group " + Alias);
        }
    }

    public void OrganiseToBuild(TileEntity target, BuildingType buildingType)
    {
        if (UnitType != UnitType.Engineer) 
        {
            Debug.LogWarning("[UnitGroup]: Attempt to call OrganiseToBuild on non-engineer unit group");
            return;
        }
        if (buildingType == BuildingType.Base) 
        {
            Debug.LogWarning("[UnitGroup]: Attempt to call OrganiseToBuild to build base, which is not allowed");
            return;
        }

        if (_tileManager.IsVacantOrIgnored(target, Occupants))
        {
            // Debug.Log("Pre-check that anchor tile " + target + " is vacant");
            var (dir, buildingTileSet) = _tileManager.RequestBuildingTiles(this, buildingType, target);

            if (buildingTileSet.Any())
            {
                Rally(target, buildingTileSet, 
                    (Unit unit) => 
                    {
                        // If the tiles are still vacant when unit reach the destination, perform the building order, otherwise retry to see if there is a vacant triangle
                        if (_tileManager.IsVacantOrIgnored(buildingTileSet, Occupants))
                        {
                            ((Engineer)unit)?.LayBlueprint(buildingTileSet, dir, buildingType);
                        }
                        else
                        {
                            // Debug.Log("Post-check that requested building tiles " + _tileManager.ToString(buildingTileSet) + " is non-vacant");
                            OrganiseToBuild(target, buildingType);
                        }
                    }
                );
            }
            else
            {
                Debug.LogWarning("[UnitGroup]: Requested tiles of " + buildingType + " cannot be satisfied");
            }
        }
        else
        {
            if (target.Occupant is BuildingStructure structure)
            {
                if (structure is Campfire && buildingType == BuildingType.Campfire || 
                    structure is Cannon && buildingType == BuildingType.Cannon)
                {
                    Rally(target, structure.Occupied, 
                        (Unit unit) => 
                        {
                            if (_tileManager.IsVacantOrIgnored(target, Occupants))
                            {
                                OrganiseToBuild(target, buildingType);
                            }
                            else
                            {
                                ((Engineer)unit)?.Build(structure);
                            }
                        }
                    );
                }
                else
                {
                    Debug.LogWarning("[UnitGroup]: Plan to build " + buildingType + " on tile " + target.Position + ", but is occupied with " + (target.Occupant is Campfire ? BuildingType.Campfire : BuildingType.Cannon));
                }
            }
            else
            {
                Debug.LogWarning("[UnitGroup]: Attempt to build on non-vacant tile " + target.Position);
            }
        }
    }

    // Need to change implementation to use IsVacantOrIgnored...
    public void OrganiseToHarvest(TileEntity target)
    {
        if (UnitType != UnitType.Engineer) 
        {
            Debug.LogWarning("[UnitGroup]: Attempt to call OrganiseToHarvest on non-engineer unit group");
            return;
        }

        if (target.Vacant)
        {
            Debug.LogWarning("[UnitGroup]: Attempt to call OrganiseToHarvest on vacant tile" + target.Position);
        }
        else
        {
            if (target.Occupant is ResourceStructure resource)
            {
                Rally
                (
                    target, 
                    new List<TileEntity>{ target }, 
                    (Unit unit) => ((Engineer)unit).Harvest(resource)
                );
            }
            else
            {
                Debug.LogWarning("[UnitGroup]: Attempt to harvest at non-resource type tile " + target.Position);
            }
        }
    }

    public void UpdateSelectEffect(bool enabled) 
    { 
        foreach(Unit unit in Units) unit.UpdateSelectEffect(enabled); 
    }

    public void UpdateLabelEffect(string alias) 
    { 
        foreach(Unit unit in Units) unit.UpdateLabelEffect(alias); 
    }
}
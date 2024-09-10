using System.Collections.Generic;
using System.Linq;
using RedBjorn.ProtoTiles;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instance { get; private set; }

    // Properties belong to ProtoTiles plugin
    public MapSettings MapSettings;
    // public KeyCode gridToggle = KeyCode.G;
    public MapView MapView;
    public MapEntity Map { get; private set; }
    public Transform Level;

    public Dictionary<string, TileEntity> TileDictionary { get; private set; } = new();

    /// <summary>
    /// Convert tile position to world position
    /// </summary>
    /// <param name="tilePos"></param>
    /// <returns></returns>
    public Vector3 ToWorldPosition(Vector3Int tilePos) 
    { 
        return Map.WorldPosition(tilePos); 
    }

    /// <summary>
    /// Get distance between two tiles
    /// </summary>
    /// <param name="gridA"></param>
    /// <param name="gridB"></param>
    /// <returns></returns>
    public float Distance(TileEntity tileA, TileEntity tileB) 
    {
        return Map.Distance(tileA, tileB); 
    }

    /// <summary>
    /// Get shortest distance between a tile and a set of tiles
    /// </summary>
    /// <param name="fromTile"></param>
    /// <param name="toTileSet"></param>
    /// <returns></returns>
    public float Distance(TileEntity fromTile, List<TileEntity> toTileSet) 
    {
        float minDist = -1;

        foreach (var t in toTileSet)
        {
            float dist = Map.Distance(fromTile, t);

            if (minDist == -1 || dist < minDist)
            {
                minDist = dist;
            }
        }

        return minDist; 
    }

    /// <summary>
    /// Get a cluster area of tiles
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<TileEntity> Area(TileEntity origin, float range)
    {
        return NodePathFinder.Area(Map, origin, range).Cast<TileEntity>().ToList();
    }

    public List<TileEntity> Area(List<TileEntity> tileSet, float range)
    {
        List<TileEntity> set = new();

        foreach(var t in tileSet)
        {
            set = set.Union(Area(t, range)).ToList();
        }

        return set;
    }

    public List<TileEntity> AreaFixedSize(TileEntity origin, int size)
    {
        return NodePathFinder.AreaFixedSize(Map, origin, size).Cast<TileEntity>().ToList();
    }

    /// <summary>
    /// Get a cluster area of walkable tiles
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<TileEntity> VacantArea(TileEntity origin, float range, List<TileEntity> dontAllowTileSet = null, List<TileOccupant> ignoreOccupantSet = null) 
    {   
        return NodePathFinder.VacantArea(Map, origin, range, dontAllowTileSet, ignoreOccupantSet).Cast<TileEntity>().ToList();
    }

    public List<TileEntity> VacantAreaFixedSize(TileEntity origin, int size, List<TileEntity> dontAllowTileSet = null, List<TileOccupant> ignoreOccupantSet = null)
    {
        return NodePathFinder.VacantAreaFixedSize(Map, origin, size, dontAllowTileSet, ignoreOccupantSet).Cast<TileEntity>().ToList();
    }

    /// <summary>
    /// Return a vacant triangle, consisting of 3 tiles
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public (int dir, List<TileEntity> tileSet) VacantTriangle(TileEntity origin, List<TileOccupant> ignoreOccupantSet = null)
    {   
        var (dir, tileSet) = NodePathFinder.VacantTriangle(Map, origin, ignoreOccupantSet);

        return (dir, tileSet.Select(n => GetTile(n.Position)).ToList());
    }

    public List<TileEntity> VacantSnowflake(TileEntity origin, List<TileOccupant> ignoreOccupantSet = null)
    {
        return NodePathFinder.VacantSnowflake(Map, origin, ignoreOccupantSet).Select(n => GetTile(n.Position)).ToList();
    }

    /// <summary>
    /// Check if the tile are vacant. May optionally exclude some occupants
    /// </summary>
    /// <param name="n"></param>
    /// <param name="ignoreOccupantSet"></param>
    /// <returns></returns>
    public bool IsVacantOrIgnored(TileEntity tile, List<TileOccupant> ignoreOccupantSet)
    {
        return tile.Vacant || (ignoreOccupantSet != null && ignoreOccupantSet.Contains(tile.Occupant)); 
    }

    /// <summary>
    /// Check if the tiles are vacant. May optionally exclude some occupants
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="ignoreOccupantSet"></param>
    /// <returns></returns>
    public bool IsVacantOrIgnored(List<TileEntity> tileSet, List<TileOccupant> ignoreOccupantSet = null)
    {
        bool flag = true;

        foreach(var t in tileSet)
        {
            flag = flag && IsVacantOrIgnored(t, ignoreOccupantSet);
        }

        return flag;
    }

    /// <summary>
    /// Return a walkable tile closest to the given tile
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public TileEntity WalkableTile(TileEntity origin)
    {
        return VacantAreaFixedSize(origin, 1)[0];
    }

    /// <summary>
    /// To preemptively check tiles requested by buildings. Return empty list if requested tiles cannot be satisfied
    /// </summary>
    /// <param name="buildingType"></param>
    /// <param name="origin"></param>
    /// <param name="buildByGroup"></param>
    /// <returns></returns>
    public (int dir, List<TileEntity> tileSet) RequestBuildingTiles(UnitGroup buildByGroup, BuildingType buildingType, TileEntity origin)
    {
        List<TileEntity> tiles = new();
        var dir = 0;

        switch (buildingType)
        {
            case BuildingType.ConstructionSite: 
            case BuildingType.Campfire: 
            case BuildingType.Cannon:
                (dir, tiles) = VacantTriangle(origin, buildByGroup.Occupants);
                break;
        }

        return (dir, tiles);
    }

    /// <summary>
    /// To preemptively check tiles requested by unit group. Return empty list if requested tiles cannot be satisfied
    /// </summary>
    /// <param name="group"></param>
    /// <param name="origin"></param>
    /// <param name="dontAllowTileSet"></param>
    /// <returns></returns>
    public List<TileEntity> RequestUnitTiles(UnitGroup group, TileEntity origin, List<TileEntity> dontAllowTileSet = null)
    {
        return VacantAreaFixedSize(origin, group.Size, dontAllowTileSet, group.Occupants);
    }

    /// <summary>
    /// Register a tile and associate it with an alias
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="tile"></param>
    public void Register(string alias, TileEntity tile) 
    {
        TileDictionary.Add(alias, tile);
    }

    /// <summary>
    /// Deregister a tile
    /// </summary>
    /// <param name="alias"></param>
    public void Deregister(string alias) 
    {
        TileDictionary.Remove(alias);
    }

    /// <summary>
    /// Retrieve a tile by its alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public TileEntity GetTile(string alias) 
    {
        TileEntity tile;
        TileDictionary.TryGetValue(alias, out tile);
        return tile;
    }

    /// <summary>
    /// Retrieve a tile by tile position
    /// </summary>
    /// <param name="tilePos"></param>
    /// <returns></returns>
    public TileEntity GetTile(Vector3Int tilePos) 
    {
        return Map.Tile(tilePos);
    }

    public string GetAlias(TileEntity tile)
    {
        return TileDictionary.FirstOrDefault(pair => pair.Value == tile).Key;
    }

    /// <summary>
    /// Check if a tile is registered
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool IsRegistered(TileEntity tile) 
    {
        return TileDictionary.ContainsValue(tile);
    }
    
    /// <summary>
    /// Clear all registered tiles
    /// </summary>
    public void ClearTiles() 
    {
        TileDictionary.Clear();
    }

    public string ToString(List<TileEntity> tileSet)
    {
        var str = "{";

        if (tileSet != null)
        {
            for (var index = 0; index < tileSet.Count; index++)
            {
                var t = tileSet[index];

                str += t;

                if (index != tileSet.Count - 1)
                {
                    str += ", ";
                }
            }
        }

        return str + "}";
    }

    public string ToString(List<TileOccupant> occpantSet)
    {
        var str = "{";

        if (occpantSet != null)
        {
            for (var index = 0; index < occpantSet.Count; index++)
            {
                var o = occpantSet[index];

                str += o is Unit ? "unit-at-" : "structure-at-" + GetAlias(o.Tile);

                if (index != occpantSet.Count - 1)
                {
                    str += ", ";
                }
            }
        }

        return str + "}";
    }

    public void Start()
    {
        // Lay out the preset entities once tile manager has been set up
        Map.SpawnPresetEntities();
    }

    public void Awake() 
    {   
        if (!MapView) {
            MapView = GameObject.FindObjectOfType<MapView>();
        }
        Map = new MapEntity(MapSettings, MapView); 
        if (MapView) {
            MapView.Init(Map);
        }
        else {
            Debug.LogWarning("Can't find MapView. Random errors can occur");
        }

        instance = this;
    }

    private void Update()
    {
        // if (Input.GetKeyUp(gridToggle))
        // {
        //     map.GridToggle();
        // }
    }
}
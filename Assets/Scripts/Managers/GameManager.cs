using System.Collections.Generic;
using RedBjorn.ProtoTiles;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public void EndGame(bool playerWin)
    {
        UIManager.instance.ShowEndMenu(playerWin);
    }

    public void Start()
    {
        Dictionary<string, Vector3Int> hexDictionary = new Dictionary<string, Vector3Int>()
        {
            { "rally pt div 1", new Vector3Int(8, 12, -20) },
            { "rally pt div 2", new Vector3Int(10, 7, -17) },
            { "rally pt div 3", new Vector3Int(7, 10, -17) },
            { "rally pt div 4", new Vector3Int(5, 10 ,-15) },
            { "rally pt cpu div 1", new Vector3Int(26, 11, -37) },
            { "rally pt cpu div 2", new Vector3Int(28, 8, -36) },
            { "rally pt cpu div 3", new Vector3Int(24, 11, -35) },
            { "rally pt cpu div 4", new Vector3Int(31, 8, -39)}
        };

        foreach (var (alias, pos) in hexDictionary) {
            var tile = TileManager.instance.GetTile(pos);
            TileManager.instance.Register(alias, tile);
        }

        Dictionary<string, TileEntity> tiles = TileManager.instance.TileDictionary;

        var engineerGroup = new UnitGroup("4", tiles["rally pt div 4"], 2, 2, UnitType.Engineer, Party.Player);
        var div1 = new UnitGroup("1", tiles["rally pt div 1"], 3, 3, UnitType.Soldier, Party.Player);
        var div2 = new UnitGroup("2", tiles["rally pt div 2"], 3, 3, UnitType.Soldier, Party.Player);
        var div3 = new UnitGroup("3", tiles["rally pt div 3"], 3, 3, UnitType.Soldier, Party.Player);

        // var cpuEngineer = new UnitGroup("cpu div 4", tiles["rally pt cpu div 4"], 2, 2, UnitType.Engineer, Party.CPU);
        var cpuDiv1 = new UnitGroup("cpu div 1", tiles["rally pt cpu div 1"], 0, 3, UnitType.Soldier, Party.CPU);
        var cpuDiv2 = new UnitGroup("cpu div 2", tiles["rally pt cpu div 2"], 0, 3, UnitType.Soldier, Party.CPU);
        var cpuDiv3 = new UnitGroup("cpu div 3", tiles["rally pt cpu div 3"], 0, 3, UnitType.Soldier, Party.CPU);
        // var cpuDiv1 = new UnitGroup("cpu div 1", )
        
        // UnitGroup div2 = new("2", tiles["a2"], 1, UnitType.Soldier);

        // UnitGroup cpu_div1 = new("cpu_div1", tiles["f4"], 2, UnitType.Soldier, Party.CPU);
        // UnitGroup cpu_div2 = new("cpu_div2", tiles["f0"], 5, UnitType.Soldier, Party.CPU);

        // ResourceStructure wood = new(6f * 5, ResourceType.Wood, tiles["c2"]);
        // ResourceStructure metal = new(6f * 4, ResourceType.Metal, tiles["d2"]);

        // Base b = new(_tileManager.VacantSnowflake(tiles["b1"]), 0);

        // Pan camera to player base at start
        var t = TileManager.instance.GetTile(new Vector3Int(2, 14, -16));
        var playerBase = t.Occupant;
        UIManager.instance.MoveCameraTo(playerBase);
    }

    public void Awake()
    {
        instance = this;
    }
}
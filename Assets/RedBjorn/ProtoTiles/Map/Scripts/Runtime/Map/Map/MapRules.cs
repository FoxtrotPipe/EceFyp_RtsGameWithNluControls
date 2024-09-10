using RedBjorn.ProtoTiles.Tiles;
using System;

namespace RedBjorn.ProtoTiles
{
    [Serializable]
    public class MapRules
    {
        /// <summary>
        /// Rules which should be filled for movable tiles
        /// </summary>
        public TileCondition IsMovable;
        public TileCondition IsWoodSpawn;
        public TileCondition IsMetalSpawn;
        public TileCondition IsBaseSpawn;
        public TileCondition IsHostileBaseSpawn;
    }
}

using System;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    [Serializable]
    public partial class TileEntity : INode
    {
        int CachedMovabeArea;
        int ObstacleCount;
        public TileData Data { get; private set; }
        public TilePreset Preset { get; private set; }
        // temp define local unit and entity properties
        public TileOccupant Occupant { get; set; } = null;
        // temp assign TileManager to each tile
        static private TileManager _manager = TileManager.instance;

        MapRules Rules;

        public int MovableArea { get { return CachedMovabeArea; } set { CachedMovabeArea = value; } }
        // temp implement not vacant when occupant is present
        public bool Vacant
        {
            get
            {
                if ((Data == null || Rules == null || Rules.IsMovable == null) && Occupant == null)
                {
                    return true;
                }
                return Rules.IsMovable.IsMet(this) && ObstacleCount == 0 && Occupant == null;
            }
        }
        public bool SetToSpawnWood { get => Rules.IsWoodSpawn.IsMet(this); }
        public bool SetToSpawnMetal { get => Rules.IsMetalSpawn.IsMet(this); }
        public bool SetToSpawnBase { get => Rules.IsBaseSpawn.IsMet(this); }
        public bool SetToSpawnHostileBase { get => Rules.IsHostileBaseSpawn.IsMet(this); }

        // public bool Walkable
        // {
        //     get
        //     {
        //         if (Data == null || Rules == null || Rules.IsMovable == null)
        //         {
        //             return true;
        //         }
        //         return Rules.IsMovable.IsMet(this) && ObstacleCount == 0;
        //     }
        // }

        public bool Visited { get; set; }
        public bool Considered { get; set; }
        public float Depth { get; set; }
        public float[] NeighbourMovable { get { return Data == null ? null : Data.SideHeight; } }
        public Vector3Int Position { get { return Data == null ? Vector3Int.zero : Data.TilePos; } }

        TileEntity() { }

        public TileEntity(TileData preset, TilePreset type, MapRules rules)
        {
            Data = preset;
            Rules = rules;
            Preset = type;
            MovableArea = Data.MovableArea;
        }

        public override string ToString()
        {
            return string.Format("Position: {0}. Vacant = {1}", Position, Vacant);
        }

        public void ChangeMovableAreaPreset(int area)
        {
            Data.MovableArea = area;
        }

        // temp define method to register local units and entities in tile
        public bool RegisterOccupant(TileOccupant occupant)
        {   
            if (Occupant != null && occupant is Unit)
            {
                // Debug.LogWarning("Disallow unit to override tile occupied by building structure or unit");
                return false;
            }
            else
            {
                Occupant = occupant;
                return true;
            }
        }

        public void DeregisterOccupant()
        {
            Occupant = null;
        }
    }
}


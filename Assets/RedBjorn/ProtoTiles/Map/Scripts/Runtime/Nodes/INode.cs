using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public interface INode
    {
        Vector3Int Position { get; }
        int MovableArea { get; }
        void ChangeMovableAreaPreset(int area);

        // New property defined to identify if the tile is vacant
        bool Vacant { get; }
        // New property defined to store occupant information
        TileOccupant Occupant { get; set; }

        float Depth { get; set; }
        bool Visited { get; set; }
        bool Considered { get; set; }
    }
}
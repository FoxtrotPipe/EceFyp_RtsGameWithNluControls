using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.ProtoTiles
{
    public class NodePathFinder
    {
        static Dictionary<INode, float> ScoreG = new Dictionary<INode, float>();
        static Dictionary<INode, float> ScoreF = new Dictionary<INode, float>();
        static Dictionary<INode, INode> CameFrom = new Dictionary<INode, INode>();

        public static HashSet<INode> AccessibleArea(IMapNode map, INode origin)
        {
            map.Reset();
            var open = new Queue<INode>();
            var closed = new HashSet<INode>();

            open.Enqueue(origin);
            var index = 0;
            while (open.Count > 0 && index < 100000)
            {
                var current = open.Dequeue();
                current.Considered = true;
                foreach (var n in map.NeighborsMovable(current).Where(neigh => neigh != null))
                {
                    if (n.Vacant && !n.Considered)
                    {
                        n.Considered = true;
                        open.Enqueue(n);
                        index++;
                    }
                }
                current.Visited = true;
                closed.Add(current);

            }
            return closed;
        }

        public static HashSet<INode> Area(IMapNode map, INode origin, float range, List<TileEntity> dontAllowTileSet = null)
        {
            map.Reset(Mathf.CeilToInt(range), origin);
            origin.Depth = 0f;
            var open = new Queue<INode>();
            var closed = new HashSet<INode>();

            open.Enqueue(origin);
            var index = 0;
            while (open.Count > 0 && index < 100000)
            {
                var current = open.Dequeue();
                current.Considered = true;
                foreach (var n in map.NeighborsMovable(current).Where(neigh => neigh != null))
                {
                    var currentDistance = current.Depth + map.Distance(current, n);
                    if (!n.Considered && currentDistance <= range)
                    {
                        n.Considered = true;
                        n.Depth = currentDistance;
                        open.Enqueue(n);
                        index++;
                    }
                }
                current.Visited = true;

                if (!(dontAllowTileSet != null && dontAllowTileSet.Contains(current)))
                {
                    closed.Add(current);
                }
            }
            return closed;
        }

        public static HashSet<INode> AreaFixedSize(IMapNode map, INode origin, int size, List<TileEntity> dontAllowTileSet = null)
        {
            map.Reset();
            var open = new Queue<INode>();
            var closed = new HashSet<INode>();

            open.Enqueue(origin);
            var index = 0;
            while (open.Count > 0 && index < 100000)
            {
                var current = open.Dequeue();
                current.Considered = true;
                foreach (var n in map.NeighborsMovable(current).Where(neigh => neigh != null))
                {
                    if (!n.Considered)
                    {
                        n.Considered = true;
                        open.Enqueue(n);
                        index++;
                    }
                }
                current.Visited = true;

                if (!dontAllowTileSet.Contains(current))
                {
                    closed.Add(current);
                }
                
                if (closed.Count == size)
                {
                    return closed;
                }
            }
            
            return closed.Count == size ? closed : null;
        }

        public static HashSet<INode> VacantArea(IMapNode map, INode origin, float range, List<TileEntity> dontAllowTileSet = null, List<TileOccupant> ignoreOccupantSet = null)
        {
            return Area(map, origin, range).Where(n => (dontAllowTileSet == null || !dontAllowTileSet.Contains(n)) && IsVacantOrIgnored(n, ignoreOccupantSet)).ToHashSet<INode>();
        }

        public static HashSet<INode> VacantAreaFixedSize(IMapNode map, INode origin, int size, List<TileEntity> dontAllowTileSet = null, List<TileOccupant> ignoreOccupantSet = null)
        {
            map.Reset();
            var open = new Queue<INode>();
            var closed = new HashSet<INode>();

            open.Enqueue(origin);
            var index = 0;
            while (open.Count > 0 && index < 100000)
            {
                var current = open.Dequeue();
                current.Considered = true;
                foreach (var n in map.NeighborsMovable(current).Where(neigh => neigh != null))
                {
                    if (!n.Considered)
                    {
                        n.Considered = true;
                        open.Enqueue(n);
                        index++;
                    }
                }
                current.Visited = true;

                if (!(dontAllowTileSet != null && dontAllowTileSet.Contains(current)) && IsVacantOrIgnored(current, ignoreOccupantSet))
                {
                    closed.Add(current);
                }

                if (closed.Count == size)
                {
                    return closed;
                }
            }
            
            return closed.Count == size ? closed : null;
        }

        public static HashSet<Vector3Int> VacantAreaPositions(IMapNode map, INode origin, float range)
        {
            map.Reset(Mathf.CeilToInt(range), origin);
            origin.Depth = 0f;
            var open = new Queue<INode>();
            var closed = new HashSet<Vector3Int>();

            open.Enqueue(origin);
            var index = 0;
            while (open.Count > 0 && index < 100000)
            {
                var current = open.Dequeue();
                current.Considered = true;
                foreach (var n in map.NeighborsMovable(current).Where(neigh => neigh != null && neigh.Vacant))
                {
                    var currentDistance = current.Depth + map.Distance(current, n);
                    if (!n.Considered && currentDistance <= range)
                    {
                        n.Considered = true;
                        n.Depth = currentDistance;
                        open.Enqueue(n);
                        index++;
                    }
                }
                current.Visited = true;
                closed.Add(current.Position);
            }
            return closed;
        }

        /// <summary>
        /// Get a vacant triangle of tiles. May optionally allow some occupants
        /// </summary>
        /// <param name="map"></param>
        /// <param name="origin"></param>
        /// <param name="ignoreOccupantSet"></param>
        /// <returns></returns>
        public static (int dir, HashSet<INode> tileSet) VacantTriangle(IMapNode map, INode origin, List<TileOccupant> ignoreOccupantSet = null)
        {
            map.Reset(Mathf.CeilToInt(1f), origin);
            var set = new HashSet<INode>();

            List<INode> list = map.NeighborsMovable(origin).Where(neigh => neigh != null).ToList();
            var dir = 0;
            for (var dirIndex = 0; dirIndex < list.Count; dirIndex++)
            {
                var n1 = list[dirIndex];
                var n2 = list[(dirIndex + 1) % list.Count];

                if (IsVacantOrIgnored(n1, ignoreOccupantSet) && IsVacantOrIgnored(n2, ignoreOccupantSet))
                {
                    set.Add(origin);
                    set.Add(n1);
                    set.Add(n2);
                    dir = dirIndex;
                    break;
                }
            }

            return (dir, set);
        }

        /// <summary>
        /// Get a vacant snowflake of tiles. May optionally allow some occupants
        /// </summary>
        /// <param name="map"></param>
        /// <param name="origin"></param>
        /// <param name="ignoreOccupantSet"></param>
        /// <returns></returns>
        public static HashSet<INode> VacantSnowflake(IMapNode map, INode origin, List<TileOccupant> ignoreOccupantSet = null)
        {
            return VacantArea(map, origin, 1, null, ignoreOccupantSet);
        }

        /// <summary>
        /// Check whether a given node is vacant or if its occpant is excluded (hence still count as "vacant")
        /// </summary>
        /// <param name="n"></param>
        /// <param name="ignoreOccupantSet"></param>
        /// <returns></returns>
        public static bool IsVacantOrIgnored(INode n, List<TileOccupant> ignoreOccupantSet)
        {
            return n.Vacant || (ignoreOccupantSet != null && ignoreOccupantSet.Contains(n.Occupant)); 
        }

        public static List<INode> Path(IMapNode map, INode start, INode finish, float range, List<TileOccupant> ignoreOccupantSet = null)
        {
            if (start.MovableArea != finish.MovableArea)
            {
                return null;
            }
            var fullPath = FindPath(map, start, finish, ignoreOccupantSet);
            return TrimPath(map, fullPath, range);
        }

        // public static List<INode> Path(IMapNode map, INode start, INode finish)
        // {
        //     if (start.MovableArea != finish.MovableArea)
        //     {
        //         return null;
        //     }
        //     return FindPath(map, start, finish);
        // }

        /// <summary>
        /// Find path, given start and finish node. May optioanlly allow some occupants
        /// </summary>
        /// <param name="map"></param>
        /// <param name="start"></param>
        /// <param name="finish"></param>
        /// <param name="ignoreOccupantSet"></param>
        /// <returns></returns>
        static List<INode> FindPath(IMapNode map, INode start, INode finish, List<TileOccupant> ignoreOccupantSet = null)
        {
            ScoreG.Clear();
            ScoreF.Clear();
            CameFrom.Clear();

            var path = new List<INode>();
            if (!IsVacantOrIgnored(finish, ignoreOccupantSet))
            {
                return path;
            }
            var open = new List<INode>();
            var closed = new List<INode>();
            open.Add(start);
            ScoreF[start] = map.Distance(start, finish);
            ScoreG[start] = 0;

            while (open.Any())
            {
                var check = open.OrderBy(o => ScoreF[o]).First();
                if (check == finish)
                {
                    break;
                }
                else if (closed.Contains(check))
                {
                    continue;
                }

                closed.Add(check);
                open.Remove(check);
                foreach (var node in map.NeighborsMovable(check).Where(n => IsVacantOrIgnored(n, ignoreOccupantSet)))
                {
                    var currengScoreG = ScoreG[check] + map.Distance(node, finish);
                    var gN = -1f;
                    if (ScoreG.TryGetValue(node, out gN))
                    {
                        if (currengScoreG < gN)
                        {
                            CameFrom[node] = check;
                            ScoreG[node] = currengScoreG;
                            ScoreF[node] = currengScoreG + map.Distance(node, finish);
                            CameFrom[node] = check;
                        }
                    }
                    else
                    {
                        open.Add(node);
                        ScoreG[node] = currengScoreG;
                        ScoreF[node] = currengScoreG + map.Distance(node, finish);
                        CameFrom[node] = check;
                    }
                }
            }
            var current = finish;
            while (CameFrom.ContainsKey(current))
            {
                path.Add(current);
                current = CameFrom[current];
            }
            path.Add(start);
            path.Reverse();

            return path;
        }

        static List<INode> TrimPath(IMapNode map, List<INode> path, float range)
        {
            var distance = 0f;
            int trimIndex = -1;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var step = distance + map.Distance(path[i], path[i + 1]);
                if (step <= range)
                {
                    distance = step;
                }
                else
                {
                    trimIndex = i + 1;
                    break;
                }
            }
            if (trimIndex >= 0)
            {
                path.RemoveRange(trimIndex, path.Count - trimIndex);
            }
            return path;
        }
    }
}
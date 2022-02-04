using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype01.Scripts.Others
{
    public class PathController : MonoBehaviour
    {
        private static readonly Collider2D[] Obstacles = new Collider2D[2];
        private static readonly RaycastHit2D[] Hits = new RaycastHit2D[16];

        [Space]
        [Min(1)]
        public int MaxIteration = 4096;
        
        [Space]
        public Grid Grid = default;
        public LayerMask ObstaclesLayerMask = 1;

        #region Public API

        public Vector2 Move(CellController cell, Vector2 target)
        {
            var direction = target - cell.Position;

            var count = Physics2D.CircleCastNonAlloc(cell.Position, cell.Body.Radius, direction.normalized, Hits, direction.magnitude, ObstaclesLayerMask);

            if (count == 0) return target;

            var nearest = 0;
            for (var i = 1; i < count; i++)
            {
                if (Hits[i].distance < Hits[nearest].distance) nearest = i;
            }

            Debug.DrawLine(Hits[nearest].point, Hits[nearest].point+ Hits[nearest].normal, Color.blue, 1f);
            
            return Hits[nearest].point + Hits[nearest].normal * cell.Body.Radius;
        }
        
        public Vector2[] GetPath(CellController cell, Vector2 target)
        {
            return GetPath(cell, target, out _);
        }
        
        public Vector2[] GetPath(CellController cell, Vector2 target, out float length)
        {
            return TryGetPath(cell, target, out var path, out length) ? path : null;
        }
        
        public bool TryGetPath(CellController cell, Vector2 target, out Vector2[] path)
        {
            return TryGetPath(cell, target, out path, out _);
        }
        
        public bool TryGetPath(CellController cell, Vector2 target, out Vector2[] path, out float length)
        {
            var points = new ConcurrentDictionary<Vector3Int, PathNode>();
            
            path = null;
            length = 0f;

            var nexts = new HashSet<PathNode>();
            var openList = new HashSet<PathNode>();
            var closedList = new HashSet<PathNode>();

            var cellOrigin = Grid.WorldToCell(cell.Position);
            var cellTarget = Grid.WorldToCell(target);

            var startPoint = GetPoint(points, cellOrigin, cell, Grid, ObstaclesLayerMask);
            startPoint.CostBase = 0;
            startPoint.CostTarget = 0;
            openList.Add(startPoint);

            var iteration = 0;
            while (openList.Count > 0)
            {
                var current = GetBest(openList);
                if (current.cell == cellTarget)
                {
                    // gotcha
                    path = CalculatePath(current);
                    path[0] = cell.Position;
                    path[path.Length - 1] = target;
                    length = current.CostBase;
                    return true;
                }

                openList.Remove(current);
                closedList.Add(current);
                
                GetNexts(points, cell, Grid, ObstaclesLayerMask, current, ref nexts);

                foreach (var next in nexts)
                {
                    if (closedList.Contains(next)) continue;

                    var cost = current.CostBase + GetCost(current, next);
                    if (cost < next.CostBase)
                    {
                        next.previous = current;
                        next.CostBase = cost;
                        next.CostTarget = GetCost(next, target);

                        if (!openList.Contains(next)) openList.Add(next);
                    }
                }

                if (MaxIteration <= ++iteration) return false;
            }
            
            return false;
        }

        #endregion

        #region Private API

        private static PathNode GetPoint(ConcurrentDictionary<Vector3Int, PathNode> points, int i, int j, CellController cell, GridLayout grid, LayerMask layerMask)
        {
            return GetPoint(points, new Vector3Int(i, j, 0), cell, grid, layerMask);
        }
        
        private static PathNode GetPoint(ConcurrentDictionary<Vector3Int, PathNode> points, Vector3Int cellInt, CellController cell, GridLayout grid, LayerMask layerMask)
        {
            if (!points.TryGetValue(cellInt, out var point))
            {
                point = new PathNode(cellInt);
                
                var count = Physics2D.OverlapCircleNonAlloc(grid.CellToWorld(cellInt), cell.Body.Radius, Obstacles, layerMask);
                point.isValid = count == 0 || (count == 1 && Obstacles[0] == cell.Body.Collider);
                
                points.TryAdd(cellInt, point);
            }

            return point;
        }

        private float GetCost(PathNode origin, PathNode target)
        {
            return GetCost(origin, Grid.CellToWorld(target.cell));
        }

        private float GetCost(PathNode origin, Vector2 target)
        {
            return Vector2.Distance(Grid.CellToWorld(origin.cell), target);
        }
        
        private static void GetNexts(ConcurrentDictionary<Vector3Int, PathNode> points, CellController cell, Grid grid, LayerMask layerMask, PathNode current, ref HashSet<PathNode> nexts)
        {
            nexts.Clear();

            for (var i = -1; i < 2; i++)
            {
                for (var j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) continue;

                    if (grid.cellLayout == GridLayout.CellLayout.Hexagon)
                    {
                        if (i == 1 && j != 0) continue;
                    }
                    
                    var next = GetPoint(points, current.i + i, current.j + j, cell, grid, layerMask);

                    if (!next.isValid) continue;

                    nexts.Add(next);
                }
            }
        }

        private static PathNode GetBest(IEnumerable<PathNode> points)
        {
            PathNode best = null;
            foreach (var point in points)
            {
                if (best == null || point.CostTotal < best.CostTotal) best = point;
            }
            return best;
        }

        private Vector2[] CalculatePath(PathNode node)
        {
            var points = new List<Vector2>();

            var current = node;

            while (current != null)
            {
                points.Add(Grid.CellToWorld(current.cell));
                current = current.previous;
            }

            points.Reverse();
            return points.ToArray();
        }

        #endregion
    }

    public class PathNode
    {
        public Vector3Int cell = Vector3Int.one;
        public PathNode previous = default;
        
        public float CostBase = float.MaxValue;
        public float CostTarget = float.MaxValue;

        public int i => cell.x;
        public int j => cell.y;
        public float CostTotal => CostBase + CostTarget;

        public bool isValid = true;
        
        public PathNode(Vector3Int cell)
        {
            this. cell = cell;
        }

        public PathNode(int i, int j) : this(new Vector3Int(i, j, 0))
        {
        }
    }
}
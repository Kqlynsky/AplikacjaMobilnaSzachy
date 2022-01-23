using static HartLib.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
//using static HartLib.PathFindingCell<TBlock><TBlock>;

namespace HartLib
{

    public class PathFinding<TBlock>
    {
        Vector2i gridSize;
        public PathFindingCell<TBlock>[,] grid { get; private set; }
        Func<Vector2i, HashSet<TBlock>, bool> checkBlockingTiles;
        Func<Vector2i, uint, bool> checkForColliders;

        public List<PathFindingCell<TBlock>> FindPath(Vector2i startPos, Vector2i endPos, HashSet<TBlock> blockingTiles, uint collider_mask = 0, bool include_checking_last = true, bool diagonals = false, bool big = false)
        {
            int safeCheck = 700;

            if (startPos.x < 0 || startPos.x > gridSize.x - 1) return null;
            if (startPos.y < 0 || startPos.y > gridSize.y - 1) return null;
            if (endPos.x < 0 || endPos.x > gridSize.x - 1) return null;
            if (endPos.y < 0 || endPos.y > gridSize.y - 1) return null;

            //if (blockingTiles is null) return null;
            if (big)
            {
                var adjacentEndPositions = new HashSet<Vector2i>() { new Vector2i(endPos + Vector2i.Right), new Vector2i(endPos + Vector2i.Up), new Vector2i(endPos + Vector2i.Up + Vector2i.Right) };

                foreach (var pos in adjacentEndPositions)
                {
                    if (CheckIfInRange(pos, gridSize) is false || grid[pos.x, pos.y] is null || grid[pos.x, pos.y].CheckForTiles(blockingTiles, checkBlockingTiles) || grid[pos.x, pos.y].CheckForColliders(collider_mask, checkForColliders))
                    {
                        return null;
                    }
                }
            }


            PathFindingCell<TBlock> startCell = grid[startPos.x, startPos.y];
            PathFindingCell<TBlock> endCell = grid[endPos.x, endPos.y];

            List<PathFindingCell<TBlock>> openSet = new List<PathFindingCell<TBlock>>();
            HashSet<PathFindingCell<TBlock>> closedSet = new HashSet<PathFindingCell<TBlock>>();
            openSet.Add(startCell);

            while (openSet.Count > 0 && safeCheck > 0)
            {
                safeCheck--;

                if (safeCheck <= 0) GD.Print("SafetyCheck");

                PathFindingCell<TBlock> currentCell = openSet[0];

                for (int i = 0; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentCell.FCost || openSet[i].FCost == currentCell.FCost && openSet[i].HCost < currentCell.HCost)
                    {
                        currentCell = openSet[i];
                    }
                }
                openSet.Remove(currentCell);
                closedSet.Add(currentCell);

                if (currentCell == endCell)
                {
                    if (include_checking_last && (currentCell.CheckForTiles(blockingTiles, checkBlockingTiles) || currentCell.CheckForColliders(collider_mask, checkForColliders)))
                    {
                        return null;
                    }
                    List<PathFindingCell<TBlock>> path = RetracePath(startCell, endCell);
                    return path;
                }

                List<PathFindingCell<TBlock>> neigbours;

                if (diagonals && big is false) { neigbours = GetNeigboursDiagonal(currentCell); }
                else { neigbours = GetNeigbours(currentCell); }

                for (int i = 0; i < neigbours.Count; i++)
                {
                    if (closedSet.Contains(neigbours[i]) || ((neigbours[i].CheckForTiles(blockingTiles, checkBlockingTiles) || (neigbours[i].CheckForColliders(collider_mask, checkForColliders))) && neigbours[i] != endCell))
                    {
                        continue;
                    }

                    bool adjacentCheck = true;
                    var neigbhourPos = neigbours[i].GridPos;

                    if (big && neigbhourPos != endPos)
                    {
                        var direction = (Dir)i;


                        if (neigbhourPos == endPos) GD.Print("XDDDD");
                        List<Vector2i> adjacent = new List<Vector2i>() { };

                        switch (direction)
                        {
                            case Dir.Up:
                                adjacent.Add(new Vector2i(neigbhourPos + Vector2i.Up));
                                adjacent.Add(new Vector2i(neigbhourPos + Vector2i.Up + Vector2i.Right));
                                break;
                            case Dir.Right:
                                adjacent.Add(new Vector2i(neigbhourPos + Vector2i.Right));
                                adjacent.Add(new Vector2i(neigbhourPos + Vector2i.Right + Vector2i.Up));
                                break;
                            case Dir.Down:
                                adjacent.Add(new Vector2i(neigbhourPos + Vector2i.Right));
                                break;
                            case Dir.Left:
                                adjacent.Add(new Vector2i(neigbhourPos + Vector2i.Up));
                                break;
                        }

                        foreach (var pos in adjacent)
                        {
                            if (CheckIfInRange(pos, gridSize) is false || grid[pos.x, pos.y] is null)
                            {
                                adjacentCheck = false;
                                break;
                            }


                            var adjacentNeigbour = grid[pos.x, pos.y];

                            if (closedSet.Contains(adjacentNeigbour) || ((adjacentNeigbour.CheckForTiles(blockingTiles, checkBlockingTiles) || (adjacentNeigbour.CheckForColliders(collider_mask, checkForColliders))) && adjacentNeigbour != endCell))
                            {
                                adjacentCheck = false;
                                break;
                            }
                            if (adjacentCheck is false) break;
                        }
                    }

                    if (adjacentCheck is false) continue;

                    int newCostToNeighbour = currentCell.GCost + GetDistance(currentCell, neigbours[i]);
                    if (newCostToNeighbour < neigbours[i].GCost || openSet.Contains(neigbours[i]) is false)
                    {
                        neigbours[i].GCost = newCostToNeighbour;
                        neigbours[i].HCost = GetDistance(neigbours[i], endCell);
                        neigbours[i].Parent = currentCell;

                        if (openSet.Contains(neigbours[i]) == false)
                        {
                            openSet.Add(neigbours[i]);
                        }
                    }
                }

            }
            return null;
        }

        public List<Vector2i> FindPossibleSpaces(Vector2i startPos, int range, HashSet<TBlock> blockingTiles, uint collider_mask = 0)
        {
            var possibleSpaces = new List<PathFindingCell<TBlock>>();
            int safeCheck = 700;


            PathFindingCell<TBlock> startCell = grid[startPos.x, startPos.y];
            startCell.Step = 0;

            List<PathFindingCell<TBlock>> openSet = new List<PathFindingCell<TBlock>>();
            HashSet<PathFindingCell<TBlock>> closedSet = new HashSet<PathFindingCell<TBlock>>();
            openSet.Add(startCell);

            while (openSet.Count > 0 && safeCheck > 0)
            {
                safeCheck--;

                if (safeCheck <= 0) GD.Print("SafetyCheck");

                PathFindingCell<TBlock> currentCell = openSet[0];

                openSet.Remove(currentCell);
                closedSet.Add(currentCell);
                //possibleSpaces.Add(currentCell);
                List<PathFindingCell<TBlock>> neigbours = GetNeigboursDiagonal(currentCell);

                for (int i = 0; i < neigbours.Count; i++)
                {

                    if (closedSet.Contains(neigbours[i])
                    || ((neigbours[i].CheckForTiles(blockingTiles, checkBlockingTiles)
                    || (neigbours[i].CheckForColliders(collider_mask, checkForColliders))))
                    || (Math.Abs(startPos.x - neigbours[i].GridPos.x) > range)
                    || (Math.Abs(startPos.y - neigbours[i].GridPos.y) > range))
                    {
                        continue;
                    }


                    var neigbhourPos = neigbours[i].GridPos;
                    int newCostToNeighbour = currentCell.GCost + GetDistance(currentCell, neigbours[i]);

                    if (newCostToNeighbour < neigbours[i].GCost || openSet.Contains(neigbours[i]) is false)
                    {
                        neigbours[i].GCost = newCostToNeighbour;
                        neigbours[i].Parent = currentCell;
                        neigbours[i].Step = currentCell.Step + 1;

                        if (neigbours[i].Step <= range && possibleSpaces.Contains(neigbours[i]) is false) { possibleSpaces.Add(neigbours[i]); }
                        if (openSet.Contains(neigbours[i]) == false)
                        {
                            openSet.Add(neigbours[i]);
                        }
                    }
                }

            }



            return possibleSpaces.Select(x => x.GridPos).ToList();
        }

        List<PathFindingCell<TBlock>> GetNeigbours(PathFindingCell<TBlock> cell)
        {
            List<PathFindingCell<TBlock>> neigbours = new List<PathFindingCell<TBlock>>();

            var dirs = Enum.GetValues(typeof(Dir));
            foreach (Dir direction in dirs)
            {
                var neigbhourPos = cell.GridPos + directions4[direction];
                if (CheckIfInRange(neigbhourPos, gridSize) && grid[neigbhourPos.x, neigbhourPos.y] != null)
                {
                    neigbours.Add(grid[neigbhourPos.x, neigbhourPos.y]);
                }
            }
            return neigbours;
        }

        List<PathFindingCell<TBlock>> GetNeigboursDiagonal(PathFindingCell<TBlock> cell)
        {
            List<PathFindingCell<TBlock>> neigbours = new List<PathFindingCell<TBlock>>();

            var dirs = Enum.GetValues(typeof(DirDiagonal));
            foreach (DirDiagonal direction in dirs)
            {
                var neigbhourPos = cell.GridPos + directions8[direction];
                if (CheckIfInRange(neigbhourPos, gridSize) && grid[neigbhourPos.x, neigbhourPos.y] != null)
                {
                    neigbours.Add(grid[neigbhourPos.x, neigbhourPos.y]);
                }
            }
            return neigbours;
        }

        List<PathFindingCell<TBlock>> RetracePath(PathFindingCell<TBlock> startNode, PathFindingCell<TBlock> endNode)
        {
            List<PathFindingCell<TBlock>> path = new List<PathFindingCell<TBlock>>();
            PathFindingCell<TBlock> curCell = endNode;

            while (curCell != startNode)
            {
                path.Add(curCell);
                curCell = curCell.Parent;
            }

            path.Reverse();
            return path;
        }

        int GetDistance(PathFindingCell<TBlock> cellA, PathFindingCell<TBlock> cellB)
        {
            int distantX = Mathf.Abs((int)(cellA.GridPos.x - cellB.GridPos.x));
            int distantY = Mathf.Abs((int)(cellA.GridPos.y - cellB.GridPos.y));

            if (distantX > distantY)
                return 14 * distantY + 10 * (distantX - distantY);
            else return 14 * distantX + 10 * (distantY - distantX);
        }


        public PathFinding(Vector2i _gridSize, Func<Vector2i, HashSet<TBlock>, bool> _checkBlockingTiles, Func<Vector2i, uint, bool> _checkForColliders = null)
        {
            gridSize = _gridSize;
            checkBlockingTiles = _checkBlockingTiles;
            checkForColliders = _checkForColliders;
            // for example: Func<Vector2i, HashSet<Main.TileTypes>, bool> checkForBlocking = (pos, blocking) => { return blocking.Contains((Main.TileTypes)Main.mapTiles.GetCellv(pos.Vec2())); };

            grid = new PathFindingCell<TBlock>[gridSize.x, gridSize.y];

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    grid[x, y] = new PathFindingCell<TBlock>(new Vector2i(x, y));
                }
            }
        }
    }
}
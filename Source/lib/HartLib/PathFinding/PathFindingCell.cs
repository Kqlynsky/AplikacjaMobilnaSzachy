using static HartLib.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HartLib
{
    public class PathFindingCell<TBlock>
    {
        public Vector2i GridPos { get; set; }
        public PathFindingCell<TBlock> Parent { get; set; }
        public PathFindingCell<TBlock> Child { get; set; }

        public bool CheckForTiles(HashSet<TBlock> checkingFor, Func<Vector2i, HashSet<TBlock>, bool> checkBlockingTiles)
        {
            if (checkingFor is null || checkingFor.Count < 1) return false;

            if (checkBlockingTiles(GridPos, checkingFor)) { return true; }

            return false;
        }

        public bool CheckForColliders(uint checkingFor, Func<Vector2i, uint, bool> checkColliders)
        {
            if (checkColliders is null || checkingFor == 0) return false;

            if (checkColliders(GridPos, checkingFor)) { return true; }

            return false;
        }

        public int Step { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost
        {
            get { return GCost + HCost; }
        }

        public PathFindingCell(Vector2i gridPos)
        {
            this.GridPos = gridPos;
        }

    }
}
using static HartLib.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HartLib
{

    public class Fov<TileType>
    {
        HashSet<TileType> blocking;
        HashSet<Vector2ui> uncovered = new HashSet<Vector2ui>();
        HashSet<Vector2ui> previousUncovered = new HashSet<Vector2ui>();
        HashSet<Vector2ui> allUncovered = new HashSet<Vector2ui>();
        Func<Vector2i, HashSet<TileType>, bool> checkBlocking;

        public HashSet<Vector2ui> Uncovered { get => uncovered; }
        public HashSet<Vector2ui> PreviousUncovered { get => previousUncovered; }
        public HashSet<Vector2ui> AllUncovered { get => allUncovered; }
        private Vector2ui MapSize { get; set; }


        //Todo Find quadrant by pos 
        //Todo recompute only specific quadrant
        public void Compute(Vector2i _origin, uint rangeLimit, uint uncoverArround = 0)
        {
            var origin = _origin.Vec2ui();
            InitUncovered(origin);
            if (uncoverArround > 0) UncoverAround(origin, uncoverArround);

            for (uint octant = 0; octant < 8; octant++) Compute(octant, origin, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));

            allUncovered.UnionWith(Uncovered);
        }

        public void Compute(Vector2ui origin, uint rangeLimit, uint dir = 0, uint uncoverArround = 0)
        {
            InitUncovered(origin);

            if (uncoverArround > 0) UncoverAround(origin, uncoverArround);

            Compute((dir - 1) % 8, origin, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));
            Compute(dir % 8, origin, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));

            allUncovered.UnionWith(Uncovered);
        }

        private void InitUncovered(Vector2ui origin)
        {
            previousUncovered = new HashSet<Vector2ui>(uncovered);
            uncovered.Clear();
            uncovered.Add(origin);
        }
        public void UncoverAround(Vector2ui pos, uint distance)
        {
            if (distance == 0) return;
            for (int y = -(int)distance; y < distance + 1; y++)
            {
                for (int x = -(int)distance; x < distance + 1; x++)
                {
                    uncovered.Add(pos + new Vector2ui(x, y));
                }
            }
        }

        struct Slope // represents the slope Y/X as a rational number
        {
            public Slope(uint y, uint x) { Y = y; X = x; }

            public bool Greater(uint y, uint x) { return Y * x > X * y; } // this > y/x
            public bool GreaterOrEqual(uint y, uint x) { return Y * x >= X * y; } // this >= y/x
            public bool Less(uint y, uint x) { return Y * x < X * y; } // this < y/x
            public bool LessOrEqual(uint y, uint x) { return Y * x <= X * y; } // this <= y/x

            public readonly uint X, Y;
        }
        public uint GetFovDirFromVec(Vector2i dir)
        {
            uint fov_dir = 0;

            if (dir.x > 0)
            {
                if (dir.y < 0) fov_dir = 1;
                else if (dir.y > 0) fov_dir = 7;
                return fov_dir;
            }

            if (dir.x < 0)
            {
                fov_dir = 4;
                if (dir.y < 0) fov_dir = 3;
                else if (dir.y > 0) fov_dir = 5;
                return fov_dir;
            }

            if (dir.y < 0) fov_dir = 2;
            else if (dir.y > 0) fov_dir = 6;
            return fov_dir;

        }
        bool BlocksLight(Vector2ui pos, uint octant, Vector2ui origin)
        {
            uint nx = origin.x, ny = origin.y;
            switch (octant)
            {
                case 0: nx += pos.x; ny -= pos.y; break;
                case 1: nx += pos.y; ny -= pos.x; break;
                case 2: nx -= pos.y; ny -= pos.x; break;
                case 3: nx -= pos.x; ny -= pos.y; break;
                case 4: nx -= pos.x; ny += pos.y; break;
                case 5: nx -= pos.y; ny += pos.x; break;
                case 6: nx += pos.y; ny += pos.x; break;
                case 7: nx += pos.x; ny += pos.y; break;
            }
            return checkBlocking(new Vector2i((int)nx, (int)ny), blocking);
        }

        void SetVisible(Vector2ui pos, uint octant, Vector2ui origin)
        {
            uint nx = origin.x, ny = origin.y;
            switch (octant)
            {
                case 0: nx += pos.x; ny -= pos.y; break;
                case 1: nx += pos.y; ny -= pos.x; break;
                case 2: nx -= pos.y; ny -= pos.x; break;
                case 3: nx -= pos.x; ny -= pos.y; break;
                case 4: nx -= pos.x; ny += pos.y; break;
                case 5: nx -= pos.y; ny += pos.x; break;
                case 6: nx += pos.y; ny += pos.x; break;
                case 7: nx += pos.x; ny += pos.y; break;
            }

            var newPos = new Vector2ui(nx, ny);
            if (CheckIfInRange(newPos, MapSize) is false) { return; }
            uncovered.Add(newPos);
        }

        void Compute(uint octant, Vector2ui origin, uint rangeLimit, uint x, Slope top, Slope bottom)
        {
            for (; x <= (uint)rangeLimit; x++)
            {
                uint topY;
                if (top.X == 1)
                {
                    topY = x;
                }
                else
                {

                    topY = ((x * 2 - 1) * top.Y + top.X) / (top.X * 2);

                    if (BlocksLight(new Vector2ui(x, topY), octant, origin))
                    {
                        if (top.GreaterOrEqual(topY * 2 + 1, x * 2) && !BlocksLight(new Vector2ui(x, topY + 1), octant, origin)) topY++;

                    }
                    else
                    {
                        uint ax = x * 2;
                        if (BlocksLight(new Vector2ui(x + 1, topY + 1), octant, origin)) ax++;
                        if (top.Greater(topY * 2 + 1, ax)) topY++;
                    }
                }
                uint bottomY;
                if (bottom.Y == 0)
                {
                    bottomY = 0;
                }
                else
                {
                    bottomY = ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);

                    if (bottom.GreaterOrEqual(bottomY * 2 + 1, x * 2)
                        && BlocksLight(new Vector2ui(x, bottomY), octant, origin)
                        && !BlocksLight(new Vector2ui(x, bottomY + 1), octant, origin))
                    {
                        bottomY++;
                    }
                }

                int wasOpaque = -1;

                for (uint y = topY; (int)y >= (int)bottomY; y--)
                {
                    if ((int)y < 0 || y > 200) GD.Print("LOL ", y);

                    if (rangeLimit < 0 || GetDistance(new Vector2i(0, 0), new Vector2i(x, y)) <= rangeLimit)
                    {
                        bool isOpaque = BlocksLight(new Vector2ui(x, y), octant, origin);
                        //bool isVisible = isOpaque || ((y != topY || top.Greater(y * 4 - 1, x * 4 + 1)) && (y != bottomY || bottom.Less(y * 4 + 1, x * 4 - 1)));

                        // NOTE: if you want the algorithm to be either fully or mostly symmetrical, replace the line above with the
                        // following line (and uncomment the Slope.LessOrEqual method). the line ensures that a clear tile is visible
                        // only if there's an unobstructed line to its center. if you want it to be fully symmetrical, also remove
                        // the "isOpaque ||" part and see NOTE comments further down
                        bool isVisible = ((y != topY || top.GreaterOrEqual(y, x)) && (y != bottomY || bottom.LessOrEqual(y, x)));

                        if (isVisible) SetVisible(new Vector2ui(x, y), octant, origin);

                        if (x != rangeLimit)
                        {
                            if (isOpaque)
                            {
                                if (wasOpaque == 0)
                                {
                                    uint nx = x * 2, ny = y * 2 + 1;
                                    //if (BlocksLight(new Vector2ui(x, y + 1), octant, origin)) nx--;
                                    if (top.Greater(ny, nx))
                                    {
                                        if (y == bottomY) { bottom = new Slope(ny, nx); break; }
                                        else Compute(octant, origin, rangeLimit, x + 1, top, new Slope(ny, nx));
                                    }
                                    else
                                    {
                                        if (y == bottomY) return;
                                    }
                                }
                                wasOpaque = 1;
                            }
                            else
                            {
                                if (wasOpaque > 0)
                                {
                                    uint nx = x * 2, ny = y * 2 + 1;
                                    //if (BlocksLight(new Vector2ui(x + 1, y + 1), octant, origin)) nx++;

                                    if (bottom.GreaterOrEqual(ny, nx)) return;
                                    top = new Slope(ny, nx);
                                }
                                wasOpaque = 0;
                            }
                        }
                    }
                }


                if (wasOpaque != 0) break;
            }
        }
        public Fov(Vector2ui _mapSize, HashSet<TileType> _blocking, Func<Vector2i, HashSet<TileType>, bool> _checkBlocking)
        {
            MapSize = _mapSize;
            blocking = _blocking;
            checkBlocking = _checkBlocking;
            // for example: Func<Vector2i, HashSet<Main.TileTypes>, bool> checkForBlocking = (pos, blocking) => { return blocking.Contains((Main.TileTypes)Main.mapTiles.GetCellv(pos.Vec2())); };
        }

    }


}
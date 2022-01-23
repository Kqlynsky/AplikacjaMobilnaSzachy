using static HartLib.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HartLib
{
    public static class BresenhamLine
    {

        public static List<Vector2i> GetLine(Vector2i pos1, Vector2i pos2) => GetLine(pos1, pos2, 100);

        public static List<Vector2i> GetLine(Vector2i pos1, Vector2i pos2, int maxLenght)
        {
            List<Vector2i> line = new List<Vector2i>();

            int lenght = 0;
            int w = pos2.x - pos1.x;
            int h = pos2.y - pos1.y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                var newPos = new Vector2i(pos1.x, pos1.y);
                line.Add(newPos);
                lenght++;
                if (lenght >= maxLenght && maxLenght > 0) { return line; }

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    pos1.x += dx1;
                    pos1.y += dy1;
                }
                else
                {
                    pos1.x += dx2;
                    pos1.y += dy2;
                }
            }
            return line;
        }
    }
}
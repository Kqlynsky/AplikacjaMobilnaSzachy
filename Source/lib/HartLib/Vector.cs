using static HartLib.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HartLib
{
    public struct Vector2i
    {
        public int x { get; set; }
        public int y { get; set; }


        public static Vector2i Zero = new Vector2i(0, 0);

        public static Vector2i Up = new Vector2i(0, 1);
        public static Vector2i Down = new Vector2i(0, -1);
        public static Vector2i Left = new Vector2i(-1, 0);
        public static Vector2i Right = new Vector2i(1, 0);


        public override string ToString() => $"({x}, {y})";
        public int AddValues { get { return x + y; } }



        public Vector2 Vec2()
        {
            return new Vector2(x, y);
        }
        public Vector2ui Vec2ui()
        {
            return new Vector2ui(Math.Abs(x), Math.Abs(y));
        }

        public float Vec2Float()
        {
            return (float)x / y;
        }
        public static bool operator ==(Vector2i vec1, Vector2i vec2)
        {
            if (vec1.x == vec2.x && vec1.y == vec2.y) return true;
            else return false;
        }

        public static bool operator !=(Vector2i vec1, Vector2i vec2)
        {
            if (vec1.x == vec2.x && vec1.y == vec2.y) return false;
            else return true;
        }

        public static bool operator ==(Vector2i vec1, Vector2 vec2)
        {
            if (vec1.x == (int)vec2.x && vec1.y == (int)vec2.y) return true;
            else return false;
        }

        public static bool operator !=(Vector2i vec1, Vector2 vec2)
        {
            if (vec1.x == (int)vec2.x && vec1.y == (int)vec2.y) return false;
            else return true;
        }

        public static Vector2i operator *(Vector2i vec1, int value)
        {
            var newVec = new Vector2i(vec1.x * value, vec1.y * value);
            return newVec;
        }

        public static Vector2i operator *(int value, Vector2i vec1)
        {
            return vec1 * value;
        }

        public static Vector2i operator *(Vector2i vec1, Vector2i vec2)
        {
            var newVec = new Vector2i(vec1.x * vec2.x, vec1.y * vec2.y);
            return newVec;
        }
        public static Vector2i operator /(Vector2i vec1, int value)
        {
            var newVec = new Vector2i(vec1.x / value, vec1.y / value);
            return newVec;
        }

        public static Vector2i operator -(Vector2i vec1, Vector2i vec2)
        {
            var newVec = new Vector2i(vec1.x - vec2.x, vec1.y - vec2.y);
            return newVec;
        }
        public static Vector2i operator +(Vector2i vec1, Vector2i vec2)
        {
            var newVec = new Vector2i(vec1.x + vec2.x, vec1.y + vec2.y);
            return newVec;
        }

        public Vector2i(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public Vector2i(Vector2 vec)
        {
            x = (int)vec.x;
            y = (int)vec.y;
        }
        public Vector2i(Vector2i vec)
        {
            x = vec.x;
            y = vec.y;
        }
        public Vector2i(Vector2ui vec)
        {
            x = (int)vec.x;
            y = (int)vec.y;

        }
        public Vector2i(uint _x, uint _y)
        {
            x = Math.Abs((int)_x);
            y = Math.Abs((int)_y);
        }
    }

    public struct Vector2ui
    {
        public uint x { get; set; }
        public uint y { get; set; }

        public static Vector2ui Zero = new Vector2ui(0, 0);

        public override string ToString() => $"({x}, {y})";
        public uint AddValues { get { return x + y; } }



        //To Vector2
        public Vector2 Vec2()
        {
            return new Vector2(x, y);
        }
        public Vector2i Vec2i()
        {
            return new Vector2i(x, y);
        }

        public static bool operator ==(Vector2ui vec1, Vector2ui vec2)
        {
            if (vec1.x == vec2.x && vec1.y == vec2.y) return true;
            else return false;
        }

        public static bool operator !=(Vector2ui vec1, Vector2ui vec2)
        {
            if (vec1.x == vec2.x && vec1.y == vec2.y) return false;
            else return true;
        }

        public static bool operator ==(Vector2ui vec1, Vector2 vec2)
        {
            if (vec1.x == (int)vec2.x && vec1.y == (int)vec2.y) return true;
            else return false;
        }

        public static bool operator !=(Vector2ui vec1, Vector2 vec2)
        {
            if (vec1.x == (int)vec2.x && vec1.y == (int)vec2.y) return false;
            else return true;
        }

        public static Vector2i operator *(Vector2ui vec1, int value)
        {
            var newVec = new Vector2i((int)vec1.x * value, (int)vec1.y * value);
            return newVec;
        }
        public static Vector2i operator *(Vector2ui vec1, Vector2i vec2)
        {
            var newVec = new Vector2i((int)vec1.x * vec2.x, (int)vec1.y * vec2.y);
            return newVec;
        }
        public static Vector2i operator /(Vector2ui vec1, int value)
        {
            var newVec = new Vector2i((int)vec1.x / value, (int)vec1.y / value);
            return newVec;
        }

        public static Vector2i operator -(Vector2ui vec1, Vector2i vec2)
        {
            var newVec = new Vector2i((int)vec1.x - vec2.x, (int)vec1.y - vec2.y);
            return newVec;
        }
        public static Vector2ui operator +(Vector2ui vec1, Vector2ui vec2)
        {
            var newVec = new Vector2ui(vec1.x + vec2.x, vec1.y + vec2.y);
            return newVec;
        }

        public Vector2ui(uint _x, uint _y)
        {
            x = _x;
            y = _y;
        }
        public Vector2ui(int _x, int _y)
        {
            x = (uint)Math.Abs(_x);
            y = (uint)Math.Abs(_y);
        }
        public Vector2ui(Vector2 vec)
        {
            x = (uint)Math.Abs(vec.x);
            y = (uint)Math.Abs(vec.y);
        }
        public Vector2ui(Vector2ui vec)
        {
            x = vec.x;
            y = vec.y;
        }
        public Vector2ui(Vector2i vec)
        {
            x = (uint)Math.Abs(vec.x);
            y = (uint)Math.Abs(vec.y);
        }
    }


}
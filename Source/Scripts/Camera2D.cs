using Godot;
using System;

namespace Chess.Source.Scripts
{
    public class Camera2D : Godot.Camera2D
    {
        public override void _Ready()
        {
            float offset = Board.CellSize * Board.BoardSize / 2;
            Position = new Vector2(offset, offset);
        }

    }
}
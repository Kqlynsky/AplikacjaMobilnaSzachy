using Godot;
using System;
using HartLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Source.Scripts
{
    public partial class Board : Node2D
    {
        public List<Vector2i> GetKingMoves(Piece piece)
        {
            var moves = new List<Vector2i>();
            foreach (var dir in HartLib.Utils.directions8)
            {
                var newPos = piece.GridPos + dir.Value;
                if (IsOnBoard(newPos) && FriendlyPieceOnPos(newPos, piece) is false) { moves.Add(newPos); }
            }
            return moves;
        }

        public List<Vector2i> GetPawnMoves(Piece piece)
        {
            var moves = new List<Vector2i>();
            var pos = piece.GridPos;
            var startingRank = 6;
            var directionUp = -1;

            if (piece.GetPieceColor() is PieceInfo.Black)
            {
                startingRank = 1;
                directionUp = 1;
            }

            var posUpOnce = new Vector2i(pos.x, pos.y + directionUp);
            var diagonals = new List<Vector2i> { pos + new Vector2i(1, directionUp), pos + new Vector2i(-1, directionUp) };

            if (PieceOnPos(posUpOnce) is null)
            {
                moves.Add(posUpOnce);
                var posUpTwice = posUpOnce + new Vector2i(0, directionUp);
                if (pos.y == startingRank && PieceOnPos(posUpTwice) is null) { moves.Add(posUpTwice); };
            }

            foreach (var diagonal in diagonals)
            {
                if (PieceOnPos(diagonal) is not null && PieceOnPos(diagonal)!.GetPieceColor() != piece.GetPieceColor())
                {
                    moves.Add(diagonal);
                }
            }
            return moves;
        }

        public List<Vector2i> GetBishopMoves(Piece piece)
        {
            var moves = new List<Vector2i>();
            foreach (var dir in HartLib.Utils.directionsDiagonals)
            {
                var newPos = piece.GridPos + dir.Value;
                while (IsOnBoard(newPos))
                {
                    if (FriendlyPieceOnPos(newPos, piece)) { break; }
                    moves.Add(newPos);
                    if (PieceOnPos(newPos) is not null)
                    {
                        break;
                    }
                    newPos += dir.Value;
                }
            }
            return moves;
        }

        public List<Vector2i> GetRookMoves(Piece piece)
        {
            var moves = new List<Vector2i>();
            foreach (var dir in HartLib.Utils.directions4)
            {
                var newPos = piece.GridPos + dir.Value;
                while (IsOnBoard(newPos))
                {
                    if (FriendlyPieceOnPos(newPos, piece)) { break; }
                    moves.Add(newPos);
                    if (PieceOnPos(newPos) is not null)
                    {
                        break;
                    }
                    newPos += dir.Value;
                }
            }
            return moves;
        }

        public List<Vector2i> GetQueenMoves(Piece piece)
        {
            List<Vector2i> moves = GetBishopMoves(piece).Concat(GetRookMoves(piece)).ToList();
            return moves;
        }

        public List<Vector2i> GetKnightMoves(Piece piece)
        {
            List<Vector2i> moves = new List<Vector2i>();
            var newPos = new Vector2i(piece.GridPos + 2 * Utils.directions4[Utils.Dir.Up]);
            moves.Add(newPos + Vector2i.Left);
            moves.Add(newPos + Vector2i.Right);

            newPos = new Vector2i(piece.GridPos + 2 * Utils.directions4[Utils.Dir.Down]);
            moves.Add(newPos + Vector2i.Left);
            moves.Add(newPos + Vector2i.Right);

            newPos = new Vector2i(piece.GridPos + 2 * Utils.directions4[Utils.Dir.Left]);
            moves.Add(newPos + Vector2i.Up);
            moves.Add(newPos + Vector2i.Down);

            newPos = new Vector2i(piece.GridPos + 2 * Utils.directions4[Utils.Dir.Right]);
            moves.Add(newPos + Vector2i.Up);
            moves.Add(newPos + Vector2i.Down);

            for (int i = moves.Count - 1; i >= 0; i--)
            {
                if (FriendlyPieceOnPos(moves[i], piece) || IsOnBoard(moves[i]) is false) moves.Remove(moves[i]);
            }
            return moves;
        }
    }
}

using Godot;
using System;
using System.Collections.Generic;
using HartLib;

namespace Chess.Source.Scripts
{
    public class Piece : Godot.Sprite
    {
        public const float SpriteSize = 320;
        public Vector2i GridPos { get; set; } = new Vector2i(0, 0);
        public Func<Piece, List<Vector2i>>? GetPossibleMoves { get; private set; }
        public PieceInfo Type
        {
            get => type;
            set
            {
                if (IsPieceValid(value) is false) { GD.PrintErr("Invalid pieceType"); }
                type = value;
                SetPieceMoves(value);
                SetPieceImage(value);
            }
        }

        Board? board;
        PieceInfo type = PieceInfo.None;
        List<Vector2i> moves = new List<Vector2i>();

        public void Input(Node viepoint, InputEvent inputEvent, int local_shape)
        {
            if (inputEvent.IsActionPressed(Board.GrabAction))
            {
                Grab();
            }
            else if (inputEvent.IsActionReleased(Board.GrabAction))
            {
                Release();
            }
        }

        public void Capture()
        {
            GD.Print("Captured ", Type);
            CallDeferred("free"); //Deletes piece node
        }

        private void Grab()
        {
            board?.GrabPiece(this);
        }

        private void Release()
        {
            board?.ReleasePiece(this);
        }

        public List<Vector2i> GetMoves()
        {
            return moves = GetPossibleMoves?.Invoke(this) ?? new List<Vector2i>();
        }

        public void Move(Vector2i newGridPos)
        {
            GridPos = newGridPos;
            Position = (newGridPos * Board.CellSize).Vec2();
            moves.Clear();
        }

        private void SetPieceImage(PieceInfo type)
        {
            var pieceTypeFrame = ((int)type % 8) - 1;
            if (type.HasFlag(PieceInfo.Black)) { pieceTypeFrame += 6; }
            Frame = pieceTypeFrame;
        }
        private void SetPieceMoves(PieceInfo type)
        {
            var typeNoColor = GetPieceFigure(type);
            switch (typeNoColor)
            {
                case PieceInfo.Pawn:
                    GetPossibleMoves = board!.GetPawnMoves;
                    break;
                case PieceInfo.Bishop:
                    GetPossibleMoves = board!.GetBishopMoves;
                    break;
                case PieceInfo.Rook:
                    GetPossibleMoves = board!.GetRookMoves;
                    break;
                case PieceInfo.Queen:
                    GetPossibleMoves = board!.GetQueenMoves;
                    break;
                case PieceInfo.Knight:
                    GetPossibleMoves = board!.GetKnightMoves;
                    break;
                default:
                    GetPossibleMoves = board!.GetKingMoves;
                    break;
            }
        }

        public void Init(PieceInfo _type, Vector2i _gridPos, Board _board)
        {
            board = _board;
            Type = _type;
            GridPos = _gridPos;
        }

        public PieceInfo GetPieceColor() => GetPieceColor(this.Type);
        public PieceInfo GetPieceFigure() => GetPieceFigure(this.Type);

        public static bool IsPieceValid(PieceInfo piece) { return IsFigure(piece) || HasColor(piece); }

        public static bool IsFigure(PieceInfo piece)
        {
            return ((int)piece % 8 > 0 && ((int)piece % 8 < 7));
        }

        public static PieceInfo GetPieceFigure(PieceInfo pieceType)
        {
            if (IsFigure(pieceType) is false) { GD.PrintErr("Not a piece"); }
            return (PieceInfo)((int)pieceType % 8);
        }

        public static PieceInfo GetPieceColor(PieceInfo pieceType)
        {
            if (IsWhite(pieceType)) { return PieceInfo.White; }
            else if (IsBlack(pieceType)) { return PieceInfo.Black; }
            return PieceInfo.None;
        }

        public static bool HasColor(PieceInfo pieceType)
        {
            return IsWhite(pieceType) || IsBlack(pieceType);
        }

        public static bool IsWhite(PieceInfo pieceType)
        {
            return (pieceType & PieceInfo.White) != 0;
        }

        public static bool IsBlack(PieceInfo pieceType)
        {
            return (pieceType & PieceInfo.Black) != 0;
        }

        public static bool GetKingMoves(Vector2i gridPos)
        {
            return true;
        }
    }

    [Flags]
    public enum PieceInfo : short
    {
        None = 0,
        King = 1,
        Queen = 2,
        Bishop = 3,
        Knight = 4,
        Rook = 5,
        Pawn = 6,
        White = 8,
        Black = 16 //Can have White and Black flag :/ 
    }
}
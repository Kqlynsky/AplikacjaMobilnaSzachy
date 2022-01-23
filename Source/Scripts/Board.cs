using Godot;
using System;
using HartLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Source.Scripts
{
    //[Tool]
    public partial class Board : Node2D
    {
        public const int BoardSize = 8;
        public const int CellSize = 320;
        public const string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string GrabAction = "Grab";
        static readonly Vector2 MouseOffset = new Vector2(CellSize / 2, CellSize / 2);

        public static string CurrentFen { get; private set; } = "XD"; //TODO make fen changes 
        public static Piece? HoldingPiece { get; private set; }
        public static Piece? ClickedPiece { get; private set; }
        public static PieceInfo WhosTurn { get; private set; } = PieceInfo.White;

        static Piece?[,] boardCells = new Piece?[BoardSize, BoardSize];
        static Color lightColor = Colors.White;
        static Color darkColor = Colors.Black;
        static Color highLightMoveColor = Colors.DarkRed;
        static Color lastMoveColor = Colors.DarkRed;
        static Color pickedUpPieceColor = Colors.DarkRed;
        static List<Vector2i> positionsToDisplay = new List<Vector2i>();
        static Vector2i? lastMoveGridPos;
        static Vector2i? pickedUpPieceGridPos;
        static Vector2i? enPassantPos;

        Dictionary<char, PieceInfo> figureFromSymbol = new Dictionary<char, PieceInfo>()
        {
            ['k'] = PieceInfo.King,
            ['q'] = PieceInfo.Queen,
            ['b'] = PieceInfo.Bishop,
            ['n'] = PieceInfo.Knight,
            ['r'] = PieceInfo.Rook,
            ['p'] = PieceInfo.Pawn
        };

        Dictionary<PieceInfo, char> symbolFromFigure = new Dictionary<PieceInfo, char>()
        {
            [PieceInfo.King] = 'k',
            [PieceInfo.Queen] = 'q',
            [PieceInfo.Bishop] = 'b',
            [PieceInfo.Knight] = 'n',
            [PieceInfo.Rook] = 'r',
            [PieceInfo.Pawn] = 'p'
        };

        Dictionary<int, char> xPosToFile = new Dictionary<int, char>()
        {
            [0] = 'a',
            [1] = 'b',
            [2] = 'c',
            [3] = 'd',
            [4] = 'e',
            [5] = 'f',
            [6] = 'g',
            [7] = 'h',
        };

        Dictionary<char, bool> castleRights = new Dictionary<char, bool>()
        {
            ['K'] = true,
            ['Q'] = true,
            ['k'] = true,
            ['q'] = true
        };

        private Vector2i? LastMoveGridPos
        {
            get => lastMoveGridPos;
            set
            {
                lastMoveGridPos = value;
                Update();
            }
        }

        private Vector2i? PickedUpPieceGridPos
        {
            get => pickedUpPieceGridPos;
            set
            {
                pickedUpPieceGridPos = value;
                Update();
            }
        }

        [Export]
        Color LightColor
        {
            get => lightColor;
            set { lightColor = value; Update(); }
        }

        [Export]
        Color DarkColor
        {
            get => darkColor;
            set { darkColor = value; Update(); }
        }

        [Export]
        Color HighLightMoveColor
        {
            get => highLightMoveColor;
            set { highLightMoveColor = value; Update(); }
        }

        [Export]
        Color PickedUpPieceColor
        {
            get => pickedUpPieceColor;
            set { pickedUpPieceColor = value; Update(); }
        }

        [Export]
        Color LastMoveColor
        {
            get => lastMoveColor;
            set { lastMoveColor = value; Update(); }
        }

        public override void _Ready()
        {
            LoadPosFromFen(StartingFen);
            GD.Print(GenerateFen());
        }

        public override void _Process(float delta)
        {
            if (HoldingPiece is not null)
            {
                HoldingPiece.Position = GetGlobalMousePosition() - MouseOffset;
            }
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed(Board.GrabAction) && ClickedPiece is not null)
            {
                //ClickMove(PosToGridPos(GetGlobalMousePosition()));
            }
        }

        void AddPiece(PieceInfo pieceType, Vector2i pos)
        {
            if (IsOnBoard(pos) is false) { return; }
            if (Piece.IsPieceValid(pieceType) is false) { return; }

            var pieceScene = (PackedScene)ResourceLoader.Load("res://Source/Piece.tscn");

            Piece? newPiece = pieceScene.Instance() as Piece;
            if (newPiece is not null)
            {
                newPiece.Init(pieceType, pos, this);
                newPiece.Position = new Vector2(pos.x * CellSize, pos.y * CellSize);
                boardCells[pos.x, pos.y] = newPiece;
            }

            AddChild(newPiece);
        }

        public void GrabPiece(Piece piece)
        {
            //if (HoldingPiece is not null) { GD.PrintErr("Holding piece!"); }
            if (piece.GetPieceColor() != WhosTurn)
            {
                return;
            }

            HoldingPiece = piece;
            ClickedPiece = piece;
            pickedUpPieceGridPos = piece.GridPos;
            UpdatePositionsDisplay(HoldingPiece.GetMoves());
            boardCells[piece.GridPos.x, piece.GridPos.y] = null;
            MoveChild(piece, GetChildCount());
        }

        public void ReleasePiece(Piece piece)
        {
            if (piece != HoldingPiece) { return; }
            HoldingPiece = null;
            pickedUpPieceGridPos = null;
            TryMovePiece(piece);
        }
        public void ClickMove(Vector2i pos)
        {
            if (ClickedPiece == HoldingPiece || ClickedPiece is null) { return; }
            HoldingPiece = null;
            TryMovePiece(ClickedPiece);
        }

        public void ReturnPiece(Piece piece)
        {
            piece.Position = (piece.GridPos * CellSize).Vec2();
            boardCells[piece.GridPos.x, piece.GridPos.y] = piece;
        }

        public void TryMovePiece(Piece piece)
        {
            var mousePos = GetGlobalMousePosition();
            var gridPos = PosToGridPos(mousePos);

            if (IsOnBoard(mousePos) is false || piece.GetMoves().Contains(gridPos) is false)
            {
                ReturnPiece(piece);
                return;
            }
            var newCell = boardCells[gridPos.x, gridPos.y];

            if (newCell is not null)
            {
                if (newCell.GetPieceColor() == piece.GetPieceColor())
                {
                    ReturnPiece(piece);
                    return;
                }
                if (newCell != HoldingPiece)
                {
                    newCell.Capture();
                }
            }
            boardCells[gridPos.x, gridPos.y] = piece;
            boardCells[piece.GridPos.x, piece.GridPos.y] = null;
            LastMoveGridPos = piece.GridPos;
            piece.Move(gridPos);
            NextTurn();
        }

        public void NextTurn()
        {
            positionsToDisplay.Clear();
            WhosTurn = WhosTurn switch
            {
                PieceInfo.Black => PieceInfo.White,
                PieceInfo.White => PieceInfo.Black,
                _ => PieceInfo.None
            };
            GD.Print($"Next turn {WhosTurn}");
        }

        public Piece? PieceOnPos(Vector2i pos)
        {
            if (IsOnBoard(pos) is false) return null;
            return boardCells[pos.x, pos.y];
        }

        public bool FriendlyPieceOnPos(Vector2i pos, Piece piece)
        {
            var pieceOnPos = PieceOnPos(pos);
            if (pieceOnPos is null) return false;
            if (pieceOnPos.GetPieceColor() != piece.GetPieceColor()) { return false; }
            return true;
        }
        void LoadPosFromFen(string fen)  //"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; 
        {
            List<(PieceInfo pieceType, Vector2i gridPos)> pieceInfo = new List<(PieceInfo, Vector2i)>();
            var file = 0;
            var rank = 7;

            var splitedFen = fen.Split(' ');
            var fenPieces = splitedFen[0];
            var side = splitedFen[1][0];

            if (side is 'w')
            {
                WhosTurn = PieceInfo.White;
            }
            else if (side is 'b')
            {
                WhosTurn = PieceInfo.Black;
            }
            else
            {
                GD.PrintErr("Invalid fen player color");
                return;
            }

            foreach (var symbol in fenPieces)
            {
                bool isNextRank = symbol is '/';
                if (isNextRank)
                {
                    file = 0; rank--;
                    continue;
                }

                bool skipingEmptySpaces = char.IsDigit(symbol);
                if (skipingEmptySpaces)
                {
                    file += (int)char.GetNumericValue(symbol); continue;
                }

                if (IsOnBoard(new Vector2i(file, rank)))
                {
                    var color = char.IsUpper(symbol) ? PieceInfo.Black : PieceInfo.White;
                    var pieceSymbol = char.ToLower(symbol);
                    if (figureFromSymbol.ContainsKey(pieceSymbol) is false)
                    {
                        return;
                    }
                    pieceInfo.Add((figureFromSymbol[pieceSymbol] | color, new Vector2i(file, rank)));
                    file++;
                    continue;
                }
                GD.PrintErr("Invalid fen figures");
                return;
            }
            foreach (var info in pieceInfo)
            {
                AddPiece(info.pieceType, info.gridPos);
            }
        }

        string GenerateFen()
        {
            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";\

            //var fen = "";
            StringBuilder piecePosFen = new StringBuilder();

            for (int y = 0; y < BoardSize; y++)
            {
                var emptySpace = 0;
                for (int x = 0; x < BoardSize; x++)
                {
                    var figure = boardCells[x, y] as Piece;
                    if (figure is not null)
                    {
                        var symbol = symbolFromFigure[Piece.GetPieceFigure(figure.Type)];
                        var color = figure.GetPieceColor();
                        if (Piece.IsWhite(figure.Type))
                        {
                            symbol = char.ToUpper(symbol);
                        }
                        piecePosFen.Append(symbol);
                        emptySpace = 0;
                        continue;
                    }
                    emptySpace++;
                }
                if (emptySpace > 0)
                {
                    piecePosFen.Append(emptySpace);
                }
                piecePosFen.Append("/");
            }
            piecePosFen.Remove(piecePosFen.Length - 1, 1);

            string castling = "";

            foreach (var right in castleRights)
            {
                if (right.Value is true) { castling += right.Key; }
            }
            if (castling is "") { castling += "-"; }

            string enPassant = "-";
            if (enPassantPos is not null)
            {
                enPassant = xPosToFile[((Vector2i)enPassantPos).x].ToString();
                enPassant += ((Vector2i)enPassantPos).y.ToString();
            }

            return $"{piecePosFen.ToString()} {(char)WhosTurn} {castling} {enPassant}";
        }


        public static Vector2i PosToGridPos(Vector2 pos)
        {
            return new Vector2i(pos / CellSize);
        }

        public static bool IsOnBoard(Vector2i pos)
        {
            if (pos.x < BoardSize && pos.y < BoardSize && pos.x >= 0 && pos.y >= 0) { return true; }
            //GD.PrintErr("Invalid pos");
            return false;
        }
        public static bool IsOnBoard(Vector2 pos)
        {
            if (pos.x < BoardSize * CellSize && pos.y < BoardSize * CellSize && pos.x >= 0 && pos.y >= 0) { return true; }
            //GD.PrintErr("Invalid pos");
            return false;
        }

        public void UpdatePositionsDisplay(List<Vector2i> positions)
        {
            positionsToDisplay = positions;
            Update();
        }

        public void ClearPositionsDisplay()
        {
            positionsToDisplay.Clear();
            Update();
        }

        public override void _Draw()
        {
            GenerateBoard();
            if (lastMoveGridPos is not null)
            {
                DrawRect(new Rect2(new Vector2(lastMoveGridPos.Value.x * CellSize, lastMoveGridPos.Value.y * CellSize), new Vector2(CellSize, CellSize)), lastMoveColor);
            }
            foreach (var pos in positionsToDisplay)
            {
                DrawRect(new Rect2(new Vector2(pos.x * CellSize, pos.y * CellSize), new Vector2(CellSize, CellSize)), highLightMoveColor);
            }
            if (pickedUpPieceGridPos is not null)
            {
                DrawRect(new Rect2(new Vector2(pickedUpPieceGridPos.Value.x * CellSize, pickedUpPieceGridPos.Value.y * CellSize), new Vector2(CellSize, CellSize)), pickedUpPieceColor);
            }
        }

        private void GenerateBoard()
        {
            for (int file = 0; file < BoardSize; file++)
            {
                for (int rank = 0; rank < BoardSize; rank++)
                {
                    bool isWhite = (file + rank) % 2 == 0;
                    Color cellColor = DarkColor;
                    if (isWhite) { cellColor = LightColor; }

                    DrawRect(new Rect2(new Vector2(file * CellSize, rank * CellSize), new Vector2(CellSize, CellSize)), cellColor);
                }
            }
        }


        //public enum PlayerColor { White = 'w', Black = 'b' }
    }
}
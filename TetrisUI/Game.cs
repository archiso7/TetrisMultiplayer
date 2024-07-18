using static TetrisCore.Types;
using static TetrisCore.GameLogic;
using Microsoft.FSharp.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.Memory;

namespace TetrisUI
{
    public class Game : Screen
    {
        private bool holdUsed = false;
        private System.Timers.Timer lockTimer;
        private const int LockDelay = 500; // Lock delay in milliseconds
        private bool isPieceLanded;
        private System.Timers.Timer dropTimer;
        private const int DropInterval = 1000;
        public List<GameObject> objects { get; set; }
        private GameState gameState;
        private Board board;
        private Board bagBoard;
        private Board holdBoard;
        private KeyBindingManager keyBindingManager;
        private TetrisPiece lastHoldPiece;
        private TetrisPiece lastQueuePiece;
        private TetrisPiece ghostPiece;

        public Game()
        {
            // Initialize the game state
            keyBindingManager = new KeyBindingManager();
            // Bind keys to actions
            keyBindingManager.BindKey(Keys.Left, GameAction.MoveLeft);
            keyBindingManager.BindKey(Keys.Right, GameAction.MoveRight);
            keyBindingManager.BindKey(Keys.Down, GameAction.MoveDown);
            keyBindingManager.BindKey(Keys.X, GameAction.RotateC);
            keyBindingManager.BindKey(Keys.Z, GameAction.RotateCC);
            keyBindingManager.BindKey(Keys.C, GameAction.Hold);
            keyBindingManager.BindKey(Keys.Space, GameAction.Drop);
            keyBindingManager.BindKey(Keys.Escape, GameAction.Pause);

            // Bind actions to handlers
            keyBindingManager.BindAction(GameAction.MoveLeft, () => Move(-1,0));
            keyBindingManager.BindAction(GameAction.MoveRight, () => Move(1,0));
            keyBindingManager.BindAction(GameAction.MoveDown, () => Move(0,1));
            keyBindingManager.BindAction(GameAction.RotateC, () => Rotate(-1));
            keyBindingManager.BindAction(GameAction.RotateCC, () => Rotate(1));
            keyBindingManager.BindAction(GameAction.Hold, Hold);
            keyBindingManager.BindAction(GameAction.Drop, Drop);
            keyBindingManager.BindAction(GameAction.Pause, Pause);

            objects = new List<GameObject>();

            dropTimer = new System.Timers.Timer(DropInterval);
            dropTimer.Elapsed += (sender, e) => Move(0, 1);

            lockTimer = new System.Timers.Timer(LockDelay);
            lockTimer.Elapsed += (sender, e) => LockPiece();

            Reset();
        }

        public void HandleInput(KeyboardKeyEventArgs e)
        {
            Keys k = e.Key;
            keyBindingManager.HandleKeyPress(k);
        }

        private void LockPiece()
        {
            lockTimer.Stop();
            isPieceLanded = false;
            holdUsed = false;
            gameState = nextPiece(board.GetTileColors(), gameState, board.BlankCount);
            int[][] cleanBoard = placePiece(Utils.FSharpListToArray(gameState.Board), gameState.CurrentPiece, true, board.BlankCount);
            ghostPiece = updateGhostPiece(cleanBoard, gameState.CurrentPiece);
            cleanBoard = placePiece(cleanBoard, gameState.CurrentPiece, false, board.BlankCount);
            UpdatePB(Utils.FSharpListToArray(gameState.Board), gameState.CurrentPiece);
        }

        private void UpdatePB (int[][] b, TetrisPiece piece)
        {
            if(onGround(b, piece, board.BlankCount))
            {
                if (!isPieceLanded)
                {
                    isPieceLanded = true;
                    lockTimer.Start();
                }
            }
            else
            {
                isPieceLanded = false;
                lockTimer.Stop();
            }

            gameState.CurrentPiece = piece;
            board.SetTileColors(b);
            gameState.Board = Utils.ToFSharpList(b);

            UpdateGhost();
        }

        private void Move(int xDir, int yDir)
        {
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = board.GetTileColors();
            (_, b, piece) = movePiece(b, piece, xDir, yDir, board.BlankCount);

            UpdatePB(b, piece);
        }

        private void Rotate(int dir)
        {
            int direction =  dir > 0 ? dir : 3;
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = board.GetTileColors();
            (_, b, piece) = rotatePiece(b, piece, direction, board.BlankCount);
            UpdatePB(b, piece);
        }

        private void Hold()
        {
            if(!holdUsed)
            {
                gameState = holdPiece(gameState, board.BlankCount);
                holdUsed = true;
                UpdatePB(Utils.FSharpListToArray(gameState.Board), gameState.CurrentPiece);
            }
        }

        private void Drop()
        {
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = board.GetTileColors();
            bool canMove = true;
            while (canMove)
            {
                (canMove, b, piece) = movePiece(b, piece, 0, 1, board.BlankCount);
            }

            UpdatePB(b, piece);
            LockPiece();
        }

        private void Pause()
        {

        }

        public void Update()
        {
            // Update holdBoard only if the hold piece has changed
            if (FSharpOption<TetrisPiece>.get_IsSome(gameState.Hold) && gameState.Hold.Value != lastHoldPiece)
            {
                holdBoard.clear();
                var holdPiece = gameState.Hold.Value;
                holdBoard.SetTileColors(placePiece(holdBoard.GetTileColors(), holdPiece, false, holdBoard.BlankCount));
                lastHoldPiece = holdPiece;
            }
            
            if((lastQueuePiece == null) || (gameState.Queue.Head != lastQueuePiece))
            {
                bagBoard.clear();
                int[][] colors = bagBoard.GetTileColors();
                for(int i = 0; i < gameState.Queue.Length; i++)
                {
                    TetrisPiece qPiece = gameState.Queue[i];
                    TetrisPiece piece = new TetrisPiece(qPiece.Shape, qPiece.Table, qPiece.Rotation, Tuple.Create(1,1+(i*4)));
                    colors = placePiece(colors, piece, false, bagBoard.BlankCount);
                }
                bagBoard.SetTileColors(colors);
                lastQueuePiece = gameState.Queue.Head;
            }

            int[][] clearBoard = CheckClear(board.GetTileColors());
            // board.SetTileColors(clearBoard);
            // gameState.Board = Utils.ToFSharpList(clearBoard);
        }

        public int[][] CheckClear(int[][] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                int[] row = b[i];
                bool clear = true;
                foreach(int tile in row)
                {
                    // Console.Write(tile);
                    if(tile <= 0)
                    {
                        clear = false;
                    }
                }
                // Console.WriteLine();
                if(clear)
                {
                    Console.WriteLine("clearing");
                    b = LineClear(b, i);
                }
            }
            return b;
        }

        public int[][] LineClear(int[][] b, int rowToRemove)
        {
            b = Utils.RemoveAt(b, rowToRemove);

            int[] newRow = new int[b[0].Length];
            for (int i = 0; i < newRow.Length; i++)
            {
                newRow[i] = rowToRemove < board.BlankCount ? (int)Board.ConvertColor(Color4.Black) : (int)Board.ConvertColor(Color4.LightGray);
            }

            List<int[]> updatedBoard = b.ToList();
            updatedBoard.Insert(0, newRow);
            return updatedBoard.ToArray();
        }

        public void UpdateGhost()
        {
            int[][] cleanBoard = placePiece(board.GetTileColors(), gameState.CurrentPiece, true, board.BlankCount);
            cleanBoard = placePiece(cleanBoard, ghostPiece, true, board.BlankCount);
            ghostPiece = updateGhostPiece(cleanBoard, gameState.CurrentPiece);
            int[][] ghostBoard = placePiece(cleanBoard, ghostPiece, false, board.BlankCount);

            // Lighten the color of the ghost piece
            for (int x = 0; x < ghostPiece.Table.Length; x++)
            {
                for (int y = 0; y < ghostPiece.Table[x].Length; y++)
                {
                    int xPos = x + ghostPiece.Position.Item1;
                    int yPos = y + ghostPiece.Position.Item2;

                    if (ghostPiece.Table[y][x] > 0)
                    {
                        if(ghostBoard[xPos][yPos] > 0)
                        {
                            ghostBoard[xPos][yPos] = -2; // Assuming -2 is a special code for ghost piece color
                        }
                    }
                }
            }

            board.SetTileColors(placePiece(ghostBoard, gameState.CurrentPiece, false, board.BlankCount));
        }

        public void Reset()
        {
            gameState = initialState();
            // Define the size of each tile and the size of the board
            OpenTK.Mathematics.Vector2 tileSize = new OpenTK.Mathematics.Vector2(40, 40);
            int boardWidth = gameState.Board.Length;
            int boardHeight = gameState.Board[0].Length;

            // Create the board object with the initialized tiles
            board = new Board(new OpenTK.Mathematics.Vector2(boardWidth, boardHeight), tileSize, new OpenTK.Mathematics.Vector2(0, -130), 4);
            objects.Add(board);
            board.SetTileColors(placePiece(board.GetTileColors(), gameState.CurrentPiece, false, board.BlankCount));
            gameState.Board = Utils.ToFSharpList(board.GetTileColors());

            holdBoard = new Board(new OpenTK.Mathematics.Vector2(6,4), tileSize*0.75f, new OpenTK.Mathematics.Vector2(-370, 185), 0);
            objects.Add(holdBoard);
            
            bagBoard = new Board(new OpenTK.Mathematics.Vector2(6,20), tileSize*0.75f, new OpenTK.Mathematics.Vector2(370, -95), 0);
            objects.Add(bagBoard);

            lastQueuePiece = null;

            ghostPiece = createPiece(gameState.CurrentPiece.Shape, gameState.CurrentPiece.Position.Item1, 0);
            UpdateGhost();
            
        }

        public void Start()
        {
            dropTimer.Start();
        }
    }

    public class Board : GameObject
    {
        private Rectangle[][] Tiles;
        private OpenTK.Mathematics.Vector2 Size;
        private OpenTK.Mathematics.Vector2 Position;
        private OpenTK.Mathematics.Vector2 TileSize;
        public int BlankCount;

        public Board(OpenTK.Mathematics.Vector2 size, OpenTK.Mathematics.Vector2 tileSize, OpenTK.Mathematics.Vector2 position, int blankCount)
        {
            Size = size;
            TileSize = tileSize;
            BlankCount = blankCount;
            Tiles = GenTiles();
            Position = position;
        }

        public Rectangle[][] GenTiles()
        {
            int boarderSize = 5;
            float boardPixelHeight = (int)Size.Y * (TileSize.Y + boarderSize);
            float boardPixelWidth = (int)Size.X * (TileSize.X + boarderSize);

            Rectangle[][] tiles = new Rectangle[(int)Size.X][];
            for (int x = 0; x < (int)Size.X; x++)
            {
                tiles[x] = new Rectangle[(int)Size.Y];
                for (int y = 0; y < (int)Size.Y; y++)
                {
                    // Calculate the position of each tile based on its size and board position
                    OpenTK.Mathematics.Vector2 position = new OpenTK.Mathematics.Vector2((x * (TileSize.X + boarderSize)) - (boardPixelWidth / 2f), (y * -(TileSize.Y + boarderSize)) + (boardPixelHeight / 2f));
                    // Determine the color of the tile based on the game state
                    Color4 color = y >= BlankCount - 1 ? (Color4)ConvertColor(0) : (Color4)ConvertColor(-1);

                    // Create a new rectangle for the tile
                    tiles[x][y] = new Rectangle(TileSize, position, color);
                }
            }

            return tiles;
        }

        public void clear()
        {
            Tiles = GenTiles();
        }

        public void Render(int _shaderProgram, OpenTK.Mathematics.Vector2? scale = null, OpenTK.Mathematics.Vector2? offset = null)
        {
            OpenTK.Mathematics.Vector2 Offset = offset ?? new OpenTK.Mathematics.Vector2(0, 0);
            Offset += Position;

            foreach (Rectangle[] rows in Tiles)
            {
                foreach (Rectangle tile in rows)
                {
                    tile.Render(_shaderProgram, scale, Offset);
                }
            }
        }

        public static object ConvertColor(object input)
        {
            return input switch
            {
                Color4 color => color switch
                {
                    _ when color == Color4.Gray => -2,
                    _ when color == Color4.Black => -1,
                    _ when color == Color4.Cyan => 1,
                    _ when color == Color4.Yellow => 2,
                    _ when color == Color4.Purple => 3,
                    _ when color == Color4.Green => 4,
                    _ when color == Color4.Red => 5,
                    _ when color == Color4.Orange => 6,
                    _ when color == Color4.Blue => 7,
                    _ => 0
                },
                int intValue => intValue switch
                {
                    -2 => Color4.Gray,
                    -1 => Color4.Black,
                    1 => Color4.Cyan,
                    2 => Color4.Yellow,
                    3 => Color4.Purple,
                    4 => Color4.Green,
                    5 => Color4.Red,
                    6 => Color4.Orange,
                    7 => Color4.Blue,
                    _ => Color4.LightGray
                },
                _ => throw new ArgumentException("Input must be either Color4 or int")
            };
        }

        public int[][] GetTileColors()
        {
            int[][] colors = new int[Tiles.Length][];
            for (int x = 0; x < Tiles.Length; x++)
            {
                colors[x] = new int[Tiles[x].Length];
                for (int y = 0; y < Tiles[x].Length; y++)
                {
                    colors[x][y] = (int)ConvertColor(Tiles[x][y].Color);
                }
            }
            return colors;
        }

        public void SetTileColors(int[][] colorValues)
        {
            for (int x = 0; x < Tiles.Length; x++)
            {
                for (int y = 0; y < Tiles[x].Length; y++)
                {
                    Tiles[x][y].Color = (Color4)ConvertColor(colorValues[x][y]);
                }
            }
        }
    }
}

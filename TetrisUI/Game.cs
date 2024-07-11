using static TetrisCore.Types;
using static TetrisCore.GameLogic;
using Microsoft.FSharp.Collections;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Drawing;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Timers;

namespace TetrisUI
{
    public class Game : Screen
    {
        private System.Timers.Timer lockTimer;
        private const int LockDelay = 500; // Lock delay in milliseconds
        private bool isPieceLanded;
        private System.Timers.Timer dropTimer;
        private const int DropInterval = 1000;
        public List<GameObject> objects { get; set; }
        private GameState gameState;
        private Board board;
        private KeyBindingManager keyBindingManager;

        public Game()
        {
            // Initialize the game state
            gameState = initialState();
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

            board = new Board(new OpenTK.Mathematics.Vector2(0,0), new Rectangle[][]{}, new OpenTK.Mathematics.Vector2(0,0));
        }

        public void HandleInput(KeyboardKeyEventArgs e)
        {
            Keys k = e.Key;
            if(!e.IsRepeat) // checks if key is first press or hold, could maybe pass this into a binding system insted idk
            {
                keyBindingManager.HandleKeyPress(k);
            }
        }

        private void LockPiece()
        {
            lockTimer.Stop();
            isPieceLanded = false;
            gameState = nextPiece(board.GetTileColors(), gameState);
            board.SetTileColors(Utils.FSharpListToArray(gameState.Board));
        }

        private void UpdatePB (int[][] b, TetrisPiece piece)
        {
            gameState.CurrentPiece = piece;
            board.SetTileColors(b);
            gameState.Board = Utils.ToFSharpList(b);
        }

        private void Move(int xDir, int yDir)
        {
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = board.GetTileColors();
            (b, piece) = movePiece(b, piece, xDir, yDir);
            if(onGround(b, piece))
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

            UpdatePB(b, piece);
        }

        private void Rotate(int dir)
        {
            int direcetion =  dir > 0 ? dir : 3;
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = board.GetTileColors();
            (b, piece) = rotatePiece(b, piece, direcetion);
            gameState.CurrentPiece = piece;
            board.SetTileColors(b);
            gameState.Board = Utils.ToFSharpList(b);
        }

        private void Hold()
        {

        }

        private void Drop()
        {

        }

        private void Pause()
        {

        }

        public void Update()
        {
            // Update game state using F# logic
            // gameState = GameLogic.updateGameState(gameState);
        }

        public void Start()
        {
            // Define the size of each tile and the size of the board
            OpenTK.Mathematics.Vector2 tileSize = new OpenTK.Mathematics.Vector2(40, 40);
            int boarderSize = 5;
            int boardWidth = gameState.Board.Length;
            int boardHeight = gameState.Board[0].Length;
            float boardPixelHeight = boardHeight * (tileSize.Y + boarderSize);
            float boardPixelWidth = boardWidth * (tileSize.X + boarderSize);

            // Initialize the tiles array
            Rectangle[][] tiles = new Rectangle[boardWidth][];
            for (int x = 0; x < boardWidth; x++)
            {
                tiles[x] = new Rectangle[boardHeight];
                for (int y = 0; y < boardHeight; y++)
                {
                    // Calculate the position of each tile based on its size and board position
                    OpenTK.Mathematics.Vector2 position = new OpenTK.Mathematics.Vector2((x * (tileSize.X + boarderSize)) - (boardPixelWidth / 2f), (y * -(tileSize.Y + boarderSize)) + (boardPixelHeight / 2f));
                    // Determine the color of the tile based on the game state
                    Color4 color = (Color4)Board.ConvertColor(gameState.Board[x][y]);

                    // Create a new rectangle for the tile
                    tiles[x][y] = new Rectangle(tileSize, position, color);
                }
            }

            // Create the board object with the initialized tiles
            board = new Board(new OpenTK.Mathematics.Vector2(boardWidth, boardHeight), tiles, new OpenTK.Mathematics.Vector2(0, -50));
            objects.Add(board);
            board.SetTileColors(placePiece(board.GetTileColors(), gameState.CurrentPiece, false));
            gameState.Board = Utils.ToFSharpList(board.GetTileColors());

            dropTimer.Start();
        }
    }

    public class Board : GameObject
    {
        private Rectangle[][] Tiles;
        private OpenTK.Mathematics.Vector2 Size;
        private OpenTK.Mathematics.Vector2 Position;

        public Board(OpenTK.Mathematics.Vector2 size, Rectangle[][] tiles, OpenTK.Mathematics.Vector2 position)
        {
            Tiles = tiles;
            Size = size;
            Position = position;
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

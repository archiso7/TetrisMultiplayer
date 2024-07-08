using static TetrisCore.Types;
using static TetrisCore.GameLogic;
using Microsoft.FSharp.Collections;
using OpenTK.Mathematics;
using System.Drawing;

namespace TetrisUI
{
    public class Game : Screen
    {
        public List<GameObject> objects { get; set; }
        private GameState gameState;
        private Board board;

        public Game()
        {
            // Initialize the game state
            gameState = initialState();
            objects = new List<GameObject>();

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

            var piece = createPiece(Shape.J, 0,0);
            var piece1 = new TetrisPiece(Shape.J, rotateMatrix(piece.Table, 1), 1, new Tuple<int,int>(0,0));
            var piece2 = createPiece(Shape.I, 0,1);

            int[][] boardColor;

            board.SetTileColors(placePiece(board.GetTileColors(), piece2, false));

            var collide = checkCollision(board.GetTileColors(), piece1);

            Console.WriteLine(collide);

            ( boardColor, piece ) = rotatePiece(placePiece(board.GetTileColors(), piece, false), piece, 1);

            board.SetTileColors(boardColor);
        }

        public void HandleInput()
        {

        }

        public void Update()
        {
            // Update game state using F# logic
            // gameState = GameLogic.updateGameState(gameState);
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

using OpenTK.Mathematics;

namespace TetrisUI
{
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

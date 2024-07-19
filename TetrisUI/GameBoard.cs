using OpenTK.Mathematics;
using static TetrisCore.Types;
using static TetrisCore.GameLogic;

namespace TetrisUI
{
    public class GameBoard
    {
        public GameState gameState;
        public Board MainBoard { get; private set; }
        public Board HoldBoard { get; private set; }
        public Board BagBoard { get; private set; }

        public bool HoldUsed { get; set; }
        public bool IsPieceLanded { get; set; }

        public GameBoard(GameState initialState, OpenTK.Mathematics.Vector2 tileSize, OpenTK.Mathematics.Vector2 boardSize, OpenTK.Mathematics.Vector2 position)
        {
            gameState = initialState;
            MainBoard = new Board(boardSize, tileSize, new OpenTK.Mathematics.Vector2(0, -130), 4);
            HoldBoard = new Board(new OpenTK.Mathematics.Vector2(6,4), tileSize*0.75f, new OpenTK.Mathematics.Vector2(-370, 185), 0);
            BagBoard = new Board(new OpenTK.Mathematics.Vector2(6,20), tileSize*0.75f, new OpenTK.Mathematics.Vector2(370, -95), 0);
        }

        public void Move(int xDir, int yDir)
        {
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = MainBoard.GetTileColors();
            gameState.CurrentPiece = movePiece(b, piece, xDir, yDir); //update this function to just return a TetrisPiece with updated location
            UpdatePB();

        }

        public void Rotate(int dir)
        {
            int direction =  dir > 0 ? dir : 3;
            TetrisPiece piece = gameState.CurrentPiece;
            int[][] b = MainBoard.GetTileColors();
            gameState.CurrentPiece = rotatePiece(b, piece, direction); //update this function to just return a TetrisPiece with updated rotation
            UpdatePB();
        }

        public void Drop()
        {
            //needs updated f# function
        }

        public void Hold()
        {
            if(!HoldUsed)
            {
                gameState = holdPiece(gameState, MainBoard.BlankCount);
                HoldUsed = true;
            }
        }

        public void Lock()
        {
            gameState = nextPiece(MainBoard.GetTileColors(), gameState);
        }

        private void UpdatePB ()
        {
            
        }
    }
}
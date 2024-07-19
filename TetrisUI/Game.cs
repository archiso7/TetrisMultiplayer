using static TetrisCore.Types;
using static TetrisCore.GameLogic;
using Microsoft.FSharp.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TetrisUI
{
    public class Game : Screen
    {
        private GameBoard playerBoard;
        private System.Timers.Timer lockTimer;
        private const int LockDelay = 500; // Lock delay in milliseconds
        private bool isPieceLanded;
        private System.Timers.Timer dropTimer;
        private const int DropInterval = 1000;
        public List<GameObject> objects { get; set; }
        private KeyBindingManager keyBindingManager;

        public Game()
        {
            playerBoard = new GameBoard(initialState(), new OpenTK.Mathematics.Vector2(40,40), new OpenTK.Mathematics.Vector2(10, 24), new OpenTK.Mathematics.Vector2(0,0));
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
            keyBindingManager.BindAction(GameAction.MoveLeft, () => playerBoard.Move(-1,0));
            keyBindingManager.BindAction(GameAction.MoveRight, () => playerBoard.Move(1,0));
            keyBindingManager.BindAction(GameAction.MoveDown, () => playerBoard.Move(0,1));
            keyBindingManager.BindAction(GameAction.RotateC, () => playerBoard.Rotate(-1));
            keyBindingManager.BindAction(GameAction.RotateCC, () => playerBoard.Rotate(1));
            keyBindingManager.BindAction(GameAction.Hold, playerBoard.Hold);
            keyBindingManager.BindAction(GameAction.Drop, playerBoard.Drop);
            keyBindingManager.BindAction(GameAction.Pause, Pause);

            objects = new List<GameObject>();

            dropTimer = new System.Timers.Timer(DropInterval);
            dropTimer.Elapsed += (sender, e) => playerBoard.Move(0, 1);

            lockTimer = new System.Timers.Timer(LockDelay);
            lockTimer.Elapsed += (sender, e) => LockPiece();
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
            playerBoard.HoldUsed = false;
            playerBoard.Lock();
        }

        private void UpdatePB (int[][] b, TetrisPiece piece)
        {
            if(onGround(b, piece, playerBoard.MainBoard.BlankCount))
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

            playerBoard.gameState.CurrentPiece = piece;
            playerBoard.MainBoard.SetTileColors(b);
            playerBoard.gameState.Board = Utils.ToFSharpList(b);
        }

        private void Pause()
        {

        }

        public void Update()
        {
            
        }

        public void Start()
        {
            dropTimer.Start();
        }
    }
}
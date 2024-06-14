using System;
using System.Collections.Generic;
using System.Linq;
using static TetrisCore.Types;
using static TetrisCore.GameLogic;
using Microsoft.FSharp.Collections;

namespace TetrisUI
{
    public class Game : Screen
    {
        public List<GameObject> objects {get;set;}
        private GameState gameState;

        public Game()
        {
            // Initialize the game state
            gameState = initialState();
            objects = new List<GameObject>();
        }

        public void HandleInput()
        {

        }

        public void Update()
        {
            // Update game state using F# logic
            // gameState = GameLogic.updateGameState(gameState);
        }

        public void Render()
        {
            // Render game using current gameState
        }
    }
}

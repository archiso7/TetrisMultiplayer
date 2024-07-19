namespace TetrisUI
{
    public enum State
    {
        Playing,
        Paused,
        GameOver
    }
    public class GameStateMachine
    {
        private State currentState;
        private GameBoard gameBoard;

        public GameStateMachine(GameBoard board)
        {
            gameBoard = board;
            currentState = State.Playing;
        }

        public State CurrentState
        {
            get { return currentState; }
        }

        public void SetState(State newState)
        {
            if (currentState == newState) return;

            ExitState(currentState);
            EnterState(newState);
            currentState = newState;
        }

        private void EnterState(State state)
        {
            switch (state)
            {
                case State.Playing:
                    break;
                case State.Paused:
                    break;
                case State.GameOver:
                    break;
            }
        }

        private void ExitState(State state)
        {
            switch (state)
            {
                case State.Playing:
                    break;
                case State.Paused:
                    // No specific actions required
                    break;
                case State.GameOver:
                    // No specific actions required
                    break;
            }
        }
    }
}
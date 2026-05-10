namespace Sokoban.Managers.States
{
    public class SolvingState : IGameState
    {
        public void EnterState(GameManager manager)
        {
            // Tắt input của người chơi để robot tự giải
            manager.IsInputEnabled = false;
        }

        public void UpdateState(GameManager manager) {}
        public void ExitState(GameManager manager) {}
    }
}
namespace Sokoban.Managers.States
{
    public class HintingState : IGameState
    {
        public void EnterState(GameManager manager)
        {
            // Tắt input của người chơi trong lúc đang chạy gợi ý
            manager.IsInputEnabled = false;
        }

        public void UpdateState(GameManager manager) {}
        public void ExitState(GameManager manager) {}
    }
}
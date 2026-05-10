namespace Sokoban.Managers.States
{
    public class PlayingState : IGameState
    {
        public void EnterState(GameManager manager)
        {
            manager.IsInputEnabled = true;
        }

        public void UpdateState(GameManager manager)
        {
            // Các update logic hàng khung hình (nếu có)
        }

        public void ExitState(GameManager manager)
        {
            manager.IsInputEnabled = false;
        }
    }
}
namespace Sokoban.Managers.States
{
    public class WinState : IGameState
    {
        public void EnterState(GameManager manager)
        {
            manager.IsInputEnabled = false;
            manager.TriggerWinSequence();
        }

        public void UpdateState(GameManager manager) {}
        public void ExitState(GameManager manager) {}
    }
}
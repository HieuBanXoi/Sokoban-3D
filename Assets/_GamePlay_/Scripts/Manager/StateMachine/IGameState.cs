namespace Sokoban.Managers.States
{
    public interface IGameState
    {
        void EnterState(GameManager manager);
        void UpdateState(GameManager manager);
        void ExitState(GameManager manager);
    }
}
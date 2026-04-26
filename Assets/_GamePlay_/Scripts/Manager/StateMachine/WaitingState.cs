using UnityEngine;

public class WaitingState : IGameState
{
    public void OnEnter(GameManager gameManager)
    {
        UIManager.Ins.ActiveTutorialUI(true);
        gameManager.isPlaying = true;
    }

    public void OnExecute(GameManager gameManager)
    {

    }

    public void OnExit(GameManager gameManager)
    {
        UIManager.Ins.ActiveTutorialUI(false);
    }
}

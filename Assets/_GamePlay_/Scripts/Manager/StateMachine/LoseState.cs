using UnityEngine;

public class LoseState : IGameState
{
    public void OnEnter(GameManager gameManager)
    {
        UIManager.Ins.ActiveGameLoseUI(true);
        gameManager.isGotoStore = true;

    }

    public void OnExecute(GameManager gameManager)
    {

    }

    public void OnExit(GameManager gameManager)
    {
    }
}

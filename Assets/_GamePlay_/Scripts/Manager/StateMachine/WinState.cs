using UnityEngine;

public class WinState : IGameState
{
    public void OnEnter(GameManager gameManager)
    {
        gameManager.isGotoStore = true;
    }

    public void OnExecute(GameManager gameManager)
    {

    }

    public void OnExit(GameManager gameManager)
    {
    }
}

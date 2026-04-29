using UnityEngine;

public class LoseState : IGameState
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

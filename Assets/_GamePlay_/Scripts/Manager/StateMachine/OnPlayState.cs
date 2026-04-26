using UnityEngine;

public class OnPlayState : IGameState
{
    public void OnEnter(GameManager gameManager)
    {
        gameManager.isPlaying = true;
    }

    public void OnExecute(GameManager gameManager)
    {

    }

    public void OnExit(GameManager gameManager)
    {
        gameManager.isPlaying = false;
    }
}

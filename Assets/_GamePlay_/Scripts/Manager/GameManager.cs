using UnityEngine;
using System;
public class GameManager : Ply_Singleton<GameManager>
{
    private IGameState currentState;
    public bool isPlaying = false;
    public bool isTutorial = true;
    public bool isGotoStore = false;
    public bool isLose = false;
    public Player player;
    public CameraFollow cameraFollow;

    // private void Start()
    // {
    //     LoadNewMap();
    // }
    public void LoadNewMap()
    {
        InputManager.Ins.player = player;
        cameraFollow.target = player.transform;
        ChangeState(new OnPlayState());
    }
    private void Update()
    {
        if (currentState != null)
        {
            currentState.OnExecute(this);
        }
    }
    public void ChangeState(IGameState newState)
    {
        currentState?.OnExit(this);

        currentState = newState;

        currentState?.OnEnter(this);
        Debug.Log("ChangeState: "+ currentState.ToString());
    }
    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void TurnOffTut()
    {
        if (isTutorial)
        {
            isTutorial= false;
        }
    }
}

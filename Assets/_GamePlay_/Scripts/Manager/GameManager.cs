using UnityEngine;
using System;
public class GameManager : Ply_Singleton<GameManager>
{
    public enum GameState
    {
        Waiting,
        Playing,
        Hinting,
        Solving,
        Win,
        Lose
    }

    public GameState State { get; private set; } = GameState.Playing;

    // Compatibility properties
    public bool IsInputEnabled => State == GameState.Playing || State == GameState.Waiting;
    public bool IsGotoStore => State == GameState.Win || State == GameState.Lose;

    public Player player;
    public CameraFollow cameraFollow;

    private void Start()
    {
        SetState(GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;
        // You can add centralized side-effects here if needed later
    }
    
    // Hint mode flags centralized here
    public bool IsHintMode { get; set; } = false; // user requested mode (play entire solution)
    public bool IsHintModeActive { get; set; } = false; // currently running hint playback

    public void LoadNewMap()
    {
        cameraFollow.target = player.TF;
    }

}

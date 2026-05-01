using UnityEngine;
using System;
public class GameManager : Ply_Singleton<GameManager>
{
    public bool isPlaying = false;
    public bool isGotoStore = false;

    public Player player;
    public CameraFollow cameraFollow;

    private void Start()
    {
        isPlaying = true;
    }
    public void LoadNewMap()
    {
        InputManager.Ins.player = player;
        cameraFollow.target = player.transform;
    }

}

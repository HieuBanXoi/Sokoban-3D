using UnityEngine;

public class Player : Ply_GameUnit
{
    [Header("Reference")]
    public PlayerGraphicController graphic;
    public PlayerMovement movement;

    public void Despawn()
    {
        Ply_Pool.Ins.Despawn(PoolType.Player, this);
    }

}

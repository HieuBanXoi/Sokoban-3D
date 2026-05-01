using UnityEngine;

public class Player : Ply_GameUnit
{
    [Header("Reference")]
    public PlayerGraphicController graphic;
    public PlayerMovement movement;

    public void Despawn()
    {
        movement.animator.SetBool("isCheering", false);
        Ply_Pool.Ins.Despawn(PoolType.Player, this);
    }

}

using UnityEngine;

public class PlayerGraphicController : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public void ApplySkin()
    {
        playerMovement.animator = SkinManager.Ins.ApplyPlayerSkin(this.transform);
    }
}

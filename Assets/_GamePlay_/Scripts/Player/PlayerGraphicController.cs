using UnityEngine;

public class PlayerGraphicController : MonoBehaviour
{
    public PlayerMovement playerMovement;
    private Transform _tf;
    private void Awake()
    {
        _tf = transform;
    }
    public void ApplySkin()
    {
        playerMovement.animator = SkinManager.Ins.ApplyPlayerSkin(_tf);
    }
}

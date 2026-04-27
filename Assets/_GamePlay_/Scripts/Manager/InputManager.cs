using UnityEngine;

public class InputManager : Ply_Singleton<InputManager>
{
    [Header("Reference")]
    public Player player;
    [Header("Push Settings")]
    public LayerMask boxLayerMask;
    public LayerMask groundLayerMask;


    private void Update()
    {
        if (!GameManager.Ins.isPlaying) return;
        if (player == null || player.movement == null) return;

        // Xử lý Input trên máy tính (Bàn phím)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            MoveUp();
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            MoveDown();
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLeft();
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight();
    }

    // CÁC HÀM NÀY DÙNG ĐỂ GẮN VÀO BUTTON TRÊN ĐIỆN THOẠI (UI)
    public void MoveUp() => player.movement.AttemptMove(Vector3.forward);
    public void MoveDown() => player.movement.AttemptMove(Vector3.back);
    public void MoveLeft() => player.movement.AttemptMove(Vector3.left);
    public void MoveRight() => player.movement.AttemptMove(Vector3.right);
}

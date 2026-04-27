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
        if (!GameManager.Ins.isPlaying)
        {
            return;
        }
        if (player == null || player.movement == null) return;

        // 1. Nhận sự kiện di chuyển (WASD hoặc Phím mũi tên)
        // GetAxisRaw trả về các giá trị -1, 0, 1 giúp nhân vật dừng lại ngay lập tức khi nhả phím
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement: prioritize larger axis
        if (Mathf.Abs(horizontal) > 0f && Mathf.Abs(vertical) > 0f)
        {
            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
                vertical = 0f;
            else
                horizontal = 0f;
        }

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Luôn truyền Vector di chuyển xuống, kể cả khi đứng yên (Vector3.zero)
        player.movement.Move(moveDirection);

        // Handle push input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.movement.HandlePushInput();
        }

    }
}

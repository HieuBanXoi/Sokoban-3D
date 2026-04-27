using UnityEngine;

public class FakeGravity : MonoBehaviour
{
    [Header("Cài đặt Trọng lực")]
    public float gravity = -9.81f; // Gia tốc trọng trường chuẩn
    private float verticalVelocity = 0f;

    [Header("Cài đặt Mặt đất")]
    public LayerMask groundLayer; // Chọn Layer của mặt đất trên Inspector
    public float groundCheckDistance = 0.5f; // Khoảng cách từ tâm vật thể đến đáy của nó
    
    private bool isGrounded = false;

    void OnEnable()
    {
        // Reset trạng thái khi vật thể được kích hoạt lại
        isGrounded = false;
        verticalVelocity = 0f;
    }
    void Update()
    {
        // Nếu chưa chạm đất thì tiếp tục rơi
        if (!isGrounded)
        {
            ApplyGravity();
            CheckGround();
        }
    }

    void ApplyGravity()
    {
        // Vận tốc tăng dần theo thời gian (gia tốc)
        verticalVelocity += gravity * Time.deltaTime;
        
        // Di chuyển vật thể xuống dưới
        transform.position += new Vector3(0, verticalVelocity * Time.deltaTime, 0);
    }

    void CheckGround()
    {
        // Bắn một tia (Raycast) từ tâm vật thể hướng thẳng xuống dưới
        // Tia này dài bằng groundCheckDistance và chỉ bắt va chạm với groundLayer
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            verticalVelocity = 0f; // Reset vận tốc khi chạm đất
            
            // Đặt vật thể nằm sát ngay trên mặt đất để không bị lún
            // hit.point là tọa độ chính xác của mặt đất nơi tia Raycast chạm vào
            transform.position = hit.point + new Vector3(0, groundCheckDistance, 0);
        }
    }
}
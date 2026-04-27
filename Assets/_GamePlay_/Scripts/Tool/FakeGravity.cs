using UnityEngine;
using DG.Tweening; // Bắt buộc phải có

public class FakeGravity : MonoBehaviour
{
    [Header("Cài đặt")]
    public float fallDuration = 1f;   
    public LayerMask groundLayer;     // Layer mặt đất
    
    [Header("Hiệu ứng")]
    public Ease fallEase = Ease.InQuad; // InQuad tạo cảm giác gia tốc nhanh dần (giống trọng lực)
    public bool useBounce = true;      // Thêm chút nảy khi chạm đất cho sinh động

    // void OnEnable()
    // {
    //     StartFalling();
    // }

    public void StartFalling()
    {
        Debug.Log("StartFalling");
        // 1. Xác định điểm rơi: Bắn một tia Raycast xuống dưới để tìm mặt đất
        // Khoảng cách bắn tia nên đủ dài (ví dụ 100 đơn vị)
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPos = hit.point;
            Debug.Log(targetPos);
            transform.DOKill(); 
            
            var moveTween = transform.DOMove(targetPos, fallDuration).SetEase(fallEase);

            // Nếu muốn thêm hiệu ứng nảy nhẹ khi chạm đất (Juicy)
            if (useBounce)
            {
                moveTween.OnComplete(() => {
                    transform.DOPunchPosition(Vector3.up * 0.2f, 0.3f, 5, 0.5f);
                });
            }
        }
        else
        {
            Debug.LogWarning("FakeGravity: Không tìm thấy mặt đất bên dưới vật thể!");
        }
    }
}
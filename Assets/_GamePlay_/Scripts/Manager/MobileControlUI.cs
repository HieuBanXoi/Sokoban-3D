using UnityEngine;
using UnityEngine.UI;

public class MobileControlUI : MonoBehaviour
{
    [Header("Movement Buttons")]
    public Button btnUp;
    public Button btnDown;
    public Button btnLeft;
    public Button btnRight;

    private void Start()
    {
        // Gán sự kiện Click cho từng nút dựa trên các hàm có sẵn trong InputManager
        if (InputManager.Ins != null)
        {
            btnUp.onClick.AddListener(() => InputManager.Ins.MoveUp());
            btnDown.onClick.AddListener(() => InputManager.Ins.MoveDown());
            btnLeft.onClick.AddListener(() => InputManager.Ins.MoveLeft());
            btnRight.onClick.AddListener(() => InputManager.Ins.MoveRight());
        }
        else
        {
            Debug.LogError("[MobileControlUI] Không tìm thấy InputManager trong Scene!");
        }
    }
}
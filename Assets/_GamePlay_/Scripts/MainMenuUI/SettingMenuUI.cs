using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeBtn;
    public Button backToMenuBtn;

    private void Start()
    {
        // Đóng UI Setting
        closeBtn.onClick.AddListener(() => gameObject.SetActive(false));
        
        // Quay về Menu
        backToMenuBtn.onClick.AddListener(OnBackToMenuClicked);
    }

    private async void OnBackToMenuClicked()
    {
        // 1. Lưu lại game ngay lập tức để giữ nguyên mọi dữ liệu (coin, level)
        if (DataSyncManager.Instance != null)
        {
            await DataSyncManager.Instance.SaveGameGlobal();
        }

        // 2. Bật cờ báo hiệu: "Tôi đang từ Game quay về Menu đây!"
        MenuRouteManager.isReturningFromGame = true;

        // 3. Load Scene Menu (Đảm bảo tên Scene khớp với dự án của bạn)
        SceneManager.LoadScene("MainMenuScene"); 
    }
}

// Class tĩnh này dữ liệu sẽ không bị mất khi chuyển Scene
public static class MenuRouteManager
{
    public static bool isReturningFromGame = false;
}
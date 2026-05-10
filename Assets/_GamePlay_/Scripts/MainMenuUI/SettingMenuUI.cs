using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sokoban.Presentation.UI;

public class SettingMenuUI : MonoBehaviour
{
    [Header("Pause Menu Buttons")]
    public Button closeBtn;       // Nút X ở góc trên cùng
    public Button settingsBtn;    // Nút Settings màu xám
    public Button resumeBtn;      // Nút Resume màu xanh
    public Button backToMenuBtn;  // Nút Main Menu màu cam[cite: 6]

    [Header("Audio Setting Panel")]
    public GameObject audioSettingPanel; // Kéo GameObject chứa Audio Panel vào đây
    public Button closeAudioBtn;         // Nút X của riêng Audio Panel

    private void Start()
    {
        // 1. Đóng UI Pause (Nút X và nút Resume)[cite: 6]
        closeBtn.onClick.AddListener(() => gameObject.SetActive(false));
        resumeBtn.onClick.AddListener(() => gameObject.SetActive(false));
        
        // 2. Mở bảng Audio Setting
        settingsBtn.onClick.AddListener(() => audioSettingPanel.SetActive(true));

        // 3. Đóng bảng Audio Setting
        closeAudioBtn.onClick.AddListener(() => audioSettingPanel.SetActive(false));
        
        // 4. Quay về Menu[cite: 6]
        backToMenuBtn.onClick.AddListener(OnBackToMenuClicked);
    }

    private async void OnBackToMenuClicked()
    {
        GameplayUIManager.Ins.ShowLoading(true);
        // 1. Lưu lại game ngay lập tức để giữ nguyên mọi dữ liệu (coin, level)[cite: 6]
        if (DataSyncManager.Instance != null)
        {
            await DataSyncManager.Instance.SaveGameGlobal();
        }
        // 2. Bật cờ báo hiệu: "Tôi đang từ Game quay về Menu đây!"[cite: 6]
        MenuRouteManager.isReturningFromGame = true;
        GameplayUIManager.Ins.ShowLoading(false);

        // 3. Load Scene Menu (Đảm bảo tên Scene khớp với dự án của bạn)[cite: 6]
        SceneManager.LoadScene("MainMenuScene");
    }
}

// Class tĩnh này dữ liệu sẽ không bị mất khi chuyển Scene
public static class MenuRouteManager
{
    public static bool isReturningFromGame = false;
}
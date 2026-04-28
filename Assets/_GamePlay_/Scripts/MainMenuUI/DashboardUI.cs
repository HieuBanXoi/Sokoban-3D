using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks; // Thêm thư viện này để dùng Task.Delay

public class DashboardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    
    [Header("Navigation Buttons")]
    public Button selectLevelBtn;
    public Button selectSkinBtn;
    public Button selectMapSkinBtn;
    public Button logoutBtn;

    private void Start()
    {
        // Gán sự kiện chuyển UI cho các nút phụ
        selectLevelBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowLevelSelect());
        selectSkinBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowSkinSelect());
        selectMapSkinBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowMapSkinSelect());
        
        // Sửa lại logic của nút đăng xuất
        logoutBtn.onClick.AddListener(OnLogoutClicked);
    }

    private void OnEnable()
    {
        UpdateCoinDisplay();
    }

    public void UpdateCoinDisplay()
    {
        if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
        {
            int currentCoins = DataSyncManager.Instance.gameDataSO.data.coins;
            coinText.text = currentCoins.ToString() + " <sprite index=0>";
        }
    }

    // Hàm xử lý tuần tự luồng UI Đăng xuất
    private async void OnLogoutClicked()
    {
        // 1. Chuyển sang màn hình Loading ngay lập tức
        MenuUIManager.Instance.ShowLoading();

        // 2. Gọi lệnh cắt đứt kết nối tài khoản ở DataSyncManager
        DataSyncManager.Instance.Logout();

        // 3. Đợi 0.5 giây để tạo cảm giác hệ thống đang xử lý và xóa dữ liệu mượt mà
        await Task.Delay(500); 

        // 4. Quay về màn hình bắt đầu có 3 nút (Tiếp tục, Đăng nhập, Đăng ký)
        MenuUIManager.Instance.ShowStartLogIn();
    }
}
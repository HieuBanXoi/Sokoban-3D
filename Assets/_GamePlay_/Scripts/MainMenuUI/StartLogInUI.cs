using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication; // Bắt buộc phải có để gọi ClearSessionToken

public class StartLogInUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueBtn;
    public Button startAsNewAnonymousBtn; // Thêm biến cho nút Khách Mới
    public Button goToLoginBtn;
    public Button goToRegisterBtn;
    public Button quitBtn;

    private void Start()
    {
        continueBtn.onClick.AddListener(OnContinueClicked);
        goToLoginBtn.onClick.AddListener(OnLoginMenuClicked);
        goToRegisterBtn.onClick.AddListener(OnRegisterMenuClicked);
        quitBtn.onClick.AddListener(OnQuitClicked);
        
        // Gán sự kiện cho nút Tạo Khách Mới
        if (startAsNewAnonymousBtn != null)
        {
            startAsNewAnonymousBtn.onClick.AddListener(OnStartAsNewAnonymousClicked);
        }
    }

    // --- NÚT 1: TIẾP TỤC VỚI TÀI KHOẢN CŨ ---
    public async void OnContinueClicked()
    {
        // BƯỚC KIỂM TRA LẦN ĐẦU VÀO GAME:
        // Nếu trên thiết bị chưa lưu bất kỳ Token đăng nhập nào (kể cả Guest)
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log("[Continue] Không tìm thấy dữ liệu cũ! Chuyển hướng sang tạo Khách Mới.");
            OnStartAsNewAnonymousClicked(); // Chuyển thẳng sang luồng tạo mới
            return; // Dừng luồng Continue tại đây
        }

        // Nếu đã có Token cũ (từng chơi rồi), tiến hành Load dữ liệu như bình thường
        SetButtonsInteractable(false);
        MenuUIManager.Instance.ShowLoading(); 
        
        await DataSyncManager.Instance.InitializeAndSync(); 
        
        Debug.Log($"[Continue] Đã khôi phục tài khoản: {AuthenticationService.Instance.PlayerId}");
        MenuUIManager.Instance.ShowDashboard();
        SetButtonsInteractable(true);
    }

    // --- NÚT 2: TẠO TÀI KHOẢN ẨN DANH MỚI TINH ---
    public async void OnStartAsNewAnonymousClicked()
    {
        SetButtonsInteractable(false);
        MenuUIManager.Instance.ShowLoading();

        try
        {
            // Bước 1: XÓA SẠCH TOKEN trên máy để ép hệ thống quên tài khoản cũ đi
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true); 
            }
            else
            {
                AuthenticationService.Instance.ClearSessionToken();
            }

            // Bước 2: CHỦ ĐỘNG ĐĂNG NHẬP ẨN DANH MỚI TRƯỚC
            // (Không gọi InitializeAndSync để tránh bị load nhầm file guest_offline cũ)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            string newPlayerId = AuthenticationService.Instance.PlayerId;

            // Bước 3: TẠO DỮ LIỆU MỚI HOÀN TOÀN TRÊN RAM (100 Coin, 50 Level...)
            DataSyncManager.Instance.gameDataSO.data = new GameData();

            // Bước 4: LƯU ÉP ĐÈ XUỐNG LOCAL VÀ CLOUD NGAY LẬP TỨC
            // Việc này sẽ tạo ra một file JSON mới tinh chuẩn chỉnh cho ID mới này
            await DataSyncManager.Instance.SaveGameGlobal();

            Debug.Log($"[New Anonymous] Đã tạo Khách mới: {newPlayerId}. Dữ liệu đã được khởi tạo thành công!");
            MenuUIManager.Instance.ShowDashboard();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            SetButtonsInteractable(true);
        }
    }

    public void OnLoginMenuClicked() => MenuUIManager.Instance.ShowLogin();
    public void OnRegisterMenuClicked() => MenuUIManager.Instance.ShowRegister();
    public void OnQuitClicked() => Application.Quit();

    private void SetButtonsInteractable(bool state)
    {
        continueBtn.interactable = state;
        if (startAsNewAnonymousBtn != null) startAsNewAnonymousBtn.interactable = state;
        goToLoginBtn.interactable = state;
        goToRegisterBtn.interactable = state;
        quitBtn.interactable = state;
    }
}
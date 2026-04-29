using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using TMPro;
using UnityEngine.UI;

public class DataSyncManager : MonoBehaviour
{
    // --- SINGLETON PATTERN ---
    public static DataSyncManager Instance { get; private set; }

    [Header("References")]
    public GameDataSO gameDataSO;

    [Header("Settings")]
    public bool autoSyncOnStart = true;
    public string bootSceneName = "BootScene";
    public string mainMenuSceneName = "MainMenuScene";
    [Header("UI References")]
    public CanvasGroup fadePanel;
    public Slider loadingSlider;
    public TextMeshProUGUI infoText;
    private const string GuestId = "guest_offline";

    private void Awake()
    {
        // Đảm bảo chỉ có một instance duy nhất tồn tại xuyên suốt game
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Giữ object này không bị xóa khi chuyển Scene
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        if (autoSyncOnStart)
        {
            // Chuyển sang tiếng Anh
            infoText.text = "Initializing system...";
            loadingSlider.value = 0.1f;

            await InitializeAndSync(); 
            
            loadingSlider.value = 0.8f;
            // Chuyển sang tiếng Anh
            infoText.text = "Sync complete!";

            await Task.Delay(500); 

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Khởi tạo dịch vụ, đăng nhập và đồng bộ dữ liệu Local/Cloud.
    /// </summary>
    public async Task InitializeAndSync()
    {
        if (gameDataSO == null)
        {
            Debug.LogError("[DataSync] Chưa gán GameDataSO!");
            return;
        }

        // BƯỚC 1: KHỞI TẠO DỊCH VỤ TRƯỚC (BẮT BUỘC)
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[DataSync] Unity Services đã khởi tạo xong.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataSync] Lỗi khởi tạo Services: {ex.Message}");
        }

        // BƯỚC 2: BÂY GIỜ MỚI AN TOÀN ĐỂ LẤY PLAYER ID
        // Kiểm tra xem người chơi đã đăng nhập từ phiên trước chưa
        string playerId = GuestId;
        if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
        {
            playerId = AuthenticationService.Instance.PlayerId;
        }

        // BƯỚC 3: LOAD LOCAL (OFFLINE-FIRST)
        // Dùng playerId vừa lấy được (có thể là GuestId hoặc ID tài khoản cũ)
        GameData localData = LocalSaveSystem.LoadLocal(playerId);
        gameDataSO.data = localData;

        bool signedIn = false;

        // BƯỚC 4: THỰC HIỆN ĐĂNG NHẬP (NẾU CHƯA)
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // SignInAnonymouslyAsync sẽ tự động dùng Session Token cũ nếu có
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            signedIn = AuthenticationService.Instance.IsSignedIn;
            if (signedIn)
            {
                playerId = AuthenticationService.Instance.PlayerId;
                Debug.Log($"[DataSync] Đăng nhập thành công. PlayerId: {playerId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataSync] Lỗi đăng nhập ẩn danh: {ex.Message}");
            signedIn = false;
        }

        // BƯỚC 5: ĐỒNG BỘ VỚI CLOUD (NẾU CÓ MẠNG)
        if (signedIn)
        {
            await SyncWithCloud(playerId);
        }
        else
        {
            // Nếu hoàn toàn offline, vẫn lưu local để cập nhật timestamp
            LocalSaveSystem.SaveLocal(playerId, gameDataSO.data);
        }
    }

    private async Task SyncWithCloud(string playerId)
    {
        string cloudKey = $"player_data_{playerId}";
        GameData cloudData = null;

        try
        {
            var dict = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { cloudKey });
            if (dict != null && dict.ContainsKey(cloudKey))
            {
                string cloudJson = dict[cloudKey].Value.GetAsString();
                if (!string.IsNullOrEmpty(cloudJson))
                {
                    cloudData = JsonUtility.FromJson<GameData>(cloudJson);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataSync] Không thể tải dữ liệu từ Cloud: {ex.Message}");
        }

        // So sánh mốc thời gian (Ticks) để chọn bản mới nhất
        if (cloudData != null)
        {
            long localTicks = gameDataSO.data.lastSaveTime;
            long cloudTicks = cloudData.lastSaveTime;

            if (cloudTicks > localTicks)
            {
                Debug.Log("[DataSync] Dữ liệu Cloud mới hơn. Đang cập nhật...");
                gameDataSO.data = cloudData;
            }
            else
            {
                Debug.Log("[DataSync] Dữ liệu Local mới hơn hoặc bằng Cloud. Giữ bản Local.");
            }
        }

        // Đồng bộ ngược lại: Lưu Local và đẩy lên Cloud để cả 2 khớp nhau hoàn toàn
        LocalSaveSystem.SaveLocal(playerId, gameDataSO.data);
        await PushToCloud(cloudKey, gameDataSO.data);
    }

    private async Task PushToCloud(string key, GameData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            var payload = new Dictionary<string, object> { { key, json } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
            Debug.Log("[DataSync] Đã đồng bộ dữ liệu lên Cloud.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataSync] Đẩy dữ liệu lên Cloud thất bại: {ex.Message}");
        }
    }

    /// <summary>
    /// Hàm lưu game chính: Gọi từ Gameplay hoặc Shop.
    /// </summary>
    public async Task<bool> SaveGameGlobal()
    {
        string playerId = AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : GuestId;

        // Lưu local ngay lập tức để không mất data nếu crash
        bool localOk = LocalSaveSystem.SaveLocal(playerId, gameDataSO.data);

        // Nếu có mạng thì đẩy lên Cloud
        if (AuthenticationService.Instance.IsSignedIn)
        {
            await PushToCloud($"player_data_{playerId}", gameDataSO.data);
            return true;
        }

        return localOk;
    }

    /// <summary>
    /// Đăng xuất và xóa session hiện tại.
    /// </summary>
    public void Logout()
    {
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                // BỎ tham số 'true'. Việc này giúp Unity giữ lại session token trên máy.
                AuthenticationService.Instance.SignOut(); 
                Debug.Log("[DataSync] Đã tạm thời đăng xuất. Token vẫn được giữ.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataSync] Lỗi trong quá trình đăng xuất: {ex.Message}");
        }

        // Đưa dữ liệu trên RAM về trạng thái mặc định để không bị đè chữ của tài khoản cũ
        if (gameDataSO != null)
        {
            gameDataSO.data = new GameData();
        }
    }
}
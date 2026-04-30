using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class GameplayUIManager : Ply_Singleton<GameplayUIManager>
{
    [Header("Gameplay Info")]
    public TextMeshProUGUI currentLevelText;

    [Header("Gameplay Buttons")]
    public Button solveBtn;
    public Button hintBtn;
    public Button undoBtn;
    public Button reloadBtn;
    public Button settingBtn;

    [Header("Panels")]
    public GameObject settingPanel;
    public GameObject levelCompletePanel; // Panel hiện ra khi thắng
    public GameObject loadingPanel;       // Panel loading (Nên có một ảnh nền đen trong suốt và xoay vòng loading)

    [Header("Win UI Elements")]
    public Button nextLevelBtn;
    public Button playAgainBtn;
    public Button winBackToMenuBtn;
    public TextMeshProUGUI coinText;

    private bool isLevelWon = false; // Cờ chặn gọi hàm win nhiều lần
    private const int HINT_PRICE = 10;
    private const int WIN_REWARD = 100;


    private void Start()
    {
        // 1. GÁN SỰ KIỆN CƠ BẢN
        undoBtn.onClick.AddListener(() => CommandManager.Ins.Undo());
        reloadBtn.onClick.AddListener(() => LevelGenerator.Ins.GenerateMapByLevel(LevelGenerator.Ins.levelIdToLoad));
        settingBtn.onClick.AddListener(() => settingPanel.SetActive(true));

        // 2. LOGIC NÚT SOLVE (Chỉ mở khi level đã Complete)
        var data = DataSyncManager.Instance.gameDataSO.data;
        int currentLevelIndex = data.currentPlayingLevel;
        if (currentLevelText != null)
        {
            currentLevelText.text = "Level " + currentLevelIndex.ToString();
        }
        LevelStatus currentStatus = data.levels.Find(l => l.levelIndex == currentLevelIndex);
        
        if (currentStatus != null && currentStatus.isCompleted)
        {
            solveBtn.interactable = true;
            solveBtn.onClick.AddListener(() => TutorialController.Ins.Play());
        }
        else
        {
            solveBtn.interactable = false; // Khóa nút Solve
        }

        // 3. LOGIC NÚT HINT (Trừ tiền)
        hintBtn.onClick.AddListener(OnHintClicked);

        // 4. GÁN SỰ KIỆN CHO MÀN HÌNH WIN
        nextLevelBtn.onClick.AddListener(OnNextLevelClicked);
        playAgainBtn.onClick.AddListener(OnPlayAgainClicked);
        winBackToMenuBtn.onClick.AddListener(OnBackToMenuClicked);

        UpdateCoinDisplay();
    }

    private async void OnHintClicked()
    {
        var data = DataSyncManager.Instance.gameDataSO.data;

        if (data.coins >= HINT_PRICE)
        {
            // Trừ tiền và chạy Hint
            data.coins -= HINT_PRICE;
            UpdateCoinDisplay();
            TutorialController.Ins.OnClickHintButton();

            // Lưu game ngầm để tránh mất tiền nếu thoát
            ShowLoading(true);
            await DataSyncManager.Instance.SaveGameGlobal();
            ShowLoading(false);
        }
        else
        {
            Debug.LogWarning("Không đủ 10 coin để dùng Gợi ý!");
            // (Tùy chọn) Hiện Text thông báo thiếu tiền tại đây
        }
    }

    // --- LOGIC KHI THẮNG GAME (BOX.CS SẼ GỌI HÀM NÀY) ---
    public async void OnLevelCompleted()
    {
        if (isLevelWon) return; // Chặn nếu 2 hộp cùng vào đích 1 lúc
        isLevelWon = true;

        ShowLoading(true);

        var data = DataSyncManager.Instance.gameDataSO.data;
        int currentLevelIndex = data.currentPlayingLevel;
        LevelStatus currentStatus = data.levels.Find(l => l.levelIndex == currentLevelIndex);

        // Chỉ thưởng tiền và lưu trạng thái nếu đây là lần ĐẦU TIÊN hoàn thành level này
        if (currentStatus != null && !currentStatus.isCompleted)
        {
            currentStatus.isCompleted = true;
            data.coins += WIN_REWARD;

            // ---------------------------------------------------------
            // LOGIC MỚI: KIỂM TRA VÀ MỞ KHÓA 10 MÀN TIẾP THEO
            // ---------------------------------------------------------
            if (currentLevelIndex % 10 == 0) // Nếu đang ở màn 10, 20, 30...
            {
                bool isAllPreviousCompleted = true;
                int startLevelOfBlock = currentLevelIndex - 9; // Ví dụ: Đang ở màn 10 -> bắt đầu kiểm tra từ màn 1

                // 1. Quét kiểm tra 9 màn trước đó
                for (int i = startLevelOfBlock; i < currentLevelIndex; i++)
                {
                    LevelStatus prevLevel = data.levels.Find(l => l.levelIndex == i);
                    if (prevLevel == null || !prevLevel.isCompleted)
                    {
                        isAllPreviousCompleted = false;
                        break; // Chỉ cần 1 màn chưa qua là dừng kiểm tra luôn
                    }
                }

                // 2. Nếu đã hoàn thành TẤT CẢ các màn trước, tiến hành mở khóa
                if (isAllPreviousCompleted)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        int nextIndexToUnlock = currentLevelIndex + i;
                        LevelStatus statusToUnlock = data.levels.Find(l => l.levelIndex == nextIndexToUnlock);
                        if (statusToUnlock != null)
                        {
                            statusToUnlock.isUnlocked = true;
                        }
                    }
                    Debug.Log($"Đã mở khóa từ màn {currentLevelIndex + 1} đến {currentLevelIndex + 10}!");
                }
                else
                {
                    Debug.Log($"[Level Complete] Bạn đã qua màn {currentLevelIndex}, nhưng chưa qua hết 9 màn trước đó. Chưa thể mở cụm tiếp theo!");
                }
            }
            // ---------------------------------------------------------

            // Lưu data lên Cloud và Local
            await DataSyncManager.Instance.SaveGameGlobal();
        }

        UpdateCoinDisplay();
        ShowLoading(false);
        
        // Hiện bảng chiến thắng
        levelCompletePanel.SetActive(true);
    }

    // --- CÁC NÚT TRONG BẢNG WIN ---
    private void OnNextLevelClicked()
    {
        var data = DataSyncManager.Instance.gameDataSO.data;
        int nextLevel = data.currentPlayingLevel + 1;

        LevelStatus nextStatus = data.levels.Find(l => l.levelIndex == nextLevel);
        
        if (nextStatus != null && nextStatus.isUnlocked)
        {
            // Cập nhật level đang chơi và tải lại scene
            data.currentPlayingLevel = nextLevel;
            ShowLoading(true);
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            Debug.LogWarning("Màn tiếp theo chưa được mở khóa hoặc đã hết màn chơi!");
            // (Tùy chọn) Hiện Text báo hiệu hết map
        }
    }

    private void OnPlayAgainClicked()
    {
        ShowLoading(true);
        SceneManager.LoadScene("MainScene"); // Tải lại nhanh chính màn này
    }

    private async void OnBackToMenuClicked()
    {
        ShowLoading(true);
        // Lưu lại cẩn thận trước khi thoát
        if (DataSyncManager.Instance != null) await DataSyncManager.Instance.SaveGameGlobal();
        
        MenuRouteManager.isReturningFromGame = true; // Cờ chuyển vào Dashboard
        SceneManager.LoadScene("MainMenuScene");
    }

    // --- UTILS ---
    public void UpdateCoinDisplay()
    {
        if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
        {
            coinText.text = DataSyncManager.Instance.gameDataSO.data.coins.ToString();
        }
    }

    public void ShowLoading(bool isShow)
    {
        if (loadingPanel != null) loadingPanel.SetActive(isShow);
    }
}
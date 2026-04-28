using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Level Buttons (Fixed 10)")]
    public Button[] levelButtons; // Kéo 10 nút Level vào đây
    public TextMeshProUGUI[] levelTexts; // Kéo 10 Text của 10 nút trên vào đây

    [Header("Navigation Buttons")]
    public Button leftArrowBtn;
    public Button rightArrowBtn;
    public Button playBtn;
    public Button backBtn;

    [Header("Colors")]
    public Color selectedColor = Color.yellow;
    public Color unlockedColor = Color.white;
    public Color lockedColor = Color.gray;

    private int currentPage = 0;
    private const int LEVELS_PER_PAGE = 10;
    private const int MAX_LEVELS = 50;
    private int maxPage;

    // Biến tạm để lưu level người chơi đang bấm chọn trên UI
    private int tempSelectedLevel = 1; 
    private void Awake()
    {
        maxPage = (MAX_LEVELS - 1) / LEVELS_PER_PAGE; // 50 level -> maxPage = 4 (Trang 0 đến 4)
    }
    private void Start()
    {
        // Gán sự kiện chuyển trang
        leftArrowBtn.onClick.AddListener(() => ChangePage(-1));
        rightArrowBtn.onClick.AddListener(() => ChangePage(1));
        
        // Gán sự kiện Play và Back
        playBtn.onClick.AddListener(OnPlayClicked);
        backBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowDashboard());
    }

    private void OnEnable()
    {
        if (DataSyncManager.Instance == null) return;

        // Lấy level đang lưu trong data để làm mốc
        tempSelectedLevel = DataSyncManager.Instance.gameDataSO.data.currentPlayingLevel;
        
        // Tính toán để mở đúng trang chứa level đang chọn (vd: level 15 -> trang 1)
        currentPage = (tempSelectedLevel - 1) / LEVELS_PER_PAGE;
        
        UpdatePageUI();
    }

    private void ChangePage(int direction)
    {
        currentPage += direction;
        currentPage = Mathf.Clamp(currentPage, 0, maxPage);
        UpdatePageUI();
    }

    private void UpdatePageUI()
    {
        // 1. Cập nhật trạng thái hiển thị của 2 nút mũi tên
        leftArrowBtn.gameObject.SetActive(currentPage > 0);
        rightArrowBtn.gameObject.SetActive(currentPage < maxPage);

        var data = DataSyncManager.Instance.gameDataSO.data;

        // 2. Cập nhật 10 nút Level cố định
        for (int i = 0; i < LEVELS_PER_PAGE; i++)
        {
            int levelId = (currentPage * LEVELS_PER_PAGE) + i + 1; // Tính toán ID thực tế (1 đến 50)
            
            // Xóa hết sự kiện cũ của nút này
            levelButtons[i].onClick.RemoveAllListeners();

            if (levelId <= MAX_LEVELS)
            {
                levelButtons[i].gameObject.SetActive(true);
                levelTexts[i].text = levelId.ToString();

                // Tìm data của level này
                LevelStatus levelStatus = data.levels.Find(l => l.levelIndex == levelId);
                bool isUnlocked = (levelStatus != null && levelStatus.isUnlocked);

                if (isUnlocked)
                {
                    levelButtons[i].interactable = true;
                    // Gán sự kiện mới
                    int idToSelect = levelId; 
                    levelButtons[i].onClick.AddListener(() => OnLevelSelected(idToSelect));
                }
                else
                {
                    // Khóa nút nếu chưa mở
                    levelButtons[i].interactable = false;
                }
            }
            else
            {
                // Ẩn nút nếu vượt quá số lượng map (vd sau này có 45 map)
                levelButtons[i].gameObject.SetActive(false);
            }
        }

        RefreshButtonColors();
    }

    private void OnLevelSelected(int levelId)
    {
        tempSelectedLevel = levelId;
        RefreshButtonColors();
    }

    private void RefreshButtonColors()
    {
        if (DataSyncManager.Instance == null) return;
        var data = DataSyncManager.Instance.gameDataSO.data;

        for (int i = 0; i < LEVELS_PER_PAGE; i++)
        {
            int levelId = (currentPage * LEVELS_PER_PAGE) + i + 1;
            if (levelId > MAX_LEVELS) break;

            Image btnImage = levelButtons[i].GetComponent<Image>();
            LevelStatus levelStatus = data.levels.Find(l => l.levelIndex == levelId);
            bool isUnlocked = (levelStatus != null && levelStatus.isUnlocked);

            if (!isUnlocked)
            {
                btnImage.color = lockedColor;
            }
            else if (levelId == tempSelectedLevel)
            {
                btnImage.color = selectedColor; // Sáng màu vàng nếu đang được chọn
            }
            else
            {
                btnImage.color = unlockedColor;
            }
        }
    }

    private void OnPlayClicked()
    {
        // 1. Lưu lại level vừa chọn vào GameData
        DataSyncManager.Instance.gameDataSO.data.currentPlayingLevel = tempSelectedLevel;
        
        // Lưu xuống Local (Có thể bỏ qua lưu Cloud ở bước này để Load scene cho nhanh, 
        // lúc end game thắng/thua lưu Cloud sau cũng được).
        _=DataSyncManager.Instance.SaveGameGlobal(); 

        Debug.Log($"[LevelUI] Chuẩn bị vào Game với Level: {tempSelectedLevel}");

        // 2. Load sang MainScene (Đảm bảo tên Scene chính xác)
        SceneManager.LoadScene("MainScene"); 
    }
}
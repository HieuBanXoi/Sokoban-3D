using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Coin Display")]
    public TextMeshProUGUI coinText;
    [Header("Level Buttons (Fixed 10)")]
    public Button[] levelButtons; 
    public TextMeshProUGUI[] levelTexts; 
    public Image[] starImages; // Kéo 10 Image của ngôi sao tương ứng với 10 nút vào đây

    [Header("Level State Sprites")]
    public Sprite unlockedSprite;
    public Sprite selectedSprite;
    public Sprite lockedSprite;

    [Header("Star Sprites")]
    public Sprite emptyStarSprite;
    public Sprite fullStarSprite;

    [Header("Navigation Buttons")]
    public Button leftArrowBtn;
    public Button rightArrowBtn;
    public Button playBtn;
    public Button backBtn;

    private int currentPage = 0;
    private const int LEVELS_PER_PAGE = 10;
    private const int MAX_LEVELS = 50;
    private int maxPage;

    private int tempSelectedLevel = 1; 

    private void Awake()
    {
        maxPage = (MAX_LEVELS - 1) / LEVELS_PER_PAGE; 
    }

    private void Start()
    {
        leftArrowBtn.onClick.AddListener(() => ChangePage(-1));
        rightArrowBtn.onClick.AddListener(() => ChangePage(1));
        
        playBtn.onClick.AddListener(OnPlayClicked);
        backBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowDashboard());
    }

    private void OnEnable()
    {
        if (DataSyncManager.Instance == null) return;

        tempSelectedLevel = DataSyncManager.Instance.gameDataSO.data.currentPlayingLevel;
        currentPage = (tempSelectedLevel - 1) / LEVELS_PER_PAGE;
        
        UpdatePageUI();
        UpdateCoinDisplay();
    }
    // Hàm cập nhật hiển thị số tiền
    public void UpdateCoinDisplay()
    {
        if (coinText != null && DataSyncManager.Instance != null)
        {
            // Lấy dữ liệu coin từ GameDataSO
            int currentCoins = DataSyncManager.Instance.gameDataSO.data.coins;
            coinText.text = currentCoins.ToString();
        }
    }
    private void ChangePage(int direction)
    {
        currentPage += direction;
        currentPage = Mathf.Clamp(currentPage, 0, maxPage);
        UpdatePageUI();
    }

    private void UpdatePageUI()
    {
        leftArrowBtn.gameObject.SetActive(currentPage > 0);
        rightArrowBtn.gameObject.SetActive(currentPage < maxPage);

        var data = DataSyncManager.Instance.gameDataSO.data;

        for (int i = 0; i < LEVELS_PER_PAGE; i++)
        {
            int levelId = (currentPage * LEVELS_PER_PAGE) + i + 1; 
            
            levelButtons[i].onClick.RemoveAllListeners();

            if (levelId <= MAX_LEVELS)
            {
                levelButtons[i].gameObject.SetActive(true);
                // Hiển thị ngôi sao nếu map tồn tại
                if(starImages[i] != null) starImages[i].gameObject.SetActive(true);

                levelTexts[i].text = levelId.ToString();

                LevelStatus levelStatus = data.levels.Find(l => l.levelIndex == levelId);
                bool isUnlocked = (levelStatus != null && levelStatus.isUnlocked);

                if (isUnlocked)
                {
                    levelButtons[i].interactable = true;
                    int idToSelect = levelId; 
                    levelButtons[i].onClick.AddListener(() => OnLevelSelected(idToSelect));
                }
                else
                {
                    levelButtons[i].interactable = false;
                }
            }
            else
            {
                levelButtons[i].gameObject.SetActive(false);
                if(starImages[i] != null) starImages[i].gameObject.SetActive(false);
            }
        }

        RefreshLevelSprites();
    }

    private void OnLevelSelected(int levelId)
    {
        tempSelectedLevel = levelId;
        RefreshLevelSprites();
    }

    private void RefreshLevelSprites()
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
            bool isCompleted = (levelStatus != null && levelStatus.isCompleted);

            // 1. Cập nhật Sprite cho nút Level
            if (!isUnlocked)
            {
                btnImage.sprite = lockedSprite;
            }
            else if (levelId == tempSelectedLevel)
            {
                btnImage.sprite = selectedSprite;
            }
            else
            {
                btnImage.sprite = unlockedSprite;
            }

            // 2. Cập nhật Sprite cho Ngôi sao
            if (starImages[i] != null)
            {
                starImages[i].sprite = isCompleted ? fullStarSprite : emptyStarSprite;
            }
        }
    }

    private void OnPlayClicked()
    {
        DataSyncManager.Instance.gameDataSO.data.currentPlayingLevel = tempSelectedLevel;
        _=DataSyncManager.Instance.SaveGameGlobal(); 
        SceneManager.LoadScene("MainScene"); 
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks; 
using UnityEngine.SceneManagement;

public class DashboardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI currentLevelText; 
    public Image currentCharacterSkinImage;
    public Image currentMapSkinImage;

    [Header("Navigation Buttons")]
    public Button playBtn;
    public Button selectLevelBtn;
    public Button selectSkinBtn;
    public Button selectMapSkinBtn;
    public Button logoutBtn;

    private void Start()
    {
        playBtn.onClick.AddListener(OnPlayClicked);
        selectLevelBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowLevelSelect());
        selectSkinBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowSkinSelect());
        selectMapSkinBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowMapSkinSelect());
        
        logoutBtn.onClick.AddListener(OnLogoutClicked);
    }
    private void OnPlayClicked()
    {
        // Hiển thị màn hình Loading (nếu có)
        if (MenuUIManager.Instance != null)
        {
            MenuUIManager.Instance.ShowLoading();
        }

        // Load Scene Gameplay (dựa vào các script trước, scene này tên là "MainScene")
        SceneManager.LoadScene("MainScene"); 
    }
    private void OnEnable()
    {
        UpdateDashboardInfo(); 
    }

    public void UpdateDashboardInfo()
    {
        if (DataSyncManager.Instance == null || DataSyncManager.Instance.gameDataSO == null) return;

        var data = DataSyncManager.Instance.gameDataSO.data;

        if (coinText != null) coinText.text = data.coins.ToString();

        if (currentLevelText != null)
        {
            currentLevelText.text = data.currentPlayingLevel.ToString();
        }

        // Lấy Sprite nhân vật từ SpriteManager
        if (currentCharacterSkinImage != null && SpriteManager.Ins != null)
        {
            int charIndex = data.currentCharacterSkinIndex;
            if (charIndex >= 0 && charIndex < SpriteManager.Ins.playerSkinSprites.Length)
            {
                currentCharacterSkinImage.sprite = SpriteManager.Ins.playerSkinSprites[charIndex];
                currentCharacterSkinImage.SetNativeSize(); 
            }
        }

        // Lấy Sprite Map từ SpriteManager
        if (currentMapSkinImage != null && SpriteManager.Ins != null)
        {
            int mapIndex = data.currentMapSkinIndex;
            if (mapIndex >= 0 && mapIndex < SpriteManager.Ins.mapSkinSprites.Length)
            {
                currentMapSkinImage.sprite = SpriteManager.Ins.mapSkinSprites[mapIndex];
                // currentMapSkinImage.SetNativeSize(); 
            }
        }
    }

    private async void OnLogoutClicked()
    {
        MenuUIManager.Instance.ShowLoading();
        DataSyncManager.Instance.Logout();
        await Task.Delay(500); 
        MenuUIManager.Instance.ShowStartLogIn();
    }
}
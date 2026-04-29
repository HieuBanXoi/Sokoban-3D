using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance { get; private set; }

    [Header("Auth Canvases")]
    public GameObject loadingCanvas;
    public GameObject startLogInCanvas; 
    public GameObject loginCanvas;
    public GameObject registerCanvas;

    [Header("Game Canvases")]
    public GameObject dashboardCanvas; // Màn hình chính sau khi đăng nhập
    public GameObject levelCanvas;     // Chọn Map/Level
    public GameObject skinCanvas;      // Chọn Skin NV
    public GameObject mapSkinCanvas;   // Chọn Skin Map
    [Header("Audio Setting")]
    public Button audioSettingBtn;
    public GameObject audioSettingPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ShowStartLogIn(); 
    }
    private void Start()
    {
        // Kiểm tra xem có phải vừa từ MainScene (Gameplay) quay về không?
        if (MenuRouteManager.isReturningFromGame)
        {
            // Tắt cờ đi để lần sau mở game bình thường nó không bị nhầm
            MenuRouteManager.isReturningFromGame = false;
            
            // Bay thẳng vào Dashboard thay vì màn hình 3 nút
            ShowDashboard(); 
        }
        // Thiết lập nút mở Audio Setting
        if (audioSettingBtn != null)        {
            audioSettingBtn.onClick.AddListener(OpenAudioSetting);
        }
    }
    public void ToggleAudioSetting(bool isShow)
    {
        if (audioSettingPanel != null)
        {
            audioSettingPanel.SetActive(isShow);
        }
    }
    public void OpenAudioSetting() => ToggleAudioSetting(true);
    // Các hàm show UI xác thực cũ
    public void ShowLoading() => SetActiveCanvas(loadingCanvas);
    public void ShowStartLogIn() => SetActiveCanvas(startLogInCanvas);
    public void ShowLogin() => SetActiveCanvas(loginCanvas);
    public void ShowRegister() => SetActiveCanvas(registerCanvas);

    // Các hàm show UI mới
    public void ShowDashboard() => SetActiveCanvas(dashboardCanvas);
    public void ShowLevelSelect() => SetActiveCanvas(levelCanvas);
    public void ShowSkinSelect() => SetActiveCanvas(skinCanvas);
    public void ShowMapSkinSelect() => SetActiveCanvas(mapSkinCanvas);

    private void SetActiveCanvas(GameObject activeCanvas)
    {
        // Tắt/bật linh hoạt
        loadingCanvas.SetActive(loadingCanvas == activeCanvas);
        startLogInCanvas.SetActive(startLogInCanvas == activeCanvas);
        loginCanvas.SetActive(loginCanvas == activeCanvas);
        registerCanvas.SetActive(registerCanvas == activeCanvas);
        
        if (dashboardCanvas) dashboardCanvas.SetActive(dashboardCanvas == activeCanvas);
        if (levelCanvas) levelCanvas.SetActive(levelCanvas == activeCanvas);
        if (skinCanvas) skinCanvas.SetActive(skinCanvas == activeCanvas);
        if (mapSkinCanvas) mapSkinCanvas.SetActive(mapSkinCanvas == activeCanvas);
    }
}
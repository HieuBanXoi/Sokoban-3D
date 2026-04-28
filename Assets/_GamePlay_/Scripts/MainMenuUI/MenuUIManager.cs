using UnityEngine;

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

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ShowStartLogIn(); 
    }

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
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;

public class LoginUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField userField;
    public TMP_InputField passField;
    
    [Header("Buttons")]
    public Button loginSubmitBtn;
    public Button backBtn;

    [Header("Feedback")]
    public TextMeshProUGUI errorText;

    private void Start()
    {
        loginSubmitBtn.onClick.AddListener(OnLoginSubmit);
        backBtn.onClick.AddListener(OnBackClicked);
    }

    public async void OnLoginSubmit()
    {
        errorText.text = "";
        loginSubmitBtn.interactable = false;

        try
        {
            // Bảo vệ: Đảm bảo Services đã khởi tạo nếu chạy test thẳng từ MainMenu
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            if (AuthenticationService.Instance.IsSignedIn) AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(userField.text, passField.text);
            
            MenuUIManager.Instance.ShowLoading();
            await DataSyncManager.Instance.InitializeAndSync();
            
            Debug.Log("[Login] Thành công!");
            MenuUIManager.Instance.ShowDashboard();
        }
        catch (RequestFailedException ex) 
        {
            errorText.text = ex.Message;
        }
        catch (Exception ex)
        {
            errorText.text = "System error. Try again later.";
            Debug.LogException(ex);
        }
        finally
        {
            loginSubmitBtn.interactable = true;
        }
    }
    public void OnBackClicked() => MenuUIManager.Instance.ShowStartLogIn();
}
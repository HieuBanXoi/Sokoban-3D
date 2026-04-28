using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;
using System;
using Unity.Services.Core;

public class RegisterUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField userField;
    public TMP_InputField passField;

    [Header("Buttons")]
    public Button registerSubmitBtn;
    public Button backBtn;

    [Header("Feedback")]
    public TextMeshProUGUI errorText;
    private void Start()
    {
        // Gán sự kiện
        registerSubmitBtn.onClick.AddListener(OnRegisterSubmit);
        backBtn.onClick.AddListener(OnBackClicked);
    }
    public async void OnRegisterSubmit()
    {
        errorText.text = "";
        registerSubmitBtn.interactable = false;

        try
        {
            if (AuthenticationService.Instance.IsSignedIn) AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(userField.text, passField.text);
            
            MenuUIManager.Instance.ShowLoading(); // Tham chiếu MenuUIManager mới
            await DataSyncManager.Instance.InitializeAndSync();
            
            Debug.Log("[Register] Thành công!");
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
            registerSubmitBtn.interactable = true;
        }
    }
    private string GetErrorMessage(int errorCode)
    {
        // Dùng if - else if để xử lý các giá trị static readonly
        if (errorCode == AuthenticationErrorCodes.InvalidParameters)
        {
            return "Invalid username or password format.";
        }
        else if (errorCode == AuthenticationErrorCodes.InvalidProvider)
        {
            return "Incorrect username or password.";
        }
        else if (errorCode == AuthenticationErrorCodes.ClientNoActiveSession)
        {
            return "Session expired. Please try again.";
        }
        else if (errorCode == AuthenticationErrorCodes.ClientInvalidUserState)
        {
            return "User is already logged in on another device.";
        }
        else
        {
            return "Login failed. Error code: " + errorCode;
        }
    }
    public void OnBackClicked() => MenuUIManager.Instance.ShowStartLogIn();
}
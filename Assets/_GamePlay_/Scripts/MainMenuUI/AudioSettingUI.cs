using UnityEngine;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeBtn;

    private void Start()
    {
        // Khi bấm nút thoát, gọi ngược lại MenuUIManager để ẩn chính nó
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() => MenuUIManager.Instance.ToggleAudioSetting(false));
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider vfxSlider;

    [Header("Buttons")]
    public Button closeBtn;

    private void Start()
    {
        // 1. Logic nút đóng[cite: 9]
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() => 
            {
                // Thêm fallback: Nếu ở Scene Game không có MenuUIManager thì tự tắt gameObject
                if (MenuUIManager.Instance != null) 
                    MenuUIManager.Instance.ToggleAudioSetting(false);
                else 
                    gameObject.SetActive(false); 
            });
        }

        // 2. Gắn sự kiện khi người chơi kéo các Slider
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (vfxSlider != null)
            vfxSlider.onValueChanged.AddListener(SetVFXVolume);
    }

    private void OnEnable()
    {
        // MỖI KHI BẬT PANEL LÊN (Ở BẤT KỲ SCENE NÀO):
        // Lấy dữ liệu đã lưu để set lại vị trí con trỏ Slider cho đồng bộ
        if (masterSlider != null)
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        
        if (vfxSlider != null)
            vfxSlider.value = PlayerPrefs.GetFloat("VfxVolume", 1f);
    }

    // --- CÁC HÀM XỬ LÝ LƯU VÀ ĐỒNG BỘ ÂM THANH ---

    private void SetMasterVolume(float value)
    {
        // AudioListener.volume là âm lượng tổng của toàn bộ game
        AudioListener.volume = value; 
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void SetMusicVolume(float value)
    {
        if (Ply_SoundManager.Ins != null)
            Ply_SoundManager.Ins.SetMusicVolume(value);
        else
            PlayerPrefs.SetFloat("MusicVolume", value); // Lưu dự phòng nếu SoundManager chưa kịp load
    }

    private void SetVFXVolume(float value)
    {
        if (Ply_SoundManager.Ins != null)
            Ply_SoundManager.Ins.SetVFXVolume(value);
        else
            PlayerPrefs.SetFloat("VfxVolume", value); // Lưu dự phòng nếu SoundManager chưa kịp load
    }
}
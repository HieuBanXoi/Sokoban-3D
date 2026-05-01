using UnityEngine;

public enum FxType
{
    Pop = 0,
    Win = 1,
    DoneEffect = 2,

}

public class Ply_SoundManager : Ply_Singleton<Ply_SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource musicSource; // Component phát Nhạc nền (BGM)
    public AudioSource vfxSource;   // Component phát Hiệu ứng (SFX)

    [Header("Audio Clips")]
    // Kéo thả các clip tương ứng với FxType vào đây (Index 0 là Pop, 1 là CanMerge...)
    public AudioClip[] fxClips;     

    // Khóa lưu trữ cài đặt âm lượng vào bộ nhớ máy
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string VFX_VOL_KEY = "VfxVolume";
    public override void Awake() // Hoặc public void Awake() tùy vào cách viết của Ply_Singleton
    {
        base.Awake(); // Giữ nguyên logic khởi tạo của Ply_Singleton
        
        // Ép Unity không được hủy object này khi chuyển Scene
        DontDestroyOnLoad(this.gameObject); 
    }
    private void Start()
    {
        // Khởi tạo âm lượng từ bộ nhớ
        float savedMusicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float savedVfxVol = PlayerPrefs.GetFloat(VFX_VOL_KEY, 1f);
        
        // THÊM DÒNG NÀY: Khởi tạo âm lượng tổng
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        SetMusicVolume(savedMusicVol);
        SetVFXVolume(savedVfxVol);
    }

    // =====================================
    // KẾT NỐI VỚI SLIDER (VOLUME CONTROL)
    // =====================================
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
        }
    }

    public void SetVFXVolume(float volume)
    {
        if (vfxSource != null)
        {
            vfxSource.volume = volume;
            PlayerPrefs.SetFloat(VFX_VOL_KEY, volume);
        }
    }

    // =====================================
    // HÀM PHÁT ÂM THANH
    // =====================================
    
    // Phát nhạc nền (Gọi hàm này khi vào map mới hoặc dashboard)
    public void PlayMusic(AudioClip bgmClip)
    {
        if (musicSource == null || bgmClip == null) return;
        
        // Nếu bài nhạc đang yêu cầu chính là bài đang phát thì không phát lại từ đầu
        if (musicSource.clip == bgmClip) return; 

        musicSource.clip = bgmClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Phát hiệu ứng âm thanh (Thay thế logic cũ)
    public void PlayFx(FxType fxType)
    {
        if (vfxSource == null) return;

        int index = (int)fxType;
        if (index < 0 || index >= fxClips.Length || fxClips[index] == null)
        {
            Debug.LogWarning($"[SoundManager] Thiếu AudioClip cho FxType: {fxType}");
            return;
        }

        // Phát đè lên nhau dựa trên mức âm lượng hiện tại của vfxSource
        vfxSource.PlayOneShot(fxClips[index], vfxSource.volume);
    }

    // =====================================
    // CÁC HÀM TIỆN ÍCH KHÁC
    // =====================================
    public void MuteAll()
    {
        if (musicSource != null) musicSource.Stop();
        if (vfxSource != null) vfxSource.Stop();
    }
}
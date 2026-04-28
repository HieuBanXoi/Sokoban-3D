using UnityEngine;
using UnityEngine.UI;

public class CharacterSkinUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button[] skinButtons; 
    public Button backBtn;

    [Header("Colors")]
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    
    // Thêm màu xám để báo hiệu skin chưa mua
    public Color lockedColor = Color.gray; 

    // Cài đặt giá tiền
    private const int SKIN_PRICE = 100;

    private void Start()
    {
        backBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowDashboard());

        for (int i = 0; i < skinButtons.Length; i++)
        {
            int index = i; 
            skinButtons[i].onClick.AddListener(() => OnSkinButtonClicked(index));
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void OnSkinButtonClicked(int index)
    {
        if (DataSyncManager.Instance == null || DataSyncManager.Instance.gameDataSO == null) return;
        
        var data = DataSyncManager.Instance.gameDataSO.data;
        
        // Kiểm tra xem index có hợp lệ không
        if (index < data.characterSkins.Count)
        {
            // TRƯỜNG HỢP 1: ĐÃ MỞ KHÓA -> Chỉ cần trang bị
            if (data.characterSkins[index].isUnlocked)
            {
                data.currentCharacterSkinIndex = index;
                _=DataSyncManager.Instance.SaveGameGlobal();
                RefreshUI();
                Debug.Log($"[CharacterSkin] Đã trang bị skin số {index}");
            }
            // TRƯỜNG HỢP 2: CHƯA MỞ KHÓA -> Tiến hành mua
            else
            {
                if (data.coins >= SKIN_PRICE)
                {
                    // Trừ tiền
                    data.coins -= SKIN_PRICE;
                    // Mở khóa skin
                    data.characterSkins[index].isUnlocked = true;
                    // Tự động trang bị luôn sau khi mua
                    data.currentCharacterSkinIndex = index;
                    
                    // Lưu dữ liệu để không bị mất tiền & mất skin nếu thoát game
                    _=DataSyncManager.Instance.SaveGameGlobal();
                    RefreshUI();
                    
                    Debug.Log($"[CharacterSkin] Mua THÀNH CÔNG skin số {index}. Coin còn lại: {data.coins}");
                }
                else
                {
                    Debug.LogWarning($"[CharacterSkin] KHÔNG ĐỦ TIỀN! Cần {SKIN_PRICE} coin nhưng bạn chỉ có {data.coins} coin.");
                    // Ở đây sau này bạn có thể hiện lên một bảng Text thông báo lỗi cho người chơi
                }
            }
        }
    }

    private void RefreshUI()
    {
        if (DataSyncManager.Instance == null) return;

        var data = DataSyncManager.Instance.gameDataSO.data;
        int currentIndex = data.currentCharacterSkinIndex;

        for (int i = 0; i < skinButtons.Length; i++)
        {
            Image btnImage = skinButtons[i].GetComponent<Image>();
            if (btnImage != null)
            {
                // Nếu đang được chọn -> Màu vàng
                if (i == currentIndex) 
                {
                    btnImage.color = selectedColor;
                }
                // Nếu chưa mở khóa -> Màu xám để dễ nhận biết
                else if (!data.characterSkins[i].isUnlocked) 
                {
                    btnImage.color = lockedColor;
                }
                // Đã mở khóa nhưng không dùng -> Màu trắng
                else 
                {
                    btnImage.color = defaultColor;
                }
            }
        }
    }
}
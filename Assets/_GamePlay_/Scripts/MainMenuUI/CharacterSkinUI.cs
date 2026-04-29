using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterSkinUI : MonoBehaviour
{
    [Header("Sprites Data")]
    public Sprite[] allSkinSprites; // Kéo thả 10 ảnh skin vào đây

    [Header("UI Display")]
    public Image skinDisplayImage;      
    public TextMeshProUGUI skinNameText; 
    public TextMeshProUGUI coinText;    
    public TextMeshProUGUI errorMsgText;

    [Header("Navigation")]
    public Button leftArrowBtn;
    public Button rightArrowBtn;
    private int viewIndex = 0; 

    [Header("Unlock Button")]
    public Button unlockBtn;
    public TextMeshProUGUI unlockBtnText;
    public Image unlockBtnImage;

    [Header("Equip Button")]
    public Button equipBtn;
    public TextMeshProUGUI equipBtnText;
    public Image equipBtnImage;
    
    [Header("Button Sprites")]
    public Sprite btnCanPressSprite;   // Sprite khi nút có thể bấm
    public Sprite btnDisabledSprite;   // Sprite khi nút bị khóa/vô hiệu hóa
    public Sprite btnEquippedSprite;   // Sprite khi đã trang bị (Equipped)

    [Header("Common")]
    public Button backBtn;

    private const int UNLOCK_PRICE = 100;

    private void Start()
    {
        leftArrowBtn.onClick.AddListener(() => ChangeView(-1));
        rightArrowBtn.onClick.AddListener(() => ChangeView(1));
        
        unlockBtn.onClick.AddListener(OnUnlockClicked);
        equipBtn.onClick.AddListener(OnEquipClicked);
        
        backBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowDashboard());
        
        if (errorMsgText != null) errorMsgText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
        {
            viewIndex = DataSyncManager.Instance.gameDataSO.data.currentCharacterSkinIndex;
        }
        UpdateUI();
    }

    private void ChangeView(int dir)
    {
        if (allSkinSprites.Length == 0) return;
        viewIndex += dir;

        if (viewIndex < 0) viewIndex = allSkinSprites.Length - 1;
        if (viewIndex >= allSkinSprites.Length) viewIndex = 0;

        if (errorMsgText != null) errorMsgText.gameObject.SetActive(false);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (DataSyncManager.Instance == null || allSkinSprites.Length == 0) return;

        var data = DataSyncManager.Instance.gameDataSO.data;
        bool isUnlocked = (viewIndex < data.characterSkins.Count) && data.characterSkins[viewIndex].isUnlocked;
        bool isEquipped = (data.currentCharacterSkinIndex == viewIndex);

        // 1. Hiển thị Skin và Tên
        skinNameText.text = allSkinSprites[viewIndex].name;
        skinDisplayImage.sprite = allSkinSprites[viewIndex];
        skinDisplayImage.SetNativeSize();
        
        // Chỉnh màu đen cho skin chưa mở khóa thay vì dùng sprite riêng
        skinDisplayImage.color = isUnlocked ? Color.white : Color.black;

        // 2. Logic Nút Unlock
        if (isUnlocked)
        {
            unlockBtnText.text = "Unlocked";
            unlockBtn.interactable = false;
            unlockBtnImage.sprite = btnDisabledSprite;
        }
        else
        {
            unlockBtnText.text = "Unlock (100)";
            unlockBtn.interactable = (data.coins >= UNLOCK_PRICE);
            unlockBtnImage.sprite = unlockBtn.interactable ? btnCanPressSprite : btnDisabledSprite;
        }

        // 3. Logic Nút Equip
        if (!isUnlocked)
        {
            equipBtnText.text = "Equip";
            equipBtn.interactable = false; // Chưa unlock thì không thể equip
            equipBtnImage.sprite = btnDisabledSprite;
        }
        else if (isEquipped)
        {
            equipBtnText.text = "Equipped";
            equipBtn.interactable = false;
            equipBtnImage.sprite = btnEquippedSprite;
        }
        else
        {
            equipBtnText.text = "Equip";
            equipBtn.interactable = true;
            equipBtnImage.sprite = btnCanPressSprite;
        }

        // 4. Cập nhật Coin
        coinText.text = data.coins.ToString();
    }

    private void OnUnlockClicked()
    {
        var data = DataSyncManager.Instance.gameDataSO.data;

        if (data.coins >= UNLOCK_PRICE)
        {
            data.coins -= UNLOCK_PRICE;
            data.characterSkins[viewIndex].isUnlocked = true;
            
            _ = DataSyncManager.Instance.SaveGameGlobal();
            UpdateUI();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ShowErrorMsg("Not enough coins!"));
        }
    }

    private void OnEquipClicked()
    {
        var data = DataSyncManager.Instance.gameDataSO.data;
        data.currentCharacterSkinIndex = viewIndex;
        
        _ = DataSyncManager.Instance.SaveGameGlobal();
        UpdateUI();
    }

    private IEnumerator ShowErrorMsg(string msg)
    {
        errorMsgText.text = msg;
        errorMsgText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        errorMsgText.gameObject.SetActive(false);
    }
}
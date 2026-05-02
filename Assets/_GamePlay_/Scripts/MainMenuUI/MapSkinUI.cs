using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class MapSkinSlot
{
    public Button button;
    public Image mapImage;
    public TextMeshProUGUI nameText;
    public GameObject highlightObj;
    public GameObject unlockTextObj;
}

public class MapSkinUI : MonoBehaviour
{
    [Header("UI Slots (3 Maps)")]
    public MapSkinSlot[] mapSlots; 

    [Header("Common UI")]
    public TextMeshProUGUI coinText; 
    public TextMeshProUGUI errorMsgText; 
    public Button backBtn;

    private const int MAP_SKIN_PRICE = 300;

    private void Start()
    {
        backBtn.onClick.AddListener(() => 
        {
            if (MenuUIManager.Instance != null) MenuUIManager.Instance.ShowDashboard();
            else gameObject.SetActive(false);
        });

        for (int i = 0; i < mapSlots.Length; i++)
        {
            int index = i; 
            if (mapSlots[i].button != null)
            {
                mapSlots[i].button.onClick.AddListener(() => OnMapSkinButtonClicked(index));
            }
        }

        if (errorMsgText != null) errorMsgText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        errorMsgText.gameObject.SetActive(false);
        RefreshUI();
    }

    private void OnMapSkinButtonClicked(int index)
    {
        if (DataSyncManager.Instance == null || DataSyncManager.Instance.gameDataSO == null) return;
        
        var data = DataSyncManager.Instance.gameDataSO.data;
        
        if (index < data.mapSkins.Count)
        {
            if (data.mapSkins[index].isUnlocked)
            {
                data.currentMapSkinIndex = index;
                _ = DataSyncManager.Instance.SaveGameGlobal();
                RefreshUI();
            }
            else
            {
                if (data.coins >= MAP_SKIN_PRICE)
                {
                    data.coins -= MAP_SKIN_PRICE;
                    data.mapSkins[index].isUnlocked = true;
                    data.currentMapSkinIndex = index; 
                    _ = DataSyncManager.Instance.SaveGameGlobal();
                    RefreshUI();
                    Ply_SoundManager.Ins.PlayFx(FxType.Coin);
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(ShowErrorMsg("Not enough coins!"));
                }
            }
        }
    }

    private void RefreshUI()
    {
        if (DataSyncManager.Instance == null || SpriteManager.Ins == null || SpriteManager.Ins.mapSkinSprites.Length == 0) return;

        var data = DataSyncManager.Instance.gameDataSO.data;
        int currentIndex = data.currentMapSkinIndex;

        if (coinText != null) coinText.text = data.coins.ToString();

        for (int i = 0; i < mapSlots.Length; i++)
        {
            // Kiểm tra theo độ dài của mapSkinSprites trong SpriteManager
            if (i >= data.mapSkins.Count || i >= SpriteManager.Ins.mapSkinSprites.Length) break;

            MapSkinSlot slot = mapSlots[i];
            bool isUnlocked = data.mapSkins[i].isUnlocked;
            Sprite mapSprite = SpriteManager.Ins.mapSkinSprites[i];

            if (slot.nameText != null) slot.nameText.text = mapSprite.name;
            if (slot.mapImage != null) 
            {
                slot.mapImage.sprite = mapSprite;
                // slot.mapImage.SetNativeSize(); 
                slot.mapImage.color = isUnlocked ? Color.white : Color.black;
            }

            if (slot.unlockTextObj != null)
            {
                slot.unlockTextObj.SetActive(!isUnlocked);
            }

            if (slot.highlightObj != null)
            {
                bool isSelected = isUnlocked && (i == currentIndex);
                slot.highlightObj.SetActive(isSelected);
            }
        }
    }

    private IEnumerator ShowErrorMsg(string msg)
    {
        if (errorMsgText != null)
        {
            errorMsgText.text = msg;
            errorMsgText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            errorMsgText.gameObject.SetActive(false);
        }
    }
}
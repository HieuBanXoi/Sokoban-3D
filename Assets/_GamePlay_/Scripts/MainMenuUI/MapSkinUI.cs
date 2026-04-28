using UnityEngine;
using UnityEngine.UI;

public class MapSkinUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button[] mapSkinButtons; 
    public Button backBtn;

    [Header("Colors")]
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public Color lockedColor = Color.gray;

    private const int MAP_SKIN_PRICE = 200;

    private void Start()
    {
        backBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowDashboard());

        for (int i = 0; i < mapSkinButtons.Length; i++)
        {
            int index = i; 
            mapSkinButtons[i].onClick.AddListener(() => OnMapSkinButtonClicked(index));
        }
    }

    private void OnEnable()
    {
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
                _=DataSyncManager.Instance.SaveGameGlobal();
                RefreshUI();
                Debug.Log($"[MapSkin] Đã trang bị Map Skin số {index}");
            }
            else
            {
                if (data.coins >= MAP_SKIN_PRICE)
                {
                    data.coins -= MAP_SKIN_PRICE;
                    data.mapSkins[index].isUnlocked = true;
                    data.currentMapSkinIndex = index;
                    
                    _=DataSyncManager.Instance.SaveGameGlobal();
                    RefreshUI();
                    
                    Debug.Log($"[MapSkin] Mua THÀNH CÔNG Map Skin số {index}. Coin còn lại: {data.coins}");
                }
                else
                {
                    Debug.LogWarning($"[MapSkin] KHÔNG ĐỦ TIỀN! Cần {MAP_SKIN_PRICE} coin nhưng bạn chỉ có {data.coins} coin.");
                }
            }
        }
    }

    private void RefreshUI()
    {
        if (DataSyncManager.Instance == null) return;

        var data = DataSyncManager.Instance.gameDataSO.data;
        int currentIndex = data.currentMapSkinIndex;

        for (int i = 0; i < mapSkinButtons.Length; i++)
        {
            Image btnImage = mapSkinButtons[i].GetComponent<Image>();
            if (btnImage != null)
            {
                if (i == currentIndex) 
                {
                    btnImage.color = selectedColor;
                }
                else if (!data.mapSkins[i].isUnlocked) 
                {
                    btnImage.color = lockedColor;
                }
                else 
                {
                    btnImage.color = defaultColor;
                }
            }
        }
    }
}
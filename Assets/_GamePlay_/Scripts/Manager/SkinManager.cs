using System.Collections.Generic;
using UnityEngine;

public class SkinManager : Ply_Singleton<SkinManager>
{
    [Header("--- MAP SKINS ---")]
    public List<MapType> mapTypes;
    public int currentMapTypeIndex = 0;
    public MapType currentMapMesh;

    [Header("--- PLAYER SKINS ---")]
    // Danh sách các Prefab/FBX của nhân vật (Barbarian, Knight, Mage...)
    public List<GameObject> playerSkinPrefabs;
    
    // Animator chung để gán cho mọi Skin (ví dụ: PlayerAnim.controller)
    public RuntimeAnimatorController sharedPlayerAnimator;
    
    public int currentPlayerSkinIndex = 0;

    public void OnInit()
    {
        // 1. ĐỌC DỮ LIỆU TỪ SCRIPTABLE OBJECT (Nếu DataSyncManager đã khởi tạo)
        if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
        {
            currentMapTypeIndex = DataSyncManager.Instance.gameDataSO.data.currentMapSkinIndex;
            currentPlayerSkinIndex = DataSyncManager.Instance.gameDataSO.data.currentCharacterSkinIndex;
            Debug.Log($"[SkinManager] Đã load Skin từ Data: Map[{currentMapTypeIndex}], Char[{currentPlayerSkinIndex}]");
        }
        else
        {
            // Dự phòng nếu test trực tiếp scene mà không qua BootScene
            currentMapTypeIndex = 0;
            currentPlayerSkinIndex = 0;
        }

        // 2. GÁN MAP MESH THEO DATA VỪA ĐỌC
        if (mapTypes.Count > 0 && currentMapTypeIndex < mapTypes.Count)
        {
            currentMapMesh = mapTypes[currentMapTypeIndex];
        }
        else
        {
            Debug.LogError("SkinManager: Lỗi gán Map! Index vượt quá số lượng MapType trong Inspector.");
        }
    }

    // ==========================================
    // QUẢN LÝ MAP SKIN
    // ==========================================
    public void SetMapType(int index)
    {
        if (index < 0 || index >= mapTypes.Count)
        {
            Debug.LogError("SkinManager: Invalid map type index!");
            return;
        }

        currentMapTypeIndex = index;
        currentMapMesh = mapTypes[currentMapTypeIndex];
    }

    public void SetMapTypeEnum(MapTypeEnum mapTypeEnum) // Giữ lại hàm cũ để tránh lỗi code hiện tại
    {
        SetMapType((int)mapTypeEnum);
    }

    // ==========================================
    // QUẢN LÝ PLAYER SKIN
    // ==========================================
    
    // UI sẽ gọi hàm này khi người chơi chọn một skin mới
    public void SetPlayerSkin(int index)
    {
        if (index < 0 || index >= playerSkinPrefabs.Count)
        {
            Debug.LogError("SkinManager: Invalid player skin index!");
            return;
        }
        currentPlayerSkinIndex = index;
    }

    /// <summary>
    /// Hàm này dùng để Spawn FBX Skin vào trong object "Graphic" của Player
    /// </summary>
    /// <param name="graphicParent">Transform của object "Graphic" (cha của skin)</param>
    /// <returns>Trả về Animator đã được setup để Player script tiếp tục điều khiển</returns>
    public Animator ApplyPlayerSkin(Transform graphicParent)
    {
        if (playerSkinPrefabs == null || playerSkinPrefabs.Count == 0)
        {
            Debug.LogWarning("SkinManager: Chưa gán Prefab Player Skin nào!");
            return null;
        }

        // 1. Xóa skin FBX cũ đang hiển thị (nếu có)
        foreach (Transform child in graphicParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Spawn skin FBX mới làm con của "Graphic"
        GameObject selectedPrefab = playerSkinPrefabs[currentPlayerSkinIndex];
        GameObject newSkinInstance = Instantiate(selectedPrefab, graphicParent);
        
        // Reset transform để khớp vị trí với Graphic
        newSkinInstance.transform.localPosition = Vector3.zero;
        newSkinInstance.transform.localRotation = Quaternion.identity;
        newSkinInstance.transform.localScale = Vector3.one;

        // 3. Xử lý Animator
        Animator anim = newSkinInstance.GetComponent<Animator>();
        if (anim == null)
        {
            // Nếu FBX gốc chưa có Animator, ta tự động thêm vào
            anim = newSkinInstance.AddComponent<Animator>();
        }

        // 4. Gán Runtime Animator Controller chung (PlayerAnim) vào FBX mới
        if (sharedPlayerAnimator != null)
        {
            anim.runtimeAnimatorController = sharedPlayerAnimator;
        }
        else
        {
            Debug.LogWarning("SkinManager: Chưa gán sharedPlayerAnimator!");
        }

        return anim;
    }
}

public enum MapTypeEnum
{
    Map1,
    Map2,
    Map3
}

[System.Serializable]
public class MapType 
{
    public Mesh onGroundMesh;
    public Mesh goalMesh;
    public Mesh wallMesh;
    public Mesh groundMesh;

    public Material normalMaterial;
    public Material iceMaterial;
}
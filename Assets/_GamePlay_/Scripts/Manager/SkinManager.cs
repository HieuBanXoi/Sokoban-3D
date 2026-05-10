using System.Collections.Generic;
using UnityEngine;

namespace Sokoban.Presentation
{
    public enum MapTypeEnum { Map1, Map2, Map3 }

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

    public class SkinManager : Ply_Singleton<SkinManager>
    {
        [Header("--- MAP SKINS ---")]
        public List<MapType> mapTypes;
        public int currentMapTypeIndex = 0;
        public MapType currentMapMesh;

        [Header("--- PLAYER SKINS ---")]
        public List<GameObject> playerSkinPrefabs;
        public RuntimeAnimatorController sharedPlayerAnimator;
        public int currentPlayerSkinIndex = 0;

        public void OnInit()
        {
            if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
            {
                currentMapTypeIndex = DataSyncManager.Instance.gameDataSO.data.currentMapSkinIndex;
                currentPlayerSkinIndex = DataSyncManager.Instance.gameDataSO.data.currentCharacterSkinIndex;
            }
            
            if (mapTypes.Count > 0 && currentMapTypeIndex < mapTypes.Count)
                currentMapMesh = mapTypes[currentMapTypeIndex];
        }

        public void SetMapType(int index)
        {
            if (index < 0 || index >= mapTypes.Count) return;
            currentMapTypeIndex = index;
            currentMapMesh = mapTypes[currentMapTypeIndex];
        }

        public void SetPlayerSkin(int index)
        {
            if (index < 0 || index >= playerSkinPrefabs.Count) return;
            currentPlayerSkinIndex = index;
        }

        public Animator ApplyPlayerSkin(Transform graphicParent)
        {
            if (playerSkinPrefabs == null || playerSkinPrefabs.Count == 0) return null;

            foreach (Transform child in graphicParent) Destroy(child.gameObject);

            GameObject selectedPrefab = playerSkinPrefabs[currentPlayerSkinIndex];
            GameObject newSkinInstance = Instantiate(selectedPrefab, graphicParent);
            
            newSkinInstance.transform.localPosition = Vector3.zero;
            newSkinInstance.transform.localRotation = Quaternion.identity;
            newSkinInstance.transform.localScale = Vector3.one;

            Animator anim = newSkinInstance.GetComponent<Animator>();
            if (anim == null) anim = newSkinInstance.AddComponent<Animator>();

            if (sharedPlayerAnimator != null) anim.runtimeAnimatorController = sharedPlayerAnimator;

            return anim;
        }
    }
}
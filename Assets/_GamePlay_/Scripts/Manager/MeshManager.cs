using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : Ply_Singleton<MeshManager>
{
    [Header("Map Type")]
    public List<MapType> mapTypes;
    public int currentMapTypeIndex = 0;
    public MapType currentMapMesh;

    void Start()
    {
        if (mapTypes.Count > 0)
        {
            currentMapMesh = mapTypes[currentMapTypeIndex];
        }
        else
        {
            Debug.LogError("MeshManager: No map types assigned in the inspector!");
        }
    }
    public void SetMapType(MapTypeEnum mapTypeEnum)
    {
        int index = (int)mapTypeEnum;
    
        if (index < 0 || index >= mapTypes.Count)
        {
            Debug.LogError("MeshManager: Invalid map type index!");
            return;
        }

        currentMapTypeIndex = index;
        currentMapMesh = mapTypes[currentMapTypeIndex];
    }

}
public enum MapTypeEnum
{
    Map1,
    Map2,
    Map3
}
[Serializable]
public class MapType 
{
    public Mesh onGroundMesh;
    public Mesh goalMesh;
    public Mesh wallMesh;
    public Mesh groundMesh;

    public Material normalMaterial;
    public Material iceMaterial;
}

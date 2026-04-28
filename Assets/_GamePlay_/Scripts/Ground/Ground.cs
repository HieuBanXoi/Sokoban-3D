using UnityEngine;

public class Ground : Ply_GameUnit
{
    [Header("Reference")]
    public GroundGraphicController groundGraphic;

    [Header("Type")]
    public GroundType groundType;


    public void SetGroundType(GroundType type)
    {
        groundType = type;
        UpdateGraphic();
    }
    public void UpdateGraphic()
    {
        if (groundGraphic == null)
        {
            Debug.LogError("Ground: No GroundGraphicController assigned!");
            return;
        }

        // Chọn mesh dựa trên loại ground
        Mesh newMesh = null;
        switch (groundType)
        {
            case GroundType.Ground:
                newMesh = SkinManager.Ins.currentMapMesh.groundMesh;
                break;
            case GroundType.OnGround:
                newMesh = SkinManager.Ins.currentMapMesh.onGroundMesh;
                break;
            case GroundType.Goal:
                newMesh = SkinManager.Ins.currentMapMesh.goalMesh;
                break;
            case GroundType.Wall:
                newMesh = SkinManager.Ins.currentMapMesh.wallMesh;
                break;
        }

        groundGraphic.SetMesh(newMesh);
    }

    public void Despawn()
    {
        Ply_Pool.Ins.Despawn(PoolType.Ground, this);
    }
}

public enum GroundType
{
    Ground,
    OnGround,
    Goal,
    Wall
}

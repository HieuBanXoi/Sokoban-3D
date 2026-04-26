using UnityEngine;

public class MergeEffect : Ply_GameUnit
{
    public void DeSpawnByTime()
    {
        Invoke(nameof(DeSpawn),3f);
    }
    public void DeSpawn()
    {
        Ply_Pool.Ins.Despawn(PoolType.MergeVFX,this);
    }
}

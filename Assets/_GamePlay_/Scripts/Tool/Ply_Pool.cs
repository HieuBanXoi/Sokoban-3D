using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PoolType { Ground, Box, Player, MergeVFX }

public class Ply_Pool : Ply_Singleton<Ply_Pool>
{
    public PoolAmount[] poolAmounts;
    public Transform poolHolder;

    [System.Serializable]
    public struct PoolAmount
    {
        public PoolType type;
        public int amount;
        public Ply_GameUnit gameUnit;
    }

    private Dictionary<PoolType, Queue<Ply_GameUnit>> dict = new Dictionary<PoolType, Queue<Ply_GameUnit>>();

    public override void Awake()
    {
        base.Awake();
        OnInit();
    }

    private void OnInit()
    {
        for (int i = 0; i < poolAmounts.Length; i++)
        {
            if (!dict.ContainsKey(poolAmounts[i].type))
            {
                dict[poolAmounts[i].type] = new Queue<Ply_GameUnit>();
            }

            for (int j = 0; j < poolAmounts[i].amount; j++)
            {
                Ply_GameUnit gameUnit = Instantiate(poolAmounts[i].gameUnit);
                gameUnit.transform.SetParent(poolHolder);
                gameUnit.gameObject.SetActive(false);
                dict[poolAmounts[i].type].Enqueue(gameUnit);
            }
        }
    }

    public Ply_GameUnit Spawn(PoolType poolType, Vector3 pos, Quaternion rot)
    {
        Ply_GameUnit gameUnit = dict[poolType].Count > 0 ? dict[poolType].Dequeue() : Instantiate(GetPrefab(poolType));
        gameUnit.transform.SetParent(poolHolder);
        
        gameUnit.tf.SetPositionAndRotation(pos, rot);
        gameUnit.gameObject.SetActive(true);

        return gameUnit;
    }

    public T Spawn<T>(PoolType poolType, Vector3 pos, Quaternion rot) where T : Ply_GameUnit
    {
        return Spawn(poolType, pos, rot) as T;
    }

    public Ply_GameUnit SpawnDirect(PoolType poolType, Vector3 pos, Quaternion rot)
    {
        Ply_GameUnit prefab = GetPrefab(poolType);
        if (prefab == null)
        {
            Debug.LogWarning($"Ply_Pool: No prefab found for PoolType {poolType}");
            return null;
        }

        Ply_GameUnit gameUnit = Instantiate(prefab, pos, rot);
        gameUnit.gameObject.SetActive(true);
        return gameUnit;
    }

    public T SpawnDirect<T>(PoolType poolType, Vector3 pos, Quaternion rot) where T : Ply_GameUnit
    {
        return SpawnDirect(poolType, pos, rot) as T;
    }

    public void Despawn(PoolType poolType, Ply_GameUnit gameUnit)
    {
        gameUnit.gameObject.SetActive(false);
        dict[poolType].Enqueue(gameUnit);
    }


    public Ply_GameUnit GetPrefab(PoolType poolType)
    {
        for (int i = 0; i < poolAmounts.Length; i++)
        {
            if (poolAmounts[i].type == poolType)
            {
                return poolAmounts[i].gameUnit;
            }
        }
        return null;
    }
}

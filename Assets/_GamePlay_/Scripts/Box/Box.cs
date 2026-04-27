using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Box : Ply_GameUnit
{
    [Header("Reference")]
    public BoxGraphicController graphic;
    public FakeGravity gravity;

    [Header("Type")]
    public BoxType boxType;
    public bool isOnGoal = false;
    public float slideSpeed = 6f;

    public void Despawn()
    {
        isOnGoal = false;
        Ply_Pool.Ins.Despawn(PoolType.Box, this);
    }
    public void SnapToGround(LayerMask groundLayer)
    {

        Vector3 origin = transform.position + Vector3.up * 1f;
        float maxDist = 6f;

        RaycastHit hit;
        bool gotHit = Physics.Raycast(origin, Vector3.down, out hit, maxDist, groundLayer.value);

        if (!gotHit)
        {
            // fallback: try without mask to help debug missing layer issues
            Debug.LogWarning("SnapToGround: no hit with groundLayer mask, trying unmasked raycast for debugging.");
            if (Physics.Raycast(origin, Vector3.down, out hit, maxDist))
            {
                Debug.LogWarning($"SnapToGround fallback hit collider '{hit.collider.name}' on layer {hit.collider.gameObject.layer}");
                gotHit = true;
            }
        }

        if (gotHit)
        {
            
            Vector3 target = new Vector3(hit.transform.position.x, transform.position.y, hit.transform.position.z);
            float duration = 0.12f;
            transform.DOMove(target, duration).OnComplete(() =>
            {
                CheckOnGoal();
            });
        }
    }

    // Overload with completion callback
    public void SnapToGround(LayerMask groundLayer, System.Action onComplete)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        float maxDist = 6f;

        RaycastHit hit;
        bool gotHit = Physics.Raycast(origin, Vector3.down, out hit, maxDist, groundLayer.value);

        if (!gotHit)
        {
            Debug.LogWarning("SnapToGround: no hit with groundLayer mask, trying unmasked raycast for debugging.");
            if (Physics.Raycast(origin, Vector3.down, out hit, maxDist))
            {
                Debug.LogWarning($"SnapToGround fallback hit collider '{hit.collider.name}' on layer {hit.collider.gameObject.layer}");
                gotHit = true;
            }
        }

        if (gotHit)
        {
            Vector3 target = new Vector3(hit.transform.position.x, transform.position.y, hit.transform.position.z);
            float duration = 0.12f;
            transform.DOMove(target, duration).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("SnapToGround: no ground found below box; forcing kinematic.");
            onComplete?.Invoke();
        }
    }

    
    public void IceSlide(Vector3 dir, int obstacleMask, LayerMask groundLayer)
    {
        if (dir.sqrMagnitude < 0.001f) return;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        float maxDist = 200f;

        bool found = Physics.Raycast(origin, dir, out hit, maxDist, obstacleMask);
        if (!found)
        {
            Debug.LogWarning("IceSlide: no wall found in direction "+dir);
            return;
        }

        Vector3 hitPoint = hit.point;
        Vector3 dirNorm = dir.normalized;
        Debug.Log($"Hit wall at {hitPoint}, dirNorm={dirNorm}");

        Vector3 target = new Vector3(hitPoint.x - dirNorm.x, transform.position.y, hitPoint.z - dirNorm.z);
        Debug.Log(target);

        float dist = Vector3.Distance(transform.position, target);
        // if (dist <= 0.01f)
        // {
        //     // already there
        //     SnapToGround(groundLayer);
        //     return;
        // }


        float duration = Mathf.Max(0.05f, dist / Mathf.Max(0.0001f, slideSpeed));
        transform.DOMove(target, duration).OnComplete(() =>
        {
            SnapToGround(groundLayer);
        });
    }
    public void SetBoxType(BoxType type)
    {
        boxType = type;
        if (graphic != null)
        {
            switch (boxType)
            {
                case BoxType.Normal:
                    graphic.SetMaterial(MeshManager.Ins.currentMapMesh.normalMaterial);
                    break;
                case BoxType.Ice:
                    graphic.SetMaterial(MeshManager.Ins.currentMapMesh.iceMaterial);
                    break;
            }
        }
    }
    public void CheckOnGoal()
    {
        if(Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f, InputManager.Ins.groundLayerMask))
        {
            Ground ground = ComponentCache<Ground>.Get(hit.collider);
            if (ground != null)
            {
                isOnGoal = ground.groundType == GroundType.Goal;
            }
        }
        CheckWinCondition();
    }
    public void CheckWinCondition()
    {
        if (isOnGoal)
        {
            Box[] boxes = LevelGenerator.Ins.boxStack.ToArray();
            foreach (Box box in boxes)
            {
                if (!box.isOnGoal) return;
            }
            // Nếu đến đây, tất cả hộp đều trên đích
            Debug.Log("Level Completed!");
        }
    }
}

public enum BoxType
{
    Normal,
    Ice
}

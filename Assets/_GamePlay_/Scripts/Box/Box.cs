using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Box : Ply_GameUnit
{
    [Header("Reference")]
    public BoxGraphicController graphic;
    public Rigidbody rb;

    [Header("Type")]
    public BoxType boxType;
    public bool isOnGoal = false;
    public float slideSpeed = 6f;

    public void Despawn()
    {
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
                if (rb != null) rb.isKinematic = true;
            });
        }
        else
        {
            Debug.LogWarning("SnapToGround: no ground found below box; forcing kinematic.");
            if (rb != null) rb.isKinematic = true;
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
        if (dist <= 0.01f)
        {
            // already there
            SnapToGround(groundLayer);
            return;
        }


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
}

public enum BoxType
{
    Normal,
    Ice
}

using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Box : Ply_GameUnit
{
    [Header("Reference")]
    public BoxGraphicController graphic;

    [Header("Type")]
    public BoxType boxType;
    public bool isOnGoal = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // allow falling first, then freeze by kinematic so boxes don't jitter
            rb.isKinematic = false;
            StartCoroutine(EnableKinematicWhenSettled());
        }
    }

    private IEnumerator EnableKinematicWhenSettled()
    {
        float timer = 0f;
        // wait up to 1s or until rigidbody sleeps
        while (timer < 1f)
        {
            if (rb == null) break;
            if (rb.IsSleeping()) break;
            timer += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
            rb.isKinematic = true;
    }

    public void SetKinematic(bool value)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = value;
    }

    /// <summary>
    /// Raycast down to ground layer and snap x,z to hit point using DOTween.
    /// Keeps current y to avoid pushing box through ground; after tween sets kinematic true.
    /// </summary>
    public void SnapToGround(LayerMask groundLayer)
    {
        Debug.Log($"SnapToGround called. groundLayer={groundLayer.value}, boxPos={transform.position}");

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

    public void Despawn()
    {
        Ply_Pool.Ins.Despawn(PoolType.Box, this);
    }

    /// <summary>
    /// For Ice boxes: cast a ray in direction until a wall is found, then move the box to the cell before that wall.
    /// Duration scales with distance and movement uses DOTween; after move it snaps to ground.
    /// </summary>
    public void IceSlide(Vector3 dir, int obstacleMask, LayerMask groundLayer, float cellSize, float slideSpeed)
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

        // ensure kinematic while tweening to avoid physics interference
        SetKinematic(true);

        float duration = Mathf.Max(0.05f, dist / Mathf.Max(0.0001f, slideSpeed));
        transform.DOMove(target, duration).OnComplete(() =>
        {
            SnapToGround(groundLayer);
        });
    }
}

public enum BoxType
{
    Normal,
    Ice
}

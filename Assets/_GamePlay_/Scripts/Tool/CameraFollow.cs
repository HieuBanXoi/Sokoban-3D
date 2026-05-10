using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("Khoảng cách từ Camera đến Player")]
    public Vector3 offset = new Vector3(0f, 5f, -7f); 
    
    [Tooltip("Độ mượt của Camera (càng nhỏ càng mượt, nhưng chậm hơn)")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Tooltip("Camera có luôn nhìn chằm chằm vào Player không?")]
    public bool lookAtTarget = true;
    [Header("Map Framing")]
    [Tooltip("If true, camera will try to frame the whole levelContainer instead of following the player")]
    public bool frameWholeMap = true;

    [Tooltip("Transform that holds generated level tiles/objects (set by LevelGenerator.levelContainer)")]
    public Transform levelContainer;

    [Tooltip("Multiplier for how far the camera should back off relative to map size")]
    public float mapViewFactor = 2f;

    [Tooltip("Extra padding in world units when framing the map")]
    public float mapPadding = 1f;

    void LateUpdate()
    {
        // If framing whole map is enabled and we have a level container, compute bounds
        if (frameWholeMap && levelContainer != null && levelContainer.childCount > 0)
        {
            Bounds bounds = new Bounds(levelContainer.GetChild(0).position, Vector3.zero);
            foreach (Transform child in levelContainer)
            {
                if (child == null) continue;
                Renderer r = child.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    bounds.Encapsulate(r.bounds);
                }
                else
                {
                    bounds.Encapsulate(child.position);
                }
            }

            Vector3 center = bounds.center;
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.z);
            float dist = maxExtent * mapViewFactor + mapPadding;

            Vector3 dir = offset.normalized;
            if (dir.magnitude < 0.001f) dir = Vector3.up + Vector3.back;

            Vector3 desiredPosition = center + dir * dist;
            Vector3 smoothedPosition = Vector3.Lerp(_tf.position, desiredPosition, smoothSpeed);
            _tf.position = smoothedPosition;

            if (lookAtTarget)
            {
                _tf.LookAt(center);
            }

            return;
        }

        // Nếu chưa gán target thì không làm gì cả để tránh lỗi
        if (target == null) return;

        // 1. Tính toán vị trí mà Camera cần đi tới (theo player)
        Vector3 desiredPositionTarget = target.position + offset;

        // 2. Dùng Vector3.Lerp để nội suy khoảng cách, tạo hiệu ứng bám theo mượt mà (smooth)
        Vector3 smoothedPositionTarget = Vector3.Lerp(_tf.position, desiredPositionTarget, smoothSpeed);
        
        // 3. Gán vị trí mới cho Camera
        _tf.position = smoothedPositionTarget;

        // 4. (Tuỳ chọn) Ép góc xoay của camera luôn hướng về phía nhân vật
        if (lookAtTarget)
        {
            _tf.LookAt(target);
        }
    }

    private Transform _tf;
    private void Awake()
    {
        _tf = transform;
    }
}
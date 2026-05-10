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
        ResetState();
        Ply_Pool.Ins.Despawn(PoolType.Box, this);
    }
    public void ResetState()
    {
        isOnGoal = false;
        tf.position = Vector3.one;
        tf.rotation = Quaternion.identity;
        tf.localScale = Vector3.one;
    }
    public void SnapToGround(LayerMask groundLayer)
    {

        Vector3 origin = tf.position + Vector3.up * 1f;
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
            
            Vector3 target = new Vector3(hit.transform.position.x, tf.position.y, hit.transform.position.z);
            float duration = 0.12f;
            tf.DOMove(target, duration).OnComplete(() =>
            {
                CheckOnGoal();
            });
        }
    }

    // Overload with completion callback
    public void SnapToGround(LayerMask groundLayer, System.Action onComplete)
    {
        Vector3 origin = tf.position + Vector3.up * 1f;
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
            Vector3 target = new Vector3(hit.transform.position.x, tf.position.y, hit.transform.position.z);
            float duration = 0.12f;
            tf.DOMove(target, duration).OnComplete(() =>
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
        Vector3 origin = tf.position + Vector3.up * 0.5f;
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

        Vector3 target = new Vector3(hitPoint.x - dirNorm.x, tf.position.y, hitPoint.z - dirNorm.z);
        Debug.Log(target);

        float dist = Vector3.Distance(tf.position, target);
        // if (dist <= 0.01f)
        // {
        //     // already there
        //     SnapToGround(groundLayer);
        //     return;
        // }


        float duration = Mathf.Max(0.05f, dist / Mathf.Max(0.0001f, slideSpeed));
        tf.DOMove(target, duration).OnComplete(() =>
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
                    graphic.SetMaterial(SkinManager.Ins.currentMapMesh.normalMaterial);
                    break;
                case BoxType.Ice:
                    graphic.SetMaterial(SkinManager.Ins.currentMapMesh.iceMaterial);
                    break;
            }
        }
    }
    public void CheckOnGoal()
    {
        if(Physics.Raycast(tf.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f, InputManager.Ins.groundLayerMask))
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
            GameManager.Ins.SetState(GameManager.GameState.Win); // Chặn input ngay lập tức khi phát hiện chiến thắng
            GameManager.Ins.player.movement.animator.SetBool("isCheering", true);
            // Nếu đến đây, tất cả hộp đều trên đích
            Sequence seq = DOTween.Sequence();

            float flyDuration = 0.5f;   // Thời gian bay lên
            float flyHeight = 3f;       // Độ cao bay lên
            float shrinkDuration = 0.2f;// Thời gian thu nhỏ
            float staggerTime = 0.15f;  // Độ trễ giữa các hộp nổ (bay lên lần lượt)
            float delayBeforeWin = 1.0f;// Khoảng thời gian đợi sau khi toàn bộ hiệu ứng xong

            for (int i = 0; i < boxes.Length; i++)
            {
                Box box = boxes[i];
                float startTime = i * staggerTime; // Tính toán thời điểm bắt đầu của hộp này

                // 1. Bay lên cao
                seq.Insert(startTime, box.tf.DOMoveY(box.tf.position.y + flyHeight, flyDuration).SetEase(Ease.OutQuad));

                // 2. Vừa bay vừa xoay 360 độ
                seq.Insert(startTime, box.tf.DORotate(new Vector3(0, 360, 0), flyDuration, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.OutQuad));

                // Tính thời điểm hộp bay đến đỉnh
                float apexTime = startTime + flyDuration;

                // 3. Đến đỉnh thì thu nhỏ dần biến mất
                seq.Insert(apexTime, box.tf.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack));

                // 4. Cũng tại đỉnh, sinh ra hiệu ứng Effect
                seq.InsertCallback(apexTime, () =>
                {
                    Ply_SoundManager.Ins.PlayFx(FxType.DoneEffect); // Phát âm thanh chiến thắng
                    // Truyền vị trí hiện tại của box (lúc này đang ở trên không)
                    SpawnEffect(box.tf.position); 
                });
            }

            // Đợi thêm một khoảng thời gian sau khi toàn bộ Sequence chạy xong
            seq.AppendInterval(delayBeforeWin);

            // Khi toàn bộ mọi thứ kết thúc, mới gọi UI Win
            seq.OnComplete(() =>
            {
                if (GameplayUIManager.Ins != null)
                {
                    Ply_SoundManager.Ins.PlayFx(FxType.Win);
                    GameplayUIManager.Ins.OnLevelCompleted();
                }
                else
                {
                    Debug.Log("Level Completed! (No UI Manager found)");
                }
            });
        }
    }
    public void SpawnEffect(Vector3 position)
    {
        MergeEffect effect = Ply_Pool.Ins.Spawn<MergeEffect>(PoolType.MergeVFX, position, Quaternion.identity);
        effect.DeSpawnByTime();
    }
}

public enum BoxType
{
    Normal,
    Ice
}

using UnityEngine;
using System;
using DG.Tweening;
using Sokoban.Core.Interfaces;
using Sokoban.Presentation;
using Sokoban.Managers;


namespace Sokoban.Entities
{
    public class Box : Ply_GameUnit, IPushable
    {
        [Header("Reference")]
        public BoxGraphicController graphic;
        public FakeGravity gravity;

        [Header("Type")]
        public BoxType boxType;
        public bool isOnGoal = false;
        public float slideSpeed = 6f;

        // Sự kiện thông báo khi trạng thái thay đổi
        public static event Action<Box> OnBoxStateChanged;

        public Transform GetTransform() => tf;
        public BoxType GetBoxType() => boxType;

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

        public bool CanBePushed(Vector3 direction)
        {
            Vector3 boxOrigin = tf.position + Vector3.up * 0.5f;
            // Ép cứng 2f là gridSize, bạn có thể đưa vào cấu hình chung nếu cần
            if (Physics.Raycast(boxOrigin, direction, 2f, InputManager.Ins.boxLayerMask | InputManager.Ins.groundLayerMask))
            {
                return false; 
            }
            return true;
        }

        public void Push(Vector3 direction, float moveDuration)
        {
            Vector3 targetBoxPos = tf.position + direction * 2f;
            tf.DOMove(targetBoxPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                CheckOnGoal();
            });
        }

        public void IceSlide(Vector3 dir, int obstacleMask, LayerMask groundLayer)
        {
            if (dir.sqrMagnitude < 0.001f) return;
            Vector3 origin = tf.position + Vector3.up * 0.5f;
            RaycastHit hit;
            float maxDist = 200f;

            bool found = Physics.Raycast(origin, dir, out hit, maxDist, obstacleMask);
            if (!found) return;

            Vector3 hitPoint = hit.point;
            Vector3 dirNorm = dir.normalized;
            Vector3 target = new Vector3(hitPoint.x - dirNorm.x, tf.position.y, hitPoint.z - dirNorm.z);

            float dist = Vector3.Distance(tf.position, target);
            float duration = Mathf.Max(0.05f, dist / Mathf.Max(0.0001f, slideSpeed));
            
            tf.DOMove(target, duration).OnComplete(() =>
            {
                SnapToGround(groundLayer);
            });
        }

        public void SnapToGround(LayerMask groundLayer)
        {
            Vector3 origin = tf.position + Vector3.up * 1f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 6f, groundLayer.value))
            {
                Vector3 target = new Vector3(hit.transform.position.x, tf.position.y, hit.transform.position.z);
                tf.DOMove(target, 0.12f).OnComplete(() =>
                {
                    CheckOnGoal();
                });
            }
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
            bool previousState = isOnGoal;

            if(Physics.Raycast(tf.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f, InputManager.Ins.groundLayerMask))
            {
                Ground ground = ComponentCache<Ground>.Get(hit.collider);
                if (ground != null)
                {
                    isOnGoal = ground.groundType == GroundType.Goal;
                }
            }

            if (previousState != isOnGoal)
            {
                OnBoxStateChanged?.Invoke(this);
            }
        }

        // Tách hoạt ảnh Win ra để GameManager gọi
        public Sequence PlayWinAnimation(float delayTime)
        {
            Sequence seq = DOTween.Sequence();
            float flyDuration = 0.5f;
            float flyHeight = 3f;
            float shrinkDuration = 0.2f;

            seq.Insert(delayTime, tf.DOMoveY(tf.position.y + flyHeight, flyDuration).SetEase(Ease.OutQuad));
            seq.Insert(delayTime, tf.DORotate(new Vector3(0, 360, 0), flyDuration, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.OutQuad));
            
            float apexTime = delayTime + flyDuration;
            seq.Insert(apexTime, tf.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack));
            seq.InsertCallback(apexTime, () =>
            {
                Ply_SoundManager.Ins.PlayFx(FxType.DoneEffect);
                SpawnEffect(tf.position); 
            });
            return seq;
        }

        private void SpawnEffect(Vector3 position)
        {
            MergeEffect effect = Ply_Pool.Ins.Spawn<MergeEffect>(PoolType.MergeVFX, position, Quaternion.identity);
            effect.DeSpawnByTime();
        }
    }
}
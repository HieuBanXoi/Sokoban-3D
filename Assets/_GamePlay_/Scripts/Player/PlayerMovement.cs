using UnityEngine;
using DG.Tweening;
using Sokoban.Core.Interfaces;
using Sokoban.Core.Patterns;

namespace Sokoban.Entities
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Parent References")]
        public Transform playerTransform;

        [Header("Animator Reference")]
        public Animator animator;

        [Header("Grid Movement Settings")]
        public float gridSize = 2f;
        public float moveDuration = 0.2f;
        public bool isMoving = false;

        [Header("Layer Masks")] // Kéo thả trực tiếp trong Inspector
        public LayerMask boxLayerMask;
        public LayerMask groundLayerMask;

        public void AttemptMove(Vector3 direction)
        {
            if (isMoving) return;

            playerTransform.forward = direction;
            Vector3 targetPos = playerTransform.position + direction * gridSize;
            Vector3 origin = playerTransform.position + Vector3.up * 0.5f;

            // 1. Kiểm tra chướng ngại vật
            if (Physics.Raycast(origin, direction, gridSize, groundLayerMask))
            {
                return; 
            }

            // 2. Kiểm tra vật thể có thể đẩy
            if (Physics.Raycast(origin, direction, out RaycastHit hit, gridSize, boxLayerMask))
            {
                IPushable pushableObj = hit.collider.GetComponent<IPushable>();
                
                if (pushableObj != null)
                {
                    if (!pushableObj.CanBePushed(direction)) return;

                    CommandManager.Ins.AddCommand(new MoveCommand(playerTransform, pushableObj.GetTransform()));

                    if (pushableObj.GetBoxType() == BoxType.Ice)
                    {
                        animator.SetTrigger("isKicking");
                        isMoving = true;
                        DOVirtual.DelayedCall(0.7f, () => 
                        {
                            pushableObj.IceSlide(direction, boxLayerMask | groundLayerMask, groundLayerMask);
                            if (animator != null) animator.SetBool("isWalking", true);

                            playerTransform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                isMoving = false;
                                if (animator != null) animator.SetBool("isWalking", false);
                            });
                        });
                        return; 
                    }

                    // Xử lý đẩy hộp thường
                    isMoving = true;
                    Ply_SoundManager.Ins.PlayFx(FxType.Pop); 
                    if (animator != null) animator.SetBool("isPushing", true);

                    pushableObj.Push(direction, moveDuration);

                    playerTransform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        isMoving = false; 
                        if (animator != null)
                        {
                            animator.SetBool("isWalking", false);
                            animator.SetBool("isPushing", false);
                        }
                    });
                }
            }
            else
            {
                // 3. Nếu ô trước mặt trống
                CommandManager.Ins.AddCommand(new MoveCommand(playerTransform));
                
                isMoving = true;
                Ply_SoundManager.Ins.PlayFx(FxType.Pop);
                if (animator != null) animator.SetBool("isWalking", true);

                playerTransform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    isMoving = false;
                    if (animator != null)
                    {
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isPushing", false);
                    }
                });
            }
        }
    }
}
using UnityEngine;
using DG.Tweening; // Bắt buộc phải có thư viện này

public class PlayerMovement : MonoBehaviour
{
    [Header("Parent References")]
    public Transform playerTransform;

    [Header("Animator Reference")]
    public Animator animator;

    [Header("Grid Movement Settings")]
    public float gridSize = 2f;
    public float moveDuration = 0.2f; // Thời gian đi hết 1 ô
    
    public bool isMoving = false;

    private LayerMask boxLayerMask;
    private LayerMask groundLayerMask;

    private void Start()
    {
        gridSize = 2f;
        boxLayerMask = InputManager.Ins.boxLayerMask;
        groundLayerMask = InputManager.Ins.groundLayerMask;
    }

    public void AttemptMove(Vector3 direction)
    {
        // Chặn input nếu nhân vật đang trong quá trình di chuyển (tween chưa xong)
        if (isMoving) return;

        playerTransform.forward = direction;

        Vector3 targetPos = playerTransform.position + direction * gridSize;
        Vector3 origin = playerTransform.position + Vector3.up * 0.5f;

        // 1. Kiểm tra chướng ngại vật
        if (Physics.Raycast(origin, direction, gridSize, groundLayerMask))
        {
            return; 
        }

        // 2. Kiểm tra có Box hay không
        if (Physics.Raycast(origin, direction, out RaycastHit hit, gridSize, boxLayerMask))
        {
            Box box = ComponentCache<Box>.Get(hit.collider);
            if (box != null)
            {
                // Kiểm tra phía sau box có khoảng trống để đẩy không
                Vector3 boxOrigin = box.transform.position + Vector3.up * 0.5f;
                if (Physics.Raycast(boxOrigin, direction, gridSize, boxLayerMask | groundLayerMask))
                {
                    return; // Vướng tường/hộp khác -> không đi được
                }
                if (box.boxType == BoxType.Ice)
                {
                    CommandManager.Ins.AddCommand(new MoveCommand(playerTransform, box));
                    animator.SetTrigger("isKicking");
                    isMoving = true;
                    DOVirtual.DelayedCall(0.7f, () => 
                    {
                        box.IceSlide(direction, boxLayerMask | groundLayerMask, groundLayerMask);

                        if (animator != null) animator.SetBool("isWalking", true);

                        playerTransform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            isMoving = false; // Mở khóa input khi ĐÃ ĐI TỚI NƠI
                            if (animator != null) animator.SetBool("isWalking", false);
                        });
                    });
                    if (!TutorialController.Ins.isHintModeActive)
                    {
                        TutorialController.Ins.isHintMode = false;
                    }
                    return; 
                }

                

                CommandManager.Ins.AddCommand(new MoveCommand(playerTransform, box));
                Vector3 targetBoxPos = box.transform.position + direction * gridSize;
                
                // Kích hoạt DOTween cho cả Player và Box
                PerformDOTweenMove(targetPos, box.transform, targetBoxPos);
            }
        }
        else
        {
            // 3. Nếu ô trước mặt trống
            CommandManager.Ins.AddCommand(new MoveCommand(playerTransform));
            PerformDOTweenMove(targetPos, null, Vector3.zero);
        }
        if (!TutorialController.Ins.isHintModeActive)
        {
            TutorialController.Ins.isHintMode = false;
        }

    }

    private void PerformDOTweenMove(Vector3 targetPlayerPos, Transform boxTransform, Vector3 targetBoxPos)
    {
        isMoving = true;
        Ply_SoundManager.Ins.PlayFx(FxType.Pop); // Phát âm thanh bước chân (hoặc đẩy hộp)
        // Bật animation
        if (animator != null) 
        {
            if (boxTransform != null) animator.SetBool("isPushing", true);
            else animator.SetBool("isWalking", true);
        }

        // Nếu có hộp, di chuyển hộp cùng lúc với player
        if (boxTransform != null)
        {
            boxTransform.DOMove(targetBoxPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                // Sau khi hộp di chuyển xong, kiểm tra xem có rơi vào vị trí Goal không
                Box boxComponent = ComponentCache<Box>.Get(boxTransform);
                if (boxComponent != null)
                {
                    boxComponent.CheckOnGoal();
                }
            });
        }

        // Di chuyển Player và xử lý logic kết thúc thông qua OnComplete
        playerTransform.DOMove(targetPlayerPos, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            isMoving = false; // Mở khóa để nhận input tiếp theo
            
            // Tắt animation
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isPushing", false);
            }
        });
    }
}
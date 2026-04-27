using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Parent References")]
    public Transform playerTransform;

    [Header("Animator Reference")]
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed;
    public float defaultMoveSpeed = 8f;
    public float pushMoveSpeed = 4f;
    public float playerCheckDistance = 0.8f;
    public float boxCheckDistance = 2f; 
    public float iceSlideSpeed = 6f;
    public bool isObstacleBefore = false;
    [Header("Box Push Settings")]
    public bool isPushing = false;
    public Box currentlyPushedBox = null;
    private LayerMask boxLayerMask;
    private LayerMask groundLayerMask;

    private void Start()
    {
        moveSpeed = defaultMoveSpeed;
        boxLayerMask = InputManager.Ins.boxLayerMask;
        groundLayerMask = InputManager.Ins.groundLayerMask;
    }
    public void Move(Vector3 direction)
    {
        if (direction.magnitude >= 0.1f)
        {
            Vector3 move = direction * moveSpeed * Time.deltaTime;
            if(isPushing && playerTransform.forward != direction)
            {
                StopPushing();
            }
            playerTransform.forward = direction;
            float checkDistance = isPushing ? boxCheckDistance : playerCheckDistance;
            Vector3 rayOrigin = isPushing ? currentlyPushedBox.transform.position + Vector3.up * 0.5f : playerTransform.position + Vector3.up * 0.5f;
            if(Physics.Raycast(rayOrigin, direction, checkDistance, boxLayerMask | groundLayerMask))
            {
                isObstacleBefore = true;
                if (animator != null) animator.SetBool("isWalking", false);
                if(isPushing)
                {
                    StopPushing();
                }
                return;
            }
            isObstacleBefore = false;
            playerTransform.Translate(move, Space.World);

            // MỚI THÊM: Nếu đang có hướng di chuyển, bật anim Walk
            animator.SetBool("isWalking", true);
        }
        else
        {
            // MỚI THÊM: Nếu không có phím nào được bấm, tắt anim Walk về Idle
            animator.SetBool("isWalking", false);
        }
    }
    public void HandlePushInput()
    {
        if(currentlyPushedBox != null)
        {
            StopPushing();
            return;
        }
        // Raycast để kiểm tra xem có hộp nào ở phía trước không
        Vector3 origin = playerTransform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(origin, playerTransform.forward, out hit, boxCheckDistance, boxLayerMask))
        {
            Box box = ComponentCache<Box>.Get(hit.collider);
            Transform boxTransform = box.transform;
            if(box == null || box.boxType == BoxType.Ice)
            {
                box.IceSlide(playerTransform.forward, boxLayerMask|groundLayerMask, groundLayerMask);
                animator.SetTrigger("isKicking");
                return;
            }
            moveSpeed = pushMoveSpeed;
            isPushing = true;
            currentlyPushedBox = box;
            boxTransform.SetParent(playerTransform);
            animator.SetBool("isPushing", true);

        }
    }
    public void StopPushing()
    {
        moveSpeed = defaultMoveSpeed;
        if (currentlyPushedBox != null)
        {
            currentlyPushedBox.transform.SetParent(null);
            currentlyPushedBox.SnapToGround(groundLayerMask);
            currentlyPushedBox = null;
        }
        isPushing = false;
        if (animator != null) animator.SetBool("isPushing", false);
    }
}

using UnityEngine;

public class InputManager : Ply_Singleton<InputManager>
{
    [Header("Reference")]
    public Player player;
    [Header("Push Settings")]
    public LayerMask boxLayerMask;
    public LayerMask wallLayerMask;
    public LayerMask groundLayerMask;
    public float pushDetectDistance = 2f; // khoảng cách raycast kiểm tra hộp
    public float pushCellSize = 2f;
    public float iceSlideSpeed = 6f; // units per second used to compute duration


    private void Update()
    {
        if (!GameManager.Ins.isPlaying)
        {
            return;
        }
        if (player == null || player.movement == null) return;

        // 1. Nhận sự kiện di chuyển (WASD hoặc Phím mũi tên)
        // GetAxisRaw trả về các giá trị -1, 0, 1 giúp nhân vật dừng lại ngay lập tức khi nhả phím
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement: prioritize larger axis
        if (Mathf.Abs(horizontal) > 0f && Mathf.Abs(vertical) > 0f)
        {
            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
                vertical = 0f;
            else
                horizontal = 0f;
        }

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Luôn truyền Vector di chuyển xuống, kể cả khi đứng yên (Vector3.zero)
        player.movement.Move(moveDirection);

        // Handle push input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandlePushInput();
        }

        // build obstacle mask (walls + other boxes)
        int obstacleMask = wallLayerMask.value | boxLayerMask.value;

        // If currently pushing, check automatic release conditions
        if (player.IsPushing())
        {
            // If player not facing the box anymore OR there is a wall immediately in front of the box, release
            Box current = player.GetPushingBox();
            if (current == null)
            {
                player.ForceStopPushing();
            }
            else
            {
                Vector3 toBox = (current.transform.position - player.transform.position).normalized;
                float dot = Vector3.Dot(player.transform.forward, toBox);
                if (dot < 0.7f)
                {
                    // not facing sufficiently towards box
                    ReleasePush();
                }
                else
                {
                    // check obstacle (wall or other box) in front of box
                    if (Physics.Raycast(current.transform.position, player.transform.forward, pushDetectDistance, obstacleMask))
                    {
                        ReleasePush();
                    }
                }
            }
        }
    }

    private void HandlePushInput()
    {
        // If already pushing -> release
        if (player.IsPushing())
        {
            ReleasePush();
            return;
        }

        // Cast forward from player to detect a box
        Vector3 origin = player.transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(origin, player.transform.forward, out hit, pushDetectDistance, boxLayerMask))
        {
            Box box = hit.collider.GetComponent<Box>();
            if (box == null) return;
            // build combined obstacle mask (wall + boxes)
            int obstacleMask = wallLayerMask.value | boxLayerMask.value;

            // If ice box, slide until first obstacle instead of normal pushing
            if (box.boxType == BoxType.Ice)
            {
                Vector3 dir = player.transform.forward.normalized;
                box.IceSlide(dir, obstacleMask, groundLayerMask, pushCellSize, iceSlideSpeed);
                return;
            }

            // Check that space in front of box is free of obstacle for normal boxes
            if (Physics.Raycast(box.transform.position, player.transform.forward, pushDetectDistance, obstacleMask))
            {
                // front blocked
                Debug.Log("Cannot push: obstacle in front of box");
                return;
            }

            // Start pushing for normal box
            if (player.movement != null && player.movement.playerRb != null)
            {
                player.StartPushing(box, player.movement.playerRb);
                box.SetKinematic(true);
            }
        }
    }

    private void ReleasePush()
    {
        Box box = player.StopPushing();
        if (box != null)
        {
            // Snap box x,z to nearest ground point and kinematic it
            box.SnapToGround(groundLayerMask);
            box.SetKinematic(true);
        }
        if (player.movement != null && player.movement.animator != null) player.movement.animator.SetBool("isPushing", false);
    }
}

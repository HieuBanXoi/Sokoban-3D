using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Parent References")]
    public Transform playerTransform; // Transform của Player cha
    public Rigidbody playerRb;        // Rigidbody của Player cha để xử lý trọng lực/nhảy

    [Header("Animator Reference")]
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;



    public void Move(Vector3 direction)
    {
        if (direction.magnitude >= 0.1f)
        {
            Vector3 move = direction * moveSpeed * Time.deltaTime;
            playerTransform.Translate(move, Space.World);
            playerTransform.forward = direction;

            // MỚI THÊM: Nếu đang có hướng di chuyển, bật anim Walk
            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            // MỚI THÊM: Nếu không có phím nào được bấm, tắt anim Walk về Idle
            if (animator != null) animator.SetBool("isWalking", false);
        }
    }

}

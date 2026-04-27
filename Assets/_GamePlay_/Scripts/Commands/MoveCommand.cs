using UnityEngine;
using DG.Tweening;

public interface ICommand
{
    void Undo();
}

public class MoveCommand : ICommand
{
    private Transform playerTransform;
    private Vector3 playerStartPos;
    private Vector3 playerStartForward; // Lưu cả hướng quay mặt của player
    
    private Transform boxTransform;
    private Vector3 boxStartPos;

    // Constructor lưu lại trạng thái NGAY TRƯỚC KHI thực hiện hành động
    public MoveCommand(Transform player, Box pushedBox = null)
    {
        this.playerTransform = player;
        this.playerStartPos = player.position;
        this.playerStartForward = player.forward;

        if (pushedBox != null)
        {
            this.boxTransform = pushedBox.transform;
            this.boxStartPos = pushedBox.transform.position;
        }
    }

    public void Undo()
    {
        // 1. Ép dừng DOTween hiện tại (tránh lỗi lôi kéo vật thể nếu đang lở dở)
        playerTransform.DOKill();
        
        // 2. Kéo Player lùi về vị trí cũ
        playerTransform.DOMove(playerStartPos, 0.15f).SetEase(Ease.OutQuad);
        playerTransform.forward = playerStartForward;

        // 3. Nếu có hộp bị đẩy, kéo hộp về vị trí cũ
        if (boxTransform != null)
        {
            boxTransform.DOKill();
            boxTransform.DOMove(boxStartPos, 0.15f).SetEase(Ease.OutQuad);
        }
    }
}

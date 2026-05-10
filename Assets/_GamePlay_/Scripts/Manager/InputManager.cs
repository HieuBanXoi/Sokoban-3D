using UnityEngine;
using Sokoban.Entities;

namespace Sokoban.Managers
{
    public class InputManager : Ply_Singleton<InputManager>
    {
        [Header("Reference")]
        public Player player;
        
        [Header("Global Settings")]
        public LayerMask boxLayerMask;
        public LayerMask groundLayerMask;

        public override void Awake() 
        {
            base.Awake();
            // Khởi tạo trễ hoặc gán từ Editor để tránh lỗi NullReference
        }

        private void Update()
        {
            if (GameManager.Ins == null || !GameManager.Ins.IsInputEnabled) return;
            if (player == null) return;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                MoveUp();
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                MoveDown();
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                MoveLeft();
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                MoveRight();
        }

        public void MoveUp() { if (GameManager.Ins.IsInputEnabled) player.movement.AttemptMove(Vector3.forward); }
        public void MoveDown() { if (GameManager.Ins.IsInputEnabled) player.movement.AttemptMove(Vector3.back); }
        public void MoveLeft() { if (GameManager.Ins.IsInputEnabled) player.movement.AttemptMove(Vector3.left); }
        public void MoveRight() { if (GameManager.Ins.IsInputEnabled) player.movement.AttemptMove(Vector3.right); }
    }
}
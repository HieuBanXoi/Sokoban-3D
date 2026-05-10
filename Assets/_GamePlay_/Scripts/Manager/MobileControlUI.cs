using UnityEngine;
using UnityEngine.UI;
using Sokoban.Managers; // Gọi đến InputManager

namespace Sokoban.Presentation.UI
{
    public class MobileControlUI : MonoBehaviour
    {
        [Header("Movement Buttons")]
        public Button btnUp;
        public Button btnDown;
        public Button btnLeft;
        public Button btnRight;

        private void Start()
        {
            if (InputManager.Ins != null)
            {
                btnUp.onClick.AddListener(() => InputManager.Ins.MoveUp());
                btnDown.onClick.AddListener(() => InputManager.Ins.MoveDown());
                btnLeft.onClick.AddListener(() => InputManager.Ins.MoveLeft());
                btnRight.onClick.AddListener(() => InputManager.Ins.MoveRight());
            }
            else
            {
                Debug.LogError("[MobileControlUI] Không tìm thấy InputManager trong Scene!");
            }
        }
    }
}
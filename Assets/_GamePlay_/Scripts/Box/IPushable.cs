using UnityEngine;

namespace Sokoban.Core.Interfaces
{
    public interface IPushable
    {
        bool CanBePushed(Vector3 direction);
        void Push(Vector3 direction, float moveDuration);
        void IceSlide(Vector3 direction, int obstacleMask, LayerMask groundLayer);
        Transform GetTransform();
        BoxType GetBoxType(); // Định nghĩa BoxType có thể di chuyển vào 1 file Enum chung sau
    }
}

public enum BoxType
    {
        Normal,
        Ice
    }
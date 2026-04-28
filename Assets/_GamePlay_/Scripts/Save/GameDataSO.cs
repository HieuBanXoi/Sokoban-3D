using UnityEngine;

[CreateAssetMenu(fileName = "GameDataSO", menuName = "Save/GameDataSO")]
public class GameDataSO : ScriptableObject
{
    // GameData sống trên RAM trong runtime, UI / Gameplay đọc ghi trực tiếp vào đây
    public GameData data = new GameData();
}

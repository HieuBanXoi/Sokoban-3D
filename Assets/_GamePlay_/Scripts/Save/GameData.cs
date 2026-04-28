using System;
using System.Collections.Generic;

[System.Serializable]
public class SkinStatus
{
    public string skinID;
    public bool isUnlocked;
}

[System.Serializable]
public class LevelStatus
{
    public int levelIndex;
    public bool isUnlocked;
    public bool isCompleted;
}

[System.Serializable]
public class GameData
{
    public int coins;
    
    // THÊM MỚI: Biến lưu vị trí Skin đang được chọn
    public int currentCharacterSkinIndex; 
    public int currentMapSkinIndex;
    public int currentPlayingLevel;       

    public List<SkinStatus> characterSkins = new List<SkinStatus>();
    public List<SkinStatus> mapSkins = new List<SkinStatus>();
    public List<LevelStatus> levels = new List<LevelStatus>();
    public long lastSaveTime;

    public GameData()
    {
        coins = 100;
        lastSaveTime = DateTime.UtcNow.Ticks;

        // 1. MẶC ĐỊNH TÀI KHOẢN MỚI SẼ DÙNG SKIN ĐẦU TIÊN (INDEX = 0)
        currentCharacterSkinIndex = 0;
        currentMapSkinIndex = 0;
        currentPlayingLevel = 1; // Mặc định bắt đầu từ level 1

        // 2. Khởi tạo 50 map (mở 10 map đầu) - Dùng i từ 1 đến 50 cho logic Level
        for (int i = 1; i <= 50; i++)
        {
            levels.Add(new LevelStatus { 
                levelIndex = i, 
                isUnlocked = (i <= 10), 
                isCompleted = false 
            });
        }

        // 3. Khởi tạo 10 skin nhân vật (mở 2 skin đầu). i chạy từ 0 để map với List Index
        for (int i = 0; i < 10; i++)
        {
            characterSkins.Add(new SkinStatus { 
                skinID = "CharSkin_" + i, 
                isUnlocked = (i < 2) // i = 0 và i = 1 sẽ là true
            });
        }

        // 4. Khởi tạo 3 skin map (mở 1 skin đầu)
        for (int i = 0; i < 3; i++)
        {
            mapSkins.Add(new SkinStatus { 
                skinID = "MapSkin_" + i, 
                isUnlocked = (i == 0) // Chỉ mở i = 0
            });
        }
    }
}
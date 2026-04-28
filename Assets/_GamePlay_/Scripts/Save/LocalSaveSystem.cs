using System;
using System.IO;
using UnityEngine;

public static class LocalSaveSystem
{
    private const string GuestId = "guest_offline";

    private static string GetFileName(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) playerId = GuestId;
        return $"player_data_{playerId}.json";
    }

    private static string GetFilePath(string playerId)
    {
        string fileName = GetFileName(playerId);
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static bool SaveLocal(string playerId, GameData data)
    {
        try
        {
            if (data == null) data = new GameData();
            // Cập nhật thời gian lưu trước khi serialize
            data.lastSaveTime = DateTime.UtcNow.Ticks;

            string json = JsonUtility.ToJson(data, true);
            string path = GetFilePath(playerId);
            File.WriteAllText(path, json);
            Debug.Log($"[LocalSave] Saved to {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LocalSave] Save failed: {ex}");
            return false;
        }
    }

    public static GameData LoadLocal(string playerId)
    {
        try
        {
            string path = GetFilePath(playerId);
            if (!File.Exists(path))
            {
                Debug.Log($"[LocalSave] No local save found at {path}. Returning new GameData.");
                return new GameData();
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return new GameData();

            GameData data = JsonUtility.FromJson<GameData>(json);
            if (data == null) return new GameData();
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LocalSave] Load failed: {ex}");
            return new GameData();
        }
    }
}

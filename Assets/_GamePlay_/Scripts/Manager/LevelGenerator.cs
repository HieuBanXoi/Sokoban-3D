using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json; // Yêu cầu package "Json.NET" trong Unity
using System.IO;

// --- CÁC CLASS DATA ĐỂ ĐỌC JSON ---
[System.Serializable]
public class GridPos
{
    public int row;
    public int col;
}

[System.Serializable]
public class LevelData
{
    public int levelId;
    public int width;
    public int height;
    public int score;
    public int depth;
    public string solution;
    public List<string> rowsStr;
    public List<List<int>> rowsInt;
    public GridPos playerPos;
    public List<GridPos> goals;
    public List<GridPos> boxes;
    public List<GridPos> iceBoxes;
}

public class LevelGenerator : Ply_Singleton<LevelGenerator>
{
    [Header("Data Input")]
    public int levelIdToLoad = 1; // Nếu muốn load theo id thay vì gán file trực tiếp, điền id vào đây và gọi GenerateMapByLevel(levelIdToLoad)
    [ReadOnly] public string currentSolution; // Lưu solution để check sau này

    [Header("Environment Setup (2x2x2)")]
    public float gridSize = 2f; // Kích thước cube

    [Header("ObjectStackHolder")]

    public Stack<Ground> groundStack = new Stack<Ground>(); // Stack để quản lý các Ground đã spawn, giúp dễ dàng đẩy lên khi spawn Box/Player và kéo xuống khi despawn
    public Player spawnedPlayerComp; // Tham chiếu đến player đã spawn để dễ dàng quản lý
    public Stack<Box> boxStack = new Stack<Box>(); // Stack để quản lý các Box đã spawn, giúp dễ dàng đẩy lên khi spawn Box mới và kéo xuống khi despawn
    private Transform _mapContainer;
    public Transform levelContainer;

    Vector3 playerSpawnOffset = new Vector3(0, -1, 0);
    [ContextMenu("Generate Level")]
    public void GenerateMap()
    {
        GenerateMapByLevel(levelIdToLoad);
    }

    public void GenerateMapByLevel(int levelId)
    {
        if (levelIdToLoad != levelId)
        {
            if (TutorialController.Ins != null) 
                TutorialController.Ins.ResetHintLimit();
        }

        levelIdToLoad = levelId;
        string resourcePath = $"_GamePlay_/Level/level_{levelId:0000}"; // Resources path without extension
        string json = null;

        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta != null)
        {
            json = ta.text;
        }
        else
        {
            string filePath = Path.Combine(Application.dataPath, "_GamePlay_", "Level", $"level_{levelId:0000}.json");
            if (File.Exists(filePath))
            {
                json = File.ReadAllText(filePath);
            }
        }

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError($"Không tìm thấy file level cho id={levelId}. Hãy đặt file dưới Resources/_GamePlay_/Level hoặc Assets/_GamePlay_/Level.");
            return;
        }

        GenerateMapFromJson(json);
    }

    private void GenerateMapFromJson(string json)
    {

        DespawnMap();
        CommandManager.Ins.Clear();

        // 1. Parse JSON dùng Newtonsoft
        LevelData data = JsonConvert.DeserializeObject<LevelData>(json);
        currentSolution = data.solution; // Lưu lại chuỗi hướng dẫn giải

        // Tạo một folder trống để gom gọn rác trong Hierarchy
        if(_mapContainer == null)
        {
            _mapContainer = new GameObject($"Level_{data.levelId}_Environment").transform;
        }
        _mapContainer.parent = this.levelContainer;

        spawnedPlayerComp = null;

        // 2. Duyệt qua mảng rowsInt để sinh map
        for (int row = 0; row < data.height; row++)
        {
            for (int col = 0; col < data.width; col++)
            {
                int cellValue = data.rowsInt[row][col];

                // Trục X tương ứng Col, Trục Z tương ứng -Row (để map sinh từ trên xuống dưới giống mảng)
                Vector3 pos = new Vector3(col * gridSize, 0, -row * gridSize);

                // Nếu SpawnCell cần trả về player, handle ở đây
                if (cellValue == 5 || cellValue == 6)
                {
                    Ground ground = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, pos, Quaternion.identity); // Sinh Ground bên dưới để player đứng lên
                    ground.SetGroundType(GroundType.OnGround); // Đặt loại Ground là OnGround để hiển thị mesh khác
                    groundStack.Push(ground);
                    ground.transform.SetParent(_mapContainer);

                    // Lớp trên: player
                    Vector3 layer1Pos = pos + Vector3.up * gridSize;
                    spawnedPlayerComp = Ply_Pool.Ins.Spawn<Player>(PoolType.Player, layer1Pos+playerSpawnOffset, Quaternion.identity);
                    spawnedPlayerComp.transform.SetParent(levelContainer); // Gắn player ra ngoài _mapContainer để dễ quản lý riêng
                }
                else
                {
                    SpawnCell(cellValue, pos);
                }
            }
        }

        // Nếu có GameManager trong scene, gán player và gọi OnInit để khởi tạo các tham chiếu
        if (spawnedPlayerComp != null)
        {
            try
            {
                if (GameManager.Ins != null)
                {
                    GameManager.Ins.player = spawnedPlayerComp;
                    GameManager.Ins.LoadNewMap(); // Gọi LoadNewMap để khởi tạo lại camera và input với player mới
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Không thể gán player vào GameManager hoặc gọi OnInit(): {ex.Message}");
            }
        }

        // Invoke(nameof(StartTutorialFromCurrentSolution), 1f); // Đợi một frame để đảm bảo mọi thứ đã khởi tạo xong rồi mới bắt đầu tutorial
        TutorialController.Ins.StartTutorialFromCurrentSolution();
        Debug.Log($"Sinh map thành công! Solution: {currentSolution}");
    }
    private void StartTutorialFromCurrentSolution()
    {
        TutorialController.Ins.StartTutorialFromCurrentSolution();
    }
    private void SpawnCell(int value, Vector3 position)
    {
        // Tính toạ độ Y cho lớp phía trên (Cao lên 1 ô / gridSize)
        Vector3 layer1Pos = position + Vector3.up * gridSize;

        // 1. XỬ LÝ LỚP ĐÁY (Y = 0)
        // Nếu là vị trí có đích (2, 4, 6, 8), lót prefab Đích
        if (value == 2 || value == 4 || value == 6 || value == 8)
        {
            Ground ground = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, position, Quaternion.identity); // Sinh Ground bên dưới để player đứng lên
            ground.SetGroundType(GroundType.Goal);
            groundStack.Push(ground);
            ground.transform.SetParent(_mapContainer);
        }
        else
        {
            Ground ground = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, position, Quaternion.identity); // Sinh Ground bên dưới để player đứng lên
            ground.SetGroundType(GroundType.OnGround);
            groundStack.Push(ground);
            ground.transform.SetParent(_mapContainer);
        }

        // 2. XỬ LÝ LỚP TRÊN (Y = 2)
        // Sinh Tường
        if (value == 0)
        {
            Ground ground = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, layer1Pos, Quaternion.identity); // Sinh Ground bên dưới để player đứng lên
            ground.SetGroundType(GroundType.Wall);
            groundStack.Push(ground);
            ground.transform.SetParent(_mapContainer);
        }
        // Sinh Normal Box (nằm trên sàn hoặc nằm trên đích)
        else if (value == 3 || value == 4)
        {
            Box box = Ply_Pool.Ins.Spawn<Box>(PoolType.Box, layer1Pos, Quaternion.identity);
            box.transform.SetParent(levelContainer);
            boxStack.Push(box);
            box.SetBoxType(BoxType.Normal);
            Physics.SyncTransforms();
            box.gravity.StartFalling();
            box.CheckOnGoal();
        }
        // Sinh Player (nằm trên sàn hoặc nằm trên đích)
        else if (value == 5 || value == 6)
        {
            // Player đã được handle riêng ở phần GenerateMapFromJson để dễ quản lý hơn, nên không spawn ở đây nữa
        }
        // Sinh Ice Box (nằm trên sàn hoặc nằm trên đích)
        else if (value == 7 || value == 8)
        {
            Box box = Ply_Pool.Ins.Spawn<Box>(PoolType.Box, layer1Pos, Quaternion.identity);
            box.transform.SetParent(levelContainer);
            boxStack.Push(box);
            box.SetBoxType(BoxType.Ice);
            Physics.SyncTransforms();
            box.gravity.StartFalling();
            box.CheckOnGoal();
        }
    }

    public void DespawnMap()
    {
        // Despawn tất cả Box trước để tránh lỗi khi despawn Ground đang có Box đứng trên
        while (boxStack.Count > 0)
        {
            Box box = boxStack.Pop();
            box.Despawn();
        }

        // Despawn tất cả Ground
        while (groundStack.Count > 0)
        {
            Ground ground = groundStack.Pop();
            ground.Despawn();
        }

        // Despawn Player nếu có
        if (spawnedPlayerComp != null)
        {
            spawnedPlayerComp.Despawn();
            spawnedPlayerComp = null;
        }

    }

    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        // Xoá tất cả các map cũ đi để gen map mới không bị đè lên nhau
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

// Gắn attribute ReadOnly để xem solution trên Inspector nhưng không bị sửa nhầm
public class ReadOnlyAttribute : PropertyAttribute { }
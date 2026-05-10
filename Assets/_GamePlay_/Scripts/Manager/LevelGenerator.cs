using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sokoban.Entities;
using Sokoban.Core.Patterns;
using Sokoban.Presentation;

namespace Sokoban.Managers
{
    [System.Serializable]
    public class GridPos { public int row; public int col; }

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
        public int levelIdToLoad = 1; 

        [Header("Environment Setup")]
        public float gridSize = 2f; 

        [Header("Decorations")]
        public List<GameObject> decorPrefabs; 
        [Range(0f, 1f)] public float decorSpawnChance = 0.3f; 
        public float decorYOffset = 1f; 
        private List<GameObject> spawnedDecors = new List<GameObject>(); 

        [Header("ObjectStackHolder")]
        public Stack<Ground> groundStack = new Stack<Ground>(); 
        public Player spawnedPlayerComp; 
        public Stack<Box> boxStack = new Stack<Box>(); 
        
        private Transform _mapContainer;
        public Transform levelContainer;

        Vector3 playerSpawnOffset = new Vector3(0, -1, 0);

        private void Start()
        {
            SkinManager.Ins.OnInit(); 
            if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
            {
                int levelToPlay = DataSyncManager.Instance.gameDataSO.data.currentPlayingLevel;
                GenerateMapByLevel(levelToPlay); 
            }
            else
            {
                GenerateMapByLevel(levelIdToLoad);
            }
        }

        public void ReloadCurrentLevel()
        {
            GenerateMapByLevel(levelIdToLoad);
        }

        public void GenerateMapByLevel(int levelId)
        {
            levelIdToLoad = levelId;
            string resourcePath = $"level_{levelId:0000}"; 
            string json = null;

            TextAsset ta = Resources.Load<TextAsset>(resourcePath);
            if (ta != null) json = ta.text;

            if (string.IsNullOrEmpty(json)) return;

            GenerateMapFromJson(json);
        }

        private void GenerateMapFromJson(string json)
        {
            DespawnMap();
            CommandManager.Ins.Clear();

            LevelData data = JsonConvert.DeserializeObject<LevelData>(json);
            if (TutorialController.Ins != null)
            {
                TutorialController.Ins.currentSolution = data.solution;
            }

            if(_mapContainer == null)
            {
                _mapContainer = new GameObject($"Level_{data.levelId}_Environment").transform;
            }
            _mapContainer.parent = this.levelContainer;

            spawnedPlayerComp = null;

            for (int row = 0; row < data.height; row++)
            {
                for (int col = 0; col < data.width; col++)
                {
                    int cellValue = data.rowsInt[row][col];
                    Vector3 pos = new Vector3(col * gridSize, 0, -row * gridSize);

                    if (cellValue == 5 || cellValue == 6)
                    {
                        Ground ground = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, pos, Quaternion.identity); 
                        ground.SetGroundType(cellValue == 6 ? GroundType.Goal : GroundType.OnGround); 
                        groundStack.Push(ground);
                        ground.transform.SetParent(_mapContainer);

                        Vector3 layer1Pos = pos + Vector3.up * gridSize;
                        spawnedPlayerComp = Ply_Pool.Ins.Spawn<Player>(PoolType.Player, layer1Pos+playerSpawnOffset, new Quaternion(0, 180, 0, 1)); 
                        spawnedPlayerComp.graphic.ApplySkin(); 
                        spawnedPlayerComp.transform.SetParent(levelContainer); 
                    }
                    else
                    {
                        SpawnCell(cellValue, pos);
                    }
                }
            }

            if (spawnedPlayerComp != null && GameManager.Ins != null)
            {
                GameManager.Ins.LoadNewMap(spawnedPlayerComp); 
            }
            if (TutorialController.Ins != null)
            {
                TutorialController.Ins.StartTutorialFromCurrentSolution();
            }
        }
        
        private void SpawnCell(int value, Vector3 position)
        {
            Vector3 layer1Pos = position + Vector3.up * gridSize;

            Ground ground = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, position, Quaternion.identity); 
            groundStack.Push(ground);
            ground.transform.SetParent(_mapContainer);

            if (value == 2 || value == 4 || value == 6 || value == 8) ground.SetGroundType(GroundType.Goal);
            else if (value == 0) ground.SetGroundType(GroundType.Ground);
            else ground.SetGroundType(GroundType.OnGround);

            if (value == 0) 
            {
                Ground wall = Ply_Pool.Ins.Spawn<Ground>(PoolType.Ground, layer1Pos, Quaternion.identity); 
                wall.SetGroundType(GroundType.Wall);
                groundStack.Push(wall);
                wall.transform.SetParent(_mapContainer);

                if (decorPrefabs != null && decorPrefabs.Count > 0 && Random.value <= decorSpawnChance)
                {
                    GameObject randomDecor = decorPrefabs[Random.Range(0, decorPrefabs.Count)];
                    Vector3 decorPos = layer1Pos + Vector3.up * decorYOffset; 
                    GameObject decorInstance = Instantiate(randomDecor, decorPos, Quaternion.identity, _mapContainer);
                    spawnedDecors.Add(decorInstance); 
                }
            }
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
            while (boxStack.Count > 0) boxStack.Pop().Despawn();
            while (groundStack.Count > 0) groundStack.Pop().Despawn();
            
            if (spawnedPlayerComp != null)
            {
                spawnedPlayerComp.Despawn();
                spawnedPlayerComp = null;
            }
            foreach (GameObject decor in spawnedDecors)
            {
                if (decor != null) Destroy(decor);
            }
            spawnedDecors.Clear();
        }
    }
    public class ReadOnlyAttribute : PropertyAttribute { }
}
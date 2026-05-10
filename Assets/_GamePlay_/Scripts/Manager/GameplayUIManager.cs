using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Threading.Tasks;
using Sokoban.Managers;
using Sokoban.Core.Patterns;
// Nếu DataSyncManager của bạn đang ở namespace khác (ví dụ Sokoban.Core.Data), 
// hãy uncomment dòng dưới đây:
// using Sokoban.Core.Data; 

namespace Sokoban.Presentation.UI
{
    public class GameplayUIManager : Ply_Singleton<GameplayUIManager>
    {
        [Header("Gameplay Info")]
        public TextMeshProUGUI currentLevelText;

        [Header("Gameplay Buttons")]
        public Button solveBtn;
        public Button hintBtn;
        public Button undoBtn;
        public Button reloadBtn;
        public Button settingBtn;

        [Header("Panels")]
        public GameObject settingPanel;
        public GameObject levelCompletePanel; 
        public Transform levelCompleteTf;   
        public GameObject loadingPanel;       

        [Header("Win UI Elements")]
        public Button nextLevelBtn;
        public Button playAgainBtn;
        public Button winBackToMenuBtn;
        public TextMeshProUGUI coinText;

        private bool isLevelWon = false; 
        private const int HINT_PRICE = 10;
        private const int WIN_REWARD = 100;

        private void OnEnable()
        {
            GameManager.OnWinSequenceComplete += OnLevelCompleted;
        }

        private void OnDisable()
        {
            GameManager.OnWinSequenceComplete -= OnLevelCompleted;
        }
        private void Start()
        {
            // 1. GÁN SỰ KIỆN CƠ BẢN
            undoBtn.onClick.AddListener(() => CommandManager.Ins.Undo());
            reloadBtn.onClick.AddListener(OnClickReload);
            settingBtn.onClick.AddListener(() => settingPanel.SetActive(true));

            // 2. LOGIC NÚT SOLVE
            var data = DataSyncManager.Instance.gameDataSO.data;
            int currentLevelIndex = data.currentPlayingLevel;
            if (currentLevelText != null)
            {
                currentLevelText.text = "Level " + currentLevelIndex.ToString();
            }
            LevelStatus currentStatus = data.levels.Find(l => l.levelIndex == currentLevelIndex);
            
            if (currentStatus != null && currentStatus.isCompleted)
            {
                solveBtn.interactable = true;
                solveBtn.onClick.AddListener(() => TutorialController.Ins.Play());
            }
            else
            {
                solveBtn.interactable = false; 
            }

            // 3. LOGIC NÚT HINT
            hintBtn.onClick.AddListener(OnHintClicked);

            // 4. GÁN SỰ KIỆN CHO MÀN HÌNH WIN
            nextLevelBtn.onClick.AddListener(OnNextLevelClicked);
            playAgainBtn.onClick.AddListener(OnPlayAgainClicked);
            winBackToMenuBtn.onClick.AddListener(OnBackToMenuClicked);

            UpdateCoinDisplay();
        }
    
        private void OnClickReload()
        {
            if(!GameManager.Ins.IsInputEnabled) return;
            LevelGenerator.Ins.GenerateMapByLevel(LevelGenerator.Ins.levelIdToLoad);
        }

        private async void OnHintClicked()
        {
            var data = DataSyncManager.Instance.gameDataSO.data;

            if (data.coins >= HINT_PRICE)
            {
                data.coins -= HINT_PRICE;
                UpdateCoinDisplay();
                TutorialController.Ins.OnClickHintButton();

                ShowLoading(true);
                await DataSyncManager.Instance.SaveGameGlobal();
                ShowLoading(false);
            }
            else
            {
                Debug.LogWarning("Không đủ 10 coin để dùng Gợi ý!");
            }
        }

        // HÀM NÀY SẼ ĐƯỢC GỌI KHI WIN SEQUENCE CỦA GAMEMANAGER CHẠY XONG
        public async void OnLevelCompleted()
        {
            if (isLevelWon) return; 
            isLevelWon = true;
            
            levelCompleteTf.localScale = Vector3.zero; 
            levelCompletePanel.SetActive(true);
            levelCompleteTf.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); 

            var data = DataSyncManager.Instance.gameDataSO.data;
            int currentLevelIndex = data.currentPlayingLevel;
            LevelStatus currentStatus = data.levels.Find(l => l.levelIndex == currentLevelIndex);

            if (currentStatus != null && !currentStatus.isCompleted)
            {
                currentStatus.isCompleted = true;
                data.coins += WIN_REWARD;

                // LOGIC KIỂM TRA VÀ MỞ KHÓA MÀN
                if (currentLevelIndex % 10 == 0) 
                {
                    bool isAllPreviousCompleted = true;
                    int startLevelOfBlock = currentLevelIndex - 9; 

                    for (int i = startLevelOfBlock; i < currentLevelIndex; i++)
                    {
                        LevelStatus prevLevel = data.levels.Find(l => l.levelIndex == i);
                        if (prevLevel == null || !prevLevel.isCompleted)
                        {
                            isAllPreviousCompleted = false;
                            break; 
                        }
                    }

                    if (isAllPreviousCompleted)
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            int nextIndexToUnlock = currentLevelIndex + i;
                            LevelStatus statusToUnlock = data.levels.Find(l => l.levelIndex == nextIndexToUnlock);
                            if (statusToUnlock != null)
                            {
                                statusToUnlock.isUnlocked = true;
                            }
                        }
                        Debug.Log($"Đã mở khóa từ màn {currentLevelIndex + 1} đến {currentLevelIndex + 10}!");
                    }
                }

                await DataSyncManager.Instance.SaveGameGlobal();
            }

            UpdateCoinDisplay();
            
            if (nextLevelBtn != null)
            {
                int nextLevel = currentLevelIndex + 1;
                LevelStatus nextStatus = data.levels.Find(l => l.levelIndex == nextLevel);
                nextLevelBtn.interactable = (nextStatus != null && nextStatus.isUnlocked);
            }
        }

        private void OnNextLevelClicked()
        {
            var data = DataSyncManager.Instance.gameDataSO.data;
            int nextLevel = data.currentPlayingLevel + 1;

            LevelStatus nextStatus = data.levels.Find(l => l.levelIndex == nextLevel);
            
            if (nextStatus != null && nextStatus.isUnlocked)
            {
                data.currentPlayingLevel = nextLevel;
                ShowLoading(true);
                SceneManager.LoadScene("MainScene");
            }
        }

        private void OnPlayAgainClicked()
        {
            ShowLoading(true);
            SceneManager.LoadScene("MainScene"); 
        }

        private async void OnBackToMenuClicked()
        {
            ShowLoading(true);
            if (DataSyncManager.Instance != null) await DataSyncManager.Instance.SaveGameGlobal();
            
            // Xóa lỗi MenuRouteManager do thiếu thư viện/class hoặc thay thế bằng PlayerPrefs nếu cần
            MenuRouteManager.isReturningFromGame = true; 
            SceneManager.LoadScene("MainMenuScene");
        }

        public void UpdateCoinDisplay()
        {
            if (DataSyncManager.Instance != null && DataSyncManager.Instance.gameDataSO != null)
            {
                coinText.text = DataSyncManager.Instance.gameDataSO.data.coins.ToString();
            }
        }

        public void ShowLoading(bool isShow)
        {
            if (loadingPanel != null) loadingPanel.SetActive(isShow);
        }
    }
}
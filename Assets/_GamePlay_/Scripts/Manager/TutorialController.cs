using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : Ply_Singleton<TutorialController>
{
    [Header("Tutorial Settings")]
    public float stepPause = 0.05f; 
    public float fallWaitTime = 1f; // Thời gian chờ hộp rơi xong khi reset map

    // Cấu trúc lưu trữ dữ liệu từng bước đi
    private struct StepData {
        public Vector3 dir;
        public bool isPush;
    }

    private List<StepData> moveSteps = new List<StepData>();
    private Coroutine playRoutine = null;
    public bool isHintMode;
    public bool isHintModeActive;
    
    [SerializeField] private int hintPushLimit = 0;

    // QUAN TRỌNG: Index luôn lấy từ số lượng Command đã thực hiện để chống lệch nhịp khi Undo
    private int CurrentStepIndex 
    {
        get 
        {
            if (CommandManager.Ins != null) return CommandManager.Ins.CommandCount;
            return 0;
        }
    }
    public override void Awake()
    {
        base.Awake();
        fallWaitTime = 1f;
    }
    /// <summary>
    /// Khởi tạo danh sách bước đi từ chuỗi Solution của Level
    /// </summary>
    public void StartTutorialFromCurrentSolution()
    {
        moveSteps.Clear();
        
        string s = LevelGenerator.Ins != null ? LevelGenerator.Ins.currentSolution : null;
        if (string.IsNullOrEmpty(s)) return;

        foreach (char c in s)
        {
            Vector3 dir = CharToDir(c);
            if (dir != Vector3.zero) 
            {
                // Chữ in hoa trong solution mặc định là bước đẩy thùng
                moveSteps.Add(new StepData { dir = dir, isPush = char.IsUpper(c) });
            }
        }
        Pause();
    }

    private Vector3 CharToDir(char c)
    {
        char lower = char.ToLower(c);
        switch (lower)
        {
            case 'r': return Vector3.right;
            case 'l': return Vector3.left;
            case 'u': return Vector3.forward; 
            case 'd': return Vector3.back;
            default: return Vector3.zero;
        }
    }

    // --- CÁC HÀM ĐIỀU KHIỂN CHÍNH ---

    public void Play()
    {
        if (playRoutine != null) return;
        if(isHintMode)
        {
            playRoutine = StartCoroutine(PlayAllRoutine());
        }
        else
        {
            LevelGenerator.Ins.ReloadCurrentLevel();
            playRoutine = StartCoroutine(PlayAllRoutine());
        }
        isHintMode = false;
    }

    public void OnClickHintButton()
    {
        if (playRoutine != null) return; 
        isHintMode = true;
        isHintModeActive = true;
        playRoutine = StartCoroutine(HintRoutine());
    }

    public void Pause()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    public void ResetHintLimit()
    {
        hintPushLimit = 0;
    }

    // --- CÁC COROUTINE XỬ LÝ LOGIC ---

    private IEnumerator PlayAllRoutine()
    {
        GameManager.Ins.isPlaying = false;
        yield return Yielders.Get(fallWaitTime);
        // Chế độ giải hết: truyền -1 để không giới hạn số thùng đẩy
        yield return StartCoroutine(CorePlayRoutine(-1));
        playRoutine = null;
        GameManager.Ins.isPlaying = true;
    }

    private IEnumerator HintRoutine()
    {
        GameManager.Ins.isPlaying = false;
        // 1. Tăng giới hạn đẩy thùng cho lần gợi ý này
        hintPushLimit++;

        // 2. Tải lại map để robot bắt đầu từ trạng thái sạch
        LevelGenerator.Ins.ReloadCurrentLevel();

        // 3. Chờ cho hộp rơi xuống và ổn định vị trí (Sử dụng Yielders tối ưu)
        yield return Yielders.Get(fallWaitTime);

        // 4. Chạy các bước di chuyển đến khi đạt giới hạn
        yield return StartCoroutine(CorePlayRoutine(hintPushLimit));
        isHintModeActive = false;
        playRoutine = null;
        GameManager.Ins.isPlaying = true;
    }

    private IEnumerator CorePlayRoutine(int targetPushLimit)
    {
        int pushedBoxCount = 0;

        // Vòng lặp chạy qua từng bước dựa trên vị trí hiện tại của Player
        while (CurrentStepIndex < moveSteps.Count)
        {
            StepData currentStep = moveSteps[CurrentStepIndex];
            if (currentStep.isPush) pushedBoxCount++;

            // Thực hiện di chuyển và đợi đến khi hoàn tất
            yield return StartCoroutine(ExecuteStep(currentStep.dir));

            // Kiểm tra điều kiện dừng của chế độ Gợi ý
            if (targetPushLimit != -1 && pushedBoxCount >= targetPushLimit)
            {
                break;
            }
        }

    }

    private IEnumerator ExecuteStep(Vector3 direction)
    {
        var playerMove = InputManager.Ins.player.movement;
        if (playerMove == null) yield break;

        // Ra lệnh cho nhân vật di chuyển (Hàm này đã có sẵn logic CommandManager và DOTween)
        playerMove.AttemptMove(direction);
        
        // Đợi cho đến khi nhân vật kết thúc mọi hoạt ảnh/di chuyển
        yield return new WaitWhile(() => playerMove.isMoving);
        
        // Nghỉ một chút giữa các bước cho mượt
        yield return Yielders.Get(stepPause);
    }

}
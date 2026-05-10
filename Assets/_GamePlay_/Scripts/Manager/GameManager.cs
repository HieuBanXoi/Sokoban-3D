using UnityEngine;
using DG.Tweening;
using Sokoban.Entities;
using Sokoban.Managers.States;

namespace Sokoban.Managers
{
    public class GameManager : Ply_Singleton<GameManager>
    {
        public Player player;
        public CameraFollow cameraFollow;

        // Quản lý State Pattern
        private IGameState currentState;
        public bool IsInputEnabled { get; set; } = false;
        
        public PlayingState playingState = new PlayingState();
        public WinState winState = new WinState();

        // Compatibility cho UI/Menu cũ
        public bool IsHintMode { get; set; } = false;
        public bool IsHintModeActive { get; set; } = false;
        public static event System.Action OnWinSequenceComplete;
        private void OnEnable()
        {
            // Lắng nghe Event từ Box
            Box.OnBoxStateChanged += HandleBoxStateChanged;
        }

        private void OnDisable()
        {
            Box.OnBoxStateChanged -= HandleBoxStateChanged;
        }

        private void Start()
        {
            SetState(playingState);
        }

        private void Update()
        {
            currentState?.UpdateState(this);
        }

        public void SetState(IGameState newState)
        {
            currentState?.ExitState(this);
            currentState = newState;
            currentState?.EnterState(this);
        }

        // Thay cho GameManager.GameState.Win cũ
        public void ChangeToWinState()
        {
            SetState(winState);
        }

        private void HandleBoxStateChanged(Box changedBox)
        {
            if (currentState == winState) return; 

            Box[] boxes = LevelGenerator.Ins.boxStack.ToArray();
            foreach (Box box in boxes)
            {
                if (!box.isOnGoal) return; 
            }

            ChangeToWinState();
        }

        public void TriggerWinSequence()
        {
            player.movement.animator.SetBool("isCheering", true);

            Box[] boxes = LevelGenerator.Ins.boxStack.ToArray();
            Sequence winSeq = DOTween.Sequence();
            float staggerTime = 0.15f;

            for (int i = 0; i < boxes.Length; i++)
            {
                Sequence boxSeq = boxes[i].PlayWinAnimation(i * staggerTime);
                winSeq.Join(boxSeq);
            }

            winSeq.AppendInterval(1.0f);
            winSeq.OnComplete(() =>
            {
                Ply_SoundManager.Ins.PlayFx(FxType.Win);
                OnWinSequenceComplete?.Invoke();
            });
        }

        public void LoadNewMap(Player player)
        {
            if (InputManager.Ins != null)
            {
                InputManager.Ins.player = player;
            }
            this.player = player;
            cameraFollow.target = player.TF;
            SetState(playingState); // Reset state khi nạp map mới
        }
    }
}
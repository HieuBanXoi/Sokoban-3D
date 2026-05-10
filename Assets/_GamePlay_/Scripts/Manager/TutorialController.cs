using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sokoban.Core.Patterns;
using Sokoban.Entities;
using Sokoban.Managers.States;
using Unity.Collections;

namespace Sokoban.Managers
{
    public class TutorialController : Ply_Singleton<TutorialController>
    {
        [Header("Tutorial Settings")]
        public float stepPause = 0.05f; 
        public float fallWaitTime = 1f;

        private struct StepData 
        {
            public Vector3 dir;
            public bool isPush;
        }

        private List<StepData> moveSteps = new List<StepData>();
        private Coroutine playRoutine = null;
        
        // Đưa các biến quản lý Hint về đúng class của nó
        public bool IsHintMode { get; set; } = false; 
        public bool IsHintModeActive { get; set; } = false; 

        [ReadOnly] public string currentSolution; 
        
        [SerializeField] private int hintPushLimit = 0;

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

        public void StartTutorialFromCurrentSolution()
        {
            moveSteps.Clear();
            
            string s = !string.IsNullOrEmpty(currentSolution) ? currentSolution : null;
            if (string.IsNullOrEmpty(s)) return;

            foreach (char c in s)
            {
                Vector3 dir = CharToDir(c);
                if (dir != Vector3.zero) 
                {
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

        public void Play()
        {
            if (playRoutine != null) return;
            if(IsHintMode)
            {
                playRoutine = StartCoroutine(PlayAllRoutine());
            }
            else
            {
                LevelGenerator.Ins.ReloadCurrentLevel();
                playRoutine = StartCoroutine(PlayAllRoutine());
            }
            IsHintMode = false;
        }

        public void OnClickHintButton()
        {
            if (playRoutine != null) return; 
            IsHintMode = true;
            IsHintModeActive = true;
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

        private IEnumerator PlayAllRoutine()
        {
            // Thay thế enum cũ bằng State Pattern
            GameManager.Ins.SetState(new SolvingState());
            yield return Yielders.Get(fallWaitTime);
            
            yield return StartCoroutine(CorePlayRoutine(-1));
            playRoutine = null;
            
            // Trả lại trạng thái Playing
            GameManager.Ins.SetState(GameManager.Ins.playingState);
        }

        private IEnumerator HintRoutine()
        {
            GameManager.Ins.SetState(new HintingState());
            hintPushLimit++;

            LevelGenerator.Ins.ReloadCurrentLevel();
            yield return Yielders.Get(fallWaitTime);

            yield return StartCoroutine(CorePlayRoutine(hintPushLimit));
            
            IsHintModeActive = false;
            playRoutine = null;
            
            GameManager.Ins.SetState(GameManager.Ins.playingState);
        }

        private IEnumerator CorePlayRoutine(int targetPushLimit)
        {
            int pushedBoxCount = 0;

            while (CurrentStepIndex < moveSteps.Count)
            {
                StepData currentStep = moveSteps[CurrentStepIndex];
                if (currentStep.isPush) pushedBoxCount++;

                yield return StartCoroutine(ExecuteStep(currentStep.dir));

                if (targetPushLimit != -1 && pushedBoxCount >= targetPushLimit)
                {
                    break;
                }
            }
        }

        private IEnumerator ExecuteStep(Vector3 direction)
        {
            if (GameManager.Ins == null || GameManager.Ins.player == null) yield break;
            var playerMove = GameManager.Ins.player.movement;
            if (playerMove == null) yield break;

            playerMove.AttemptMove(direction);
            
            yield return new WaitWhile(() => playerMove.isMoving);
            yield return Yielders.Get(stepPause);
        }
    }
}
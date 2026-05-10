using System.Collections.Generic;
using UnityEngine;
using Sokoban.Core.Interfaces;
using Sokoban.Managers;

namespace Sokoban.Core.Patterns
{
    public class CommandManager : Ply_Singleton<CommandManager>
    {
        private Stack<ICommand> commandHistory = new Stack<ICommand>();
        public int CommandCount => commandHistory.Count;
        
        public void Clear()
        {
            commandHistory.Clear();
        }

        public void AddCommand(ICommand command)
        {
            if (GameManager.Ins == null || GameManager.Ins.player == null || GameManager.Ins.player.movement.isMoving) return;
            commandHistory.Push(command);
        }

        public bool CanUndo()
        {
            return commandHistory.Count > 0;
        }

        public void Undo()
        {
            if(!GameManager.Ins.IsInputEnabled) return;
            if (!CanUndo()) return;
            
            if (GameManager.Ins != null && GameManager.Ins.player != null)
            {
                GameManager.Ins.player.movement.isMoving = false;
                if (GameManager.Ins.player.movement.animator != null)
                {
                    GameManager.Ins.player.movement.animator.SetBool("isWalking", false);
                    GameManager.Ins.player.movement.animator.SetBool("isPushing", false);
                }
            }

            ICommand lastCommand = commandHistory.Pop();
            lastCommand.Undo();
        }
    }
}
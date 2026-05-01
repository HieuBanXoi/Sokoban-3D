using System.Collections.Generic;
using UnityEngine;

public class CommandManager : Ply_Singleton<CommandManager>
{
    // Dùng Stack (Vào sau ra trước) là cấu trúc dữ liệu chuẩn nhất cho Undo
    private Stack<ICommand> commandHistory = new Stack<ICommand>();
    public int CommandCount => commandHistory.Count;
    public void Clear()
    {
        commandHistory.Clear();
    }

    public void AddCommand(ICommand command)
    {
        // Chặn không cho lưu nếu player đang lơ lửng di chuyển
        if (InputManager.Ins != null && InputManager.Ins.player.movement.isMoving) return;
        
        commandHistory.Push(command);
    }

    public bool CanUndo()
    {
        return commandHistory.Count > 0;
    }

    public void Undo()
    {
        if(!GameManager.Ins.isPlaying) return;
        if (!CanUndo())
        {
            Debug.LogWarning("CommandManager: Không có nước đi nào để Undo!");
            return;
        }
        Debug.Log($"CommandManager: Undo lệnh thứ {commandHistory.Count}");
        // Mở khóa input và tắt animation cho Player nếu lỡ bấm Undo lúc đang đi
        if (InputManager.Ins != null && InputManager.Ins.player != null)
        {
            InputManager.Ins.player.movement.isMoving = false;
            if (InputManager.Ins.player.movement.animator != null)
            {
                InputManager.Ins.player.movement.animator.SetBool("isWalking", false);
                InputManager.Ins.player.movement.animator.SetBool("isPushing", false);
            }
        }

        // Lấy lệnh gần nhất ra và hoàn tác
        ICommand lastCommand = commandHistory.Pop();
        lastCommand.Undo();
    }
}
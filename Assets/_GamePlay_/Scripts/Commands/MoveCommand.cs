using UnityEngine;
using DG.Tweening;
using Sokoban.Core.Interfaces;

namespace Sokoban.Core.Patterns
{
    public class MoveCommand : ICommand
    {
        private Transform playerTransform;
        private Vector3 playerStartPos;
        private Vector3 playerStartForward;
        
        private Transform pushedTransform;
        private Vector3 pushedStartPos;

        public MoveCommand(Transform player, Transform pushedObj = null)
        {
            this.playerTransform = player;
            this.playerStartPos = player.position;
            this.playerStartForward = player.forward;

            if (pushedObj != null)
            {
                this.pushedTransform = pushedObj;
                this.pushedStartPos = pushedObj.position;
            }
        }

        public void Undo()
        {
            playerTransform.DOKill();
            playerTransform.DOMove(playerStartPos, 0.15f).SetEase(Ease.OutQuad);
            playerTransform.forward = playerStartForward;

            if (pushedTransform != null)
            {
                pushedTransform.DOKill();
                pushedTransform.DOMove(pushedStartPos, 0.15f).SetEase(Ease.OutQuad);
            }
        }
    }
}
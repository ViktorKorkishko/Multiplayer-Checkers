using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Checker : MonoBehaviour
{
    public CheckerColor CheckerColor;
    public CheckerType CheckerType = CheckerType.Default;

    public bool IsForcedToMove(Checker[,] board, Vector2Int position)
    {
        if (CheckerColor == CheckerColor.White || CheckerType == CheckerType.King)
        {
            // can kill by going top left
            if (position.x >= 2 && position.y <= 5)
            {
                Debug.Log(position);
                Checker middleChecker = board[position.x - 1, position.y + 1];
                
                if (middleChecker && middleChecker.CheckerColor != CheckerColor)
                {
                    //check if is is possible to land after the jump
                    if (board[position.x - 2, position.y + 2] == null)
                    {
                        return true;
                    }
                }
            }

            // can kill by going top right
            if (position.x <= 5 && position.y <= 5)
            {
                Checker middleChecker = board[position.x + 1, position.y + 1];
                if (middleChecker && middleChecker.CheckerColor != CheckerColor)
                {
                    //check if is is possible to land ater the jump
                    if (board[position.x + 2, position.y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }
        else if(CheckerColor == CheckerColor.Black || CheckerType == CheckerType.King)
        {
            // can kill by going bottom left
            if (position.x >= 2 && position.y >= 2)
            {
                Checker middleChecker = board[position.x - 1, position.y - 1];
                if (middleChecker && middleChecker.CheckerColor != CheckerColor)
                {
                    //check if is is possible to land ater the jump
                    if (board[position.x - 2, position.y - 2] == null)
                    {
                        return true;
                    }
                }
            }

            // can kill by going bottom right
            if (position.x <= 5 && position.y >= 2)
            {
                Checker middleChecker = board[position.x + 1, position.y - 1];
                if (middleChecker && middleChecker.CheckerColor != CheckerColor)
                {
                    //check if is is possible to land ater the jump
                    if (board[position.x + 2, position.y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool ValidMove(Checker[,] board, Vector2Int startPosition, Vector2Int endPosition)
    {
        // if you are moving if another checker
        if (board[endPosition.x, endPosition.y])
        {
            return false;
        }

        int deltaMoveX = Mathf.Abs(startPosition.x - endPosition.x);
        int deltaMoveY = endPosition.y - startPosition.y;

        if (CheckerColor == CheckerColor.White || CheckerType == CheckerType.King)
        {
            if (deltaMoveX == 1 && deltaMoveY == 1)
            {
                return true;
            }
            else if (deltaMoveX == 2 && deltaMoveY == 2)
            {
                Checker middleChecker = board[(startPosition.x + endPosition.x) / 2,
                    (startPosition.y + endPosition.y) / 2];
                if (middleChecker && middleChecker.CheckerColor != CheckerColor)
                {
                    return true;
                }
            }
        }

        if (CheckerColor == CheckerColor.Black || CheckerType == CheckerType.King)
        {
            if (deltaMoveX == 1 && deltaMoveY == -1)
            {
                return true;
            }
            else if (deltaMoveX == 2 && deltaMoveY == -2)
            {
                Checker middleChecker = board[(startPosition.x + endPosition.x) / 2,
                    (startPosition.y + endPosition.y) / 2];
                if (middleChecker && middleChecker.CheckerColor != CheckerColor)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
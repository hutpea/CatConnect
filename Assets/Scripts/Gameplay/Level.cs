using CatConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Board board;
    public LevelTarget levelTarget;
    public int moveCount;

    public void Init()
    {
        InitLevelMusic();
        moveCount = 12;
        board.Init();
    }

    public void InitLevelMusic()
    {
        if (GameController.Instance.useProfile.OnMusic)
        {
            GameController.Instance.musicManager.LerpMusicSourceVolume(.3f, 1f);
        }
    }

    public void DecreaseMoveCount()
    {
        moveCount -= 1;
        if(moveCount < 0)
        {
            moveCount = 0;
        }
        GameplayController.Instance.gameplayUIController.UpdateMoveCountText(moveCount);
    }

    public void RestoreMoveCount(int amount)
    {
        moveCount = amount;
        GameplayController.Instance.gameplayUIController.UpdateMoveCountText(moveCount);
    }

    public void SetupMoveCount(int amount)
    {
        moveCount = amount;
        GameplayController.Instance.gameplayUIController.UpdateMoveCountText(moveCount);
    }
}

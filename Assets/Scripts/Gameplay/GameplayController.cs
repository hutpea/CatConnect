using CatConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : Singleton<GameplayController>
{
    public Level level;
    public GameplayUIController gameplayUIController;
    public ConnectController connectController;

    protected override void OnAwake()
    {
        base.OnAwake();
        level.Init();
        gameplayUIController.Init();

        GameController.Instance.admobAds.DestroyBanner();
        GameController.Instance.admobAds.ShowBanner();
    }
}
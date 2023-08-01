using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CatConnect;

public class LosePopup : BaseBox
{
    private static LosePopup instance;

    public static LosePopup Setup(bool isSaveBox = false, Action actionOpenBoxSave = null, bool firstLose = true)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<LosePopup>(PathPrefabs.LOSE_BOX));
            instance.Init();
        }
        instance.OnShow(firstLose);
        return instance;
    }

    [SerializeField] private RectTransform popUpAwardMove;
    [SerializeField] private Button replayBtn;
    [SerializeField] private Button watchAdBtn;
    [SerializeField] private Text levelText;
    [SerializeField] private Transform targetItemContainer;

    public List<TargetItem> targetItems;
    public Sprite failTarget;

    private void Init()
    {
        replayBtn.onClick.AddListener(OnClickReplayButton);
        watchAdBtn.onClick.AddListener(OnClickWatchAdButton);
    }

    private void OnShow(bool firstLose)
    {
        GameController.Instance.musicManager.PlayLoseSound();
        levelText.text = "Level " + ((UseProfile.CurrentLevel > Context.MAX_LEVEL) ? Context.MAX_LEVEL.ToString() : (UseProfile.CurrentLevel).ToString());
        popUpAwardMove.DOScale(1, .75f).SetDelay(.55f).SetEase(Ease.OutBack);
        SetupTargetItems(GameplayController.Instance.level.levelTarget.targetItems);
        if (!firstLose)
        {
            watchAdBtn.gameObject.SetActive(false);
        }
    }

    private void OnClickReplayButton()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.IsCheatAd)
        {
            Close();
            GameController.Instance.LoadScene("Gameplay");
            return;
        }
        GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
        {
            Close();
            GameController.Instance.LoadScene("Gameplay");
        });

    }

    private void OnClickWatchAdButton()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.IsCheatAd)
        {
            GameplayController.Instance.level.board.isLevelEnd = false;
            GameplayController.Instance.level.board.enableTileInteraction = true;
            GameplayController.Instance.level.board.enablePowerupsInteraction = true;
            GameplayController.Instance.level.RestoreMoveCount(10);
            GameplayController.Instance.level.InitLevelMusic();
            Close();
            return;
        }
        GameController.Instance.admobAds.ShowVideoReward(
           actionReward: () =>
           {
               GameplayController.Instance.level.board.isLevelEnd = false;
               GameplayController.Instance.level.board.enableTileInteraction = true;
               GameplayController.Instance.level.board.enablePowerupsInteraction = true;
               GameplayController.Instance.level.RestoreMoveCount(10);
               GameplayController.Instance.level.InitLevelMusic();
               Close();
           },
           actionNotLoadedVideo: () =>
           {
               GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                (
                watchAdBtn.transform.position,
                "No video at the moment!",
                Color.white,
                isSpawnItemPlayer: true
                );
           },
           actionClose: null,
           ActionWatchVideo.None,
           GameController.Instance.useProfile.CurrentLevelPlay.ToString()
        );
    }

    public void SetupTargetItems(List<TargetItem> list)
    {
        foreach (Transform child in targetItemContainer)
        {
            Destroy(child.gameObject);
        }
        targetItems = new List<TargetItem>();
        foreach (var i in list)
        {
            targetItems.Add(i);
        }
        foreach (var item in targetItems)
        {
            Transform targetItem = Instantiate(GameAssets.Instance.targetItemPrefab, targetItemContainer);
            Image targetItemImage = targetItem.Find("Icon").GetComponent<Image>();
            Text targetItemText = targetItem.Find("Text").GetComponent<Text>();
            RectTransform check = targetItem.Find("Check").GetComponent<RectTransform>();
            switch (item.name)
            {
                case LEVEL_TARGET_TYPE.RedCat:
                    {
                        targetItemImage.sprite = GameAssets.Instance.redCat;
                        break;
                    }
                case LEVEL_TARGET_TYPE.YellowCat:
                    {
                        targetItemImage.sprite = GameAssets.Instance.yellowCat;
                        break;
                    }
                case LEVEL_TARGET_TYPE.WhiteCat:
                    {
                        targetItemImage.sprite = GameAssets.Instance.whiteCat;
                        break;
                    }
                case LEVEL_TARGET_TYPE.GreenCat:
                    {
                        targetItemImage.sprite = GameAssets.Instance.greenCat;
                        break;
                    }
                case LEVEL_TARGET_TYPE.PurpleCat:
                    {
                        targetItemImage.sprite = GameAssets.Instance.purpleCat;
                        break;
                    }
                case LEVEL_TARGET_TYPE.PinkCat:
                    {
                        targetItemImage.sprite = GameAssets.Instance.pinkCat;
                        break;
                    }
                case LEVEL_TARGET_TYPE.Wool:
                    {
                        targetItemImage.sprite = GameAssets.Instance.wool;
                        break;
                    }
                case LEVEL_TARGET_TYPE.Ice:
                    {
                        targetItemImage.sprite = GameAssets.Instance.icePhase1;
                        break;
                    }
                case LEVEL_TARGET_TYPE.Block:
                    {
                        targetItemImage.sprite = GameAssets.Instance.blockSprite;
                        break;
                    }
            }
            targetItem.name = item.name.ToString();
            if(item.currentAmount < item.targetAmount)
            {
                targetItemText.text = item.currentAmount.ToString() + "/" + item.targetAmount.ToString();
                check.transform.GetComponent<Image>().sprite = failTarget;
                check.DOScale(1f, .66f).SetDelay(.66f).SetEase(Ease.OutBack);
            }
            else
            {
                targetItemText.text = item.targetAmount.ToString() + "/" + item.targetAmount.ToString();
                check.DOScale(1f, .66f).SetDelay(.66f).SetEase(Ease.OutBack);
            }
        }
    }
}

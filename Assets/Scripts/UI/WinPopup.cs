using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CatConnect;

public class WinPopup : BaseBox
{
    private static WinPopup instance;

    public static WinPopup Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<WinPopup>(PathPrefabs.WIN_BOX));
            instance.Init();
        }
        instance.OnShow();
        return instance;
    }

    [SerializeField] private Button replayLevelBtn;
    [SerializeField] private Button nextLevelBtn;
    [SerializeField] private Text levelText;
    [SerializeField] private Transform targetItemContainer;

    public List<TargetItem> targetItems;

    private void Init()
    {
        replayLevelBtn.onClick.AddListener(OnClickReplayButton);
        nextLevelBtn.onClick.AddListener(OnClickNextLevelButton);
    }

    private void OnShow()
    {
        GameController.Instance.musicManager.PlayWinSound();
        levelText.text = "Level " + ((UseProfile.CurrentLevel > Context.MAX_LEVEL) ? Context.MAX_LEVEL.ToString() : (UseProfile.CurrentLevel - 1).ToString());
        SetupTargetItems(GameplayController.Instance.level.levelTarget.targetItems);
    }

    private void OnClickReplayButton()
    {
        GameController.Instance.musicManager.PlayClickSound();
        UseProfile.CurrentLevel -= 1;
        Close();
        GameController.Instance.LoadScene("Gameplay");
    }

    private void OnClickNextLevelButton()
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

    public void SetupTargetItems(List<TargetItem> list)
    {
        foreach(Transform child in targetItemContainer)
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
            targetItemText.text = item.targetAmount.ToString() + "/" + item.targetAmount.ToString();
            check.DOScale(1f, .66f).SetDelay(.66f).SetEase(Ease.OutBack);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

public class RatePopup : BaseBox
{

    private static RatePopup instance;
    public static RatePopup Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<RatePopup>(PathPrefabs.RATE_GAME_BOX));
            instance.Init();
        }
        instance.OnShow();
        return instance;
    }
    private const int MIN_API_LEVEL_REVIEW = 21;
    [SerializeField] private ReviewInAppController reviewInAppController;

    [SerializeField] private RectTransform heartSticker;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private List<Sprite> lstSprStar;
    [SerializeField] private List<Image> lstImgStar;
    private int countStar;

    public void Init()
    {
        btnConfirm.onClick.AddListener(RateAction);
        btnClose.onClick.AddListener(CloseAction);
    }
    public void OnShow()
    {
        for (int i = 0; i < lstImgStar.Count; i++)
        {
            int index = i + 1;
            lstImgStar[i].sprite = lstSprStar[0];
        }
        countStar = 0;
        heartSticker.DOScale(1, .75f).SetDelay(.55f).SetEase(Ease.OutBack);
    }

    public void ClickStar(int index)
    {
        countStar = index;
        Debug.Log(countStar);
        for (int i = 0; i < lstImgStar.Count; i++)
        {
            lstImgStar[i].sprite = lstSprStar[0]; //unrate
        }
        for (int i = 0; i < index; i++)
        {
            lstImgStar[i].sprite = lstSprStar[1]; //rate
        }
    }
    public void RateAction()
    {
        Debug.Log("Count star:" + countStar);
        GameController.Instance.musicManager.PlayClickSound();
        if (countStar <= 0)
            return;
        if (countStar >= 4)
        {
            UseProfile.CanShowRate = false;

            try
            {
#if UNITY_ANDROID
                if (RemoteConfigController.GetBoolConfig("on_review_inapp_rate", false))
                {
                    if (Context.GetSDKLevel() >= MIN_API_LEVEL_REVIEW)
                    {
                        reviewInAppController.ShowReview(() =>
                        {
                            Application.OpenURL(Config.OPEN_LINK_RATE);

                        });
                    }
                    else
                    {
                        Application.OpenURL(Config.OPEN_LINK_RATE);
                    }
                }
                else
                {
                    Application.OpenURL(Config.OPEN_LINK_RATE);
                }
#else
            Application.OpenURL(Config.OPEN_LINK_RATE);
            OnCloseWithNeverShow();
#endif
            }
            catch
            {

            }

            CloseAction();
        }
        else
        {
            ShowTextThankRate();
            CloseAction();
        }
    }
    public void CloseAction()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.LoadScene("Gameplay");
        Close();
    }

    public void ShowTextThankRate()
    {
        //StartCoroutine(Helper.StartAction(() =>
        //{
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
    (
       btnConfirm.transform.position,
        "Thank you for the review!",
        Color.white,
        isSpawnItemPlayer: true
    );
        // }, 0.5f));
    }
}
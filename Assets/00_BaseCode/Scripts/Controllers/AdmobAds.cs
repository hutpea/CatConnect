using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;



//  ----------------------------------------------
//  Author:     CuongCT <caothecuong91@gmail.com> 
//  Copyright (c) 2016 OneSoft JSC
// ----------------------------------------------

public class AdmobAds : MonoBehaviour
{

    [SerializeField] private AdsController ironsourceController;


#if UNITY_ANDROID
    private const string InterstitialAdUnitId = "b54ac498d13ef37f";
    private const string RewardedAdUnitId = "6b227ce71cafca73";
    private const string BanerAdUnitId = "a7c39fcfb47b2e38";
#elif UNITY_IOS
    private const string InterstitialAdUnitId = "c8d31e48f08ed31e";
    private const string RewardedAdUnitId = "02932bb866cbb369";
    private const string BanerAdUnitId = "ff665c0a75cadcc4";
#endif

    public float countdownAds;

    public void Init()
    {
        ironsourceController.Init();
        countdownAds = 1000;
    }

    #region Interstitial
    public bool ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other", UnityAction actionIniterClose = null, string level = null)
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            actionIniterClose?.Invoke();
            return false;
        }

#if UNITY_EDITOR
        actionIniterClose?.Invoke();
        return true;
#endif


        if (UseProfile.CurrentLevel > RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_INITSTIALL, 3))
        {
            GameController.Instance.AnalyticsController.LoadInterEligible();

            if (countdownAds > RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_INITSTIALL, 90))
            {
                ironsourceController.ShowInterstitial(actionIniterClose,
            () =>
            {
                UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
                UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
                actionIniterClose?.Invoke();
                GameController.Instance.AnalyticsController.LogInterShow();
                countdownAds = 0;
            });
            }
            else
            {
                if (actionIniterClose != null)
                    actionIniterClose();
            }
        }
        else
        {
            if (actionIniterClose != null)
                actionIniterClose();
        }





        return true;
    }

    #endregion

    #region Video Reward

    /// <summary>
    /// Xử lý Show Video
    /// </summary>
    /// <param name="actionReward">Hành động khi xem xong Video và nhận thưởng </param>
    /// <param name="actionNotLoadedVideo"> Hành động báo lỗi không có video để xem </param>
    /// <param name="actionClose"> Hành động khi đóng video (Đóng lúc đang xem dở hoặc đã xem hết) </param>
    public void ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo, UnityAction actionClose, ActionWatchVideo actionType, string level)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            actionNotLoadedVideo?.Invoke();
            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, false, level);
            return;
        }

        if (!ironsourceController.ShowRewardedVideo(actionNotLoadedVideo, ()=> { 
            actionReward?.Invoke(); 
            countdownAds = 0;
            GameController.Instance.AnalyticsController.LogVideoRewardShow(actionType.ToString());
        }))
        {
            actionNotLoadedVideo?.Invoke();
        }
    }

    #endregion

    #region Banner
    public void ShowBanner()
    {
        ironsourceController.ShowBanner();

    }

    public void DestroyBanner()
    {
        ironsourceController.HideBanner();
    }
    #endregion

    private void Update()
    {
        countdownAds += Time.unscaledDeltaTime;
    }
}

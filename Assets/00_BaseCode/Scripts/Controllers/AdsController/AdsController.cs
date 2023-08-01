using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using com.adjust.sdk;


public class AdsController : MonoBehaviour
{
    private UnityAction onInterstitialClosed;
    private UnityAction onInterstitialShowFailed;

    private UnityAction onRewardedClosed;
    private UnityAction onRewaredShowFailed;

    private bool gotRewarded;
    private bool isShownBanner;
    private bool loadBannerCalled;

    private const string IronsouceAppID = Config.IRONSOURCE_DEV_KEY;

    private IEnumerator reloadInterCoru;

    public void Init()
    {
        IronSource.Agent.init(IronsouceAppID, IronSourceAdUnits.BANNER, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.REWARDED_VIDEO);
        IronSourceEvents.onInterstitialAdClosedEvent += IronSourceEvents_onInterstitialAdClosedEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += IronSourceEvents_onInterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += IronSourceEvents_onInterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdReadyEvent += IronSourceEvents_onInterstitialReady;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += IronSourceEvents_onInterstitialAdShowSucceededEvent;

        IronSourceEvents.onRewardedVideoAdClosedEvent += IronSourceEvents_onRewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += IronSourceEvents_onRewardedVideoAdShowFailedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += IronSourceEvents_onRewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdReadyEvent += IronSourceEvents_onRewardedReady;

        IronSourceEvents.onBannerAdLoadFailedEvent += OnBannerLoadFail;
        IronSourceEvents.onBannerAdLoadedEvent += OnBannerLoaded;
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
        // Load banner & interstitial
        loadInterstitial();
        //#if !ENV_PROD
        IronSource.Agent.validateIntegration();
        //#endif

        IronSourceEvents.onImpressionSuccessEvent += ImpressionSuccessEvent;
#if ENV_LOG
Debug.Log("AdsController initialized");
#endif
    }


    private void IronSourceEvents_onInterstitialAdShowSucceededEvent()
    {
        if (reloadInterCoru != null)
        {
            StopCoroutine(reloadInterCoru);
            reloadInterCoru = null;
        }
    }

    private void IronSourceEvents_onRewardedVideoAdRewardedEvent(IronSourcePlacement obj)
    {
        if (onRewardedClosed != null)
            onRewardedClosed();

        GameController.Instance.AnalyticsController.LogVideoRewardShowDone("");
        // GameController.Instance.dataContains.questDatabase.UpdateQuest(QuestEnum.Watch_X_Video, 1);
    }

    private void IronSourceEvents_onRewardedReady()
    {
        GameController.Instance.AnalyticsController.LogVideoRewardReady();
        // GameController.Instance.dataContains.questDatabase.UpdateQuest(QuestEnum.Watch_X_Video, 1);
    }

    private void IronSourceEvents_onRewardedVideoAdShowFailedEvent(IronSourceError obj)
    {
        onRewaredShowFailed?.Invoke();
    }

    private void IronSourceEvents_onRewardedVideoAdClosedEvent()
    {
        //InvokeRewardedClosedLater();
    }

    private void IronSourceEvents_onInterstitialAdShowFailedEvent(IronSourceError obj)
    {

        onInterstitialShowFailed?.Invoke();
    }

    private void IronSourceEvents_onInterstitialAdLoadFailedEvent(IronSourceError obj)
    {
        if (reloadInterCoru != null)
        {
            StopCoroutine(reloadInterCoru);
            reloadInterCoru = null;
        }

        reloadInterCoru = Helper.StartAction(() => { loadInterstitial(); }, 0.3f);
        StartCoroutine(reloadInterCoru);
    }

    private void IronSourceEvents_onInterstitialReady()
    {
        if (reloadInterCoru != null)
        {
            StopCoroutine(reloadInterCoru);
            reloadInterCoru = null;
        }
        GameController.Instance.AnalyticsController.LogInterReady();
    }

    private void IronSourceEvents_onInterstitialAdClosedEvent()
    {
        loadInterstitial();
        onInterstitialClosed?.Invoke();
    }

    public void loadInterstitial()
    {
        if (reloadInterCoru != null)
        {
            StopCoroutine(reloadInterCoru);
            reloadInterCoru = null;
        }

        if (!IronSource.Agent.isInterstitialReady())
            IronSource.Agent.loadInterstitial();
    }

    public void ShowInterstitial(UnityAction interstitialShowFailedAction, UnityAction interstitialClosedAction)
    {
        this.onInterstitialClosed = interstitialClosedAction;
        this.onInterstitialShowFailed = interstitialShowFailedAction;
        this.onRewaredShowFailed = null;
        this.onRewardedClosed = null;
        if (!IronSource.Agent.isInterstitialReady())
        {
            this.onInterstitialShowFailed?.Invoke();
        }
        else
        {
            IronSource.Agent.showInterstitial();
        }
    }

    public bool IsRewardedVideoAvailable
    {
        get
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }
    }

    public bool ShowRewardedVideo(UnityAction rewardedShowFailedAction, UnityAction rewardedClosedAction)
    {
        this.onRewaredShowFailed = rewardedShowFailedAction;
        this.onRewardedClosed = rewardedClosedAction;
        this.onInterstitialClosed = rewardedClosedAction;
        this.onInterstitialShowFailed = rewardedShowFailedAction;
        this.gotRewarded = false;
        if (!IronSource.Agent.isRewardedVideoAvailable())
        {
            if (IronSource.Agent.isInterstitialReady())
            {
                //GameController.Instance.dataContains.questDatabase.UpdateQuest(QuestEnum.Watch_X_Video, 1);
                IronSource.Agent.showInterstitial();
                GameController.Instance.AnalyticsController.LogVideoRewardEligible();
                return true;
            }
            else
            {
                this.onRewaredShowFailed?.Invoke();
                return false;
            }

        }
        else
        {
            IronSource.Agent.showRewardedVideo();
            GameController.Instance.AnalyticsController.LogVideoRewardEligible();
            return true;
        }
    }

    public void ShowBanner()
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
            return;

        IronSource.Agent.displayBanner();
    }

    public void HideBanner()
    {
        isShownBanner = false;
        //if (IronSource.Agent.IsBa)
        IronSource.Agent.hideBanner();
    }

    private IEnumerator reloadBannerCoru;
    public void OnBannerLoadFail(IronSourceError err)
    {
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }

        reloadBannerCoru = Helper.StartAction(() => { IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM); }, 0.3f);
        StartCoroutine(reloadBannerCoru);
    }    

    public void OnBannerLoaded()
    {
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }

        ShowBanner();
    }    

    //private async void InvokeRewardedClosedLater()
    //{
    //    await Task.Delay(10);
    //    //onRewardedClosed?.Invoke(gotRewarded);  
    //}

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    private void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
    {
        var adRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);
        adRevenue.setRevenue(impressionData.revenue.Value, "USD");
        adRevenue.setAdRevenueNetwork(impressionData.adNetwork);
        adRevenue.setAdRevenueUnit(impressionData.adUnit);
        adRevenue.setAdRevenuePlacement(impressionData.placement);
        Adjust.trackAdRevenue(adRevenue);
    }
}


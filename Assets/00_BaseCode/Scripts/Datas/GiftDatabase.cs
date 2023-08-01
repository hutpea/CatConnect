using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Datas/GiftDatabase", fileName = "GiftDatabase.asset")]
public class GiftDatabase : SerializedScriptableObject
{
    public Dictionary<GiftType, Gift> giftList;

    public bool GetGift(GiftType giftType, out Gift gift)
    {
        return giftList.TryGetValue(giftType, out gift);
    }

    public Sprite GetIconItem(GiftType giftType)
    {
        Gift gift = null;
        //if (IsCharacter(giftType))
        //{
        //    var Char = GameController.Instance.dataContain.dataSkins.GetSkinInfo(giftType);
        //    if (Char != null)
        //        return Char.iconSkin;
        //}
        bool isGetGift = GetGift(giftType, out gift);
        return isGetGift ? gift.getGiftSprite : null;
    }
    public GameObject GetAnimItem(GiftType giftType)
    {
        Gift gift = null;
        bool isGetGift = GetGift(giftType, out gift);
        return isGetGift ? gift.getGiftAnim : null;
    }

    public void Claim(GiftType giftType, int amount, Reason reason = Reason.none)
    {

        switch (giftType)
        {
            case GiftType.Coin:
                // GameController.Instance.useProfile.Coin += amount;
                break;
            case GiftType.Health:
                // GameController.Instance.useProfile.Health += amount;
                break;
            case GiftType.RemoveAds:
                GameController.Instance.useProfile.IsRemoveAds = true;
                GameController.Instance.admobAds.DestroyBanner();
                break;

            case GiftType.Return:
                GameController.Instance.useProfile.CurrentNumReturn += amount;
                break;
            case GiftType.AddBranch:
                GameController.Instance.useProfile.CurrentNumAddBranch += amount;
                break;
            case GiftType.RemoveBomb:
                GameController.Instance.useProfile.CurrentNumRemoveBomb += amount;
                break;
            case GiftType.RemoveCage:
                GameController.Instance.useProfile.CurrentNumRemoveCage += amount;
                break;
            case GiftType.RemoveEgg:
                GameController.Instance.useProfile.CurrentNumRemoveEgg += amount;
                break;
            case GiftType.RemoveSleep:
                GameController.Instance.useProfile.CurrentNumRemoveSleep += amount;
                break;
            case GiftType.RemoveJail:
                GameController.Instance.useProfile.CurrentNumRemoveJail += amount;
                break;

            case GiftType.RandomSkin:
                //List<BirdSkinData> randomBirds = GameController.Instance.dataContain.birdSkinDatabase.GetRandomListIAPBirdSkinData(amount);
                //for(int i = 0; i < randomBirds.Count; i++)
                //{
                //    ClaimSkin(randomBirds[i].birdSkin);
                //}
                break;
            case GiftType.RandomBranch:
                //List<BranchSkinData> randomBranches = GameController.Instance.dataContain.branchSkinDatabase.GetRandomListIAPBranchData(amount);
                //for (int i = 0; i < randomBranches.Count; i++)
                //{
                //    ClaimBranch(randomBranches[i].id);
                //}
                break;
            case GiftType.RandomTheme:
                //List<ThemeSkinData> randomThemes = GameController.Instance.dataContain.themeSkinDatabase.GetRandomListIAPThemeData(amount);
                //for (int i = 0; i < randomThemes.Count; i++)
                //{
                //    ClaimTheme(randomThemes[i].id);
                //}
                break;
        }
    } 
    
    public static bool IsCharacter(GiftType giftType)
    {
        switch (giftType)
        {
            case GiftType.RandomSkin:
                return true;
        }
        return false;
    }
}

public class Gift
{
    [SerializeField] private Sprite giftSprite;
    [SerializeField] private GameObject giftAnim;
    public virtual Sprite getGiftSprite => giftSprite;
    public virtual GameObject getGiftAnim => giftAnim;

}

public enum GiftType
{
    None = 0,
    Coin = 1,
    Health = 2,
    RandomSkin = 3,
    BirdSkin_1_0 = 5,
    BirdSkin_2_0 = 6,
    BirdSkin_3_0 = 7,
    BirdSkin_4_0 = 8,
    BirdSkin_5_0 = 9,
    BirdSkin_6_0 = 10,
    BirdSkin_7_0 = 11,
    BirdSkin_8_0 = 12,
    BirdSkin_9_0 = 13,
    BirdSkin_10_0 = 14,
    BirdSkin_1_1 = 15,
    BirdSkin_2_1 = 16,
    BirdSkin_3_1 = 17,
    BirdSkin_4_1 = 18,
    BirdSkin_5_1 = 19,
    BirdSkin_6_1 = 20,
    BirdSkin_7_1 = 21,
    BirdSkin_8_1 = 22,
    BirdSkin_9_1 = 23,
    BirdSkin_10_1 = 24,

    DefaultBranch = 25,
    AutumnBranch = 26,
    ParkBranch = 27,
    GreeceBranch = 28,
    NederlandBranch = 29,
    MoonlightBranch = 30,
    DesertBranch = 31,
    SpringBranch = 32,
    SummerBranch = 33,
    ChinaBranch = 34,
    WinterBranch = 35,
    DefaultTheme = 36,
    AutumnTheme = 37,
    ParkTheme = 38,
    GreeceTheme = 39,
    NederlandTheme = 40,
    MoonlightTheme = 41,
    DesertTheme = 42,
    SpringTheme = 43,
    SummerTheme = 44,
    ChinaTheme = 45,
    WinterTheme = 46,

    AddBranch = 47,
    Return = 48,

    RemoveBomb = 49,
    RemoveCage = 50,
    RemoveEgg = 51,
    RemoveSleep = 52,
    RemoveJail = 53,

    RandomBranch = 54,
    RandomTheme = 55,

    BirdSkinIronMan = 56,
    BirdSkinCaptain = 57,
    BirdSkinDrStrange = 58,
    BirdSkinLoki = 59,
    BirdSkinBatman = 60,
    BirdSkinAqua = 61,
    BirdSkinIAP_7_1 = 62,
    BirdSkinIAP_8_1 = 63,
    BirdSkinIAP_9_1 = 64,
    BirdSkinIAP_10_1 = 65,

    CyberpunkBranch = 66,
    AquaBranch = 67,
    DrStrangeBranch = 68,
    AsgardBranch = 69,
    BatmanBranch = 70,
    StarkTowerBranch = 71,

    CyberpunkTheme = 72,
    AquaTheme = 73,
    DrStrangeTheme = 74,
    AsgardTheme = 75,
    BatmanTheme = 76,
    StarkTowerTheme = 77,
    RemoveAds = 78,

    
    ConnectBirdMiniGame_Egg = 79,
    EventConnectBirdTheme = 80,
    EventConnectBirdBranch = 81,
        BirdSkin_2_2 = 82,
    pointConnectBirdMNG = 83,
}

public enum Reason
{
    none = 0,
    play_with_item = 1,
    watch_video_claim_item_main_home = 2,
    daily_login = 3,
    lucky_spin = 4,
    unlock_skin_in_special_gift = 5,
    reward_accumulate = 6,
}

[Serializable]
public class RewardRandom
{
    public int id;
    public GiftType typeItem;
    public int amount;
    public int weight;

    public RewardRandom()
    {
    }
    public RewardRandom(GiftType item, int amount, int weight = 0)
    {
        this.typeItem = item;
        this.amount = amount;
        this.weight = weight;
    }

    public GiftRewardShow GetReward()
    {
        GiftRewardShow rew = new GiftRewardShow();
        rew.type = typeItem;
        rew.amount = amount;

        return rew;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;
    public static GameAssets Instance
    {
        get
        {
            if (_i == null) _i = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            return _i;
        }
    }

    #region "Game Resources"

    public Transform cellHolderPrefab;
    public Transform cellHolderVisualPrefab;
    
    public Transform targetItemPrefab;

    public Transform testDot;

    [Header("Dots")]
    public Transform anyColorDot;
    public Transform woolDotPrefab;

    [Header("Spine")]
    public List<CatAnimationData> catAnimationDatas;
    public AnimationReferenceAsset boxBoom;
    public AnimationReferenceAsset iceCrack;
    public AnimationReferenceAsset iceBroken;

    [Header("Tiles")]
    public Transform waterTile;
    public Transform iceTile;
    public Transform blockTile;
    public Transform gapTile;

    [Header("Effects")]
    public Transform circleDotPrefab;
    public Transform burstCircleParticleEffect;
    public List<ParticleSystem> confetties;

    [Header("Tile sprites")]
    public Sprite icePhase1;
    public Sprite icePhase2;
    public Sprite icePhase3;
    public Sprite blockSprite;

    [Header("Dot sprites")]
    public Sprite defaultCircle;
    public Sprite redCat;
    public Sprite yellowCat;
    public Sprite whiteCat;
    public Sprite greenCat;
    public Sprite purpleCat;
    public Sprite pinkCat;
    public Sprite wool;
    public Sprite catPrototypeSprite;

    [Header("Tutorials")]
    public GameObject tutorialLV1Prefab;
    public GameObject tutorialLV2Prefab;
    public GameObject tutorialRandomizePrefab;
    public GameObject tutorialErasePrefab;
    public GameObject tutorialSameColorPrefab;

    [Header("Other sprites")]
    public Sprite addPowerupButton;
    public Sprite containAmountPowerupButton;

    #endregion
}

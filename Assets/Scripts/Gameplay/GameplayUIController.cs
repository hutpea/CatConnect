using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace CatConnect
{
    public class GameplayUIController : MonoBehaviour
    {
        public enum PowerUpPhase
        {
            None,
            Erase,
            SameColor,
            Explode
        }

        [Header("UI Elements")]
        public Text levelText;
        public Button settingBtn;
        public Button replayBtn;
        public Text moveCountText;
        public Transform levelTargetContainer;

        public GameObject belowPanel;
        public bool enablePowerupInteraction;
        public Button randomizePowerUpBtn;
        public Button erasePowerUpBtn;
        public Button sameColorPowerUpBtn;

        public GameObject powerup1Item;
        public GameObject powerup2Item;
        public GameObject powerup3Item;

        public Image containerRandomize;
        public Image containerErase;
        public Image containerSameColor;

        public Text containerRandomizeAmount;
        public Text containerEraseAmount;
        public Text containerSameColorAmount;

        [Header("Manager")]
        public PowerUpPhase powerUpPhase;

        public void Init()
        {
            enablePowerupInteraction = true;
            powerUpPhase = PowerUpPhase.None;

            switch (UseProfile.CurrentLevel)
            {
                case 1:
                case 2:
                    {
                        belowPanel.SetActive(false);
                        powerup1Item.gameObject.SetActive(false);
                        powerup2Item.gameObject.SetActive(false);
                        powerup3Item.gameObject.SetActive(false);
                        break;
                    }
                case 3:
                    {
                        powerup2Item.gameObject.SetActive(false);
                        powerup3Item.gameObject.SetActive(false);
                        break;
                    }
                case 4:
                    {
                        powerup1Item.gameObject.SetActive(false);
                        powerup3Item.gameObject.SetActive(false);
                        break;
                    }
                case 5:
                    {
                        powerup1Item.gameObject.SetActive(false);
                        powerup2Item.gameObject.SetActive(false);
                        break;
                    }
            }

            settingBtn.onClick.AddListener(OnSettingBtnClicked);
            replayBtn.onClick.AddListener(OnReplayBtnClicked);
            randomizePowerUpBtn.onClick.AddListener(OnRandomizeBtnClicked);
            erasePowerUpBtn.onClick.AddListener(OnEraseBtnClicked);
            sameColorPowerUpBtn.onClick.AddListener(OnSameColorBtnClicked);

            UpdateMoveCountText(GameplayController.Instance.level.moveCount);

            UpdatePowerups();

        }

        public void UpdatePowerups()
        {
            if (GameController.Instance.useProfile.CurrentRandomizePowerup <= 0)
            {
                containerRandomize.sprite = GameAssets.Instance.addPowerupButton;
                containerRandomizeAmount.text = "";
            }
            else
            {
                containerRandomize.sprite = GameAssets.Instance.containAmountPowerupButton;
                containerRandomizeAmount.text = GameController.Instance.useProfile.CurrentRandomizePowerup.ToString();
            }
            if (GameController.Instance.useProfile.CurrentErasePowerup <= 0)
            {
                containerErase.sprite = GameAssets.Instance.addPowerupButton;
                containerEraseAmount.text = "";
            }
            else
            {
                containerErase.sprite = GameAssets.Instance.containAmountPowerupButton;
                containerEraseAmount.text = GameController.Instance.useProfile.CurrentErasePowerup.ToString();
            }
            if (GameController.Instance.useProfile.CurrentSameColorPowerup <= 0)
            {
                containerSameColor.sprite = GameAssets.Instance.addPowerupButton;
                containerSameColorAmount.text = "";
            }
            else
            {
                containerSameColor.sprite = GameAssets.Instance.containAmountPowerupButton;
                containerSameColorAmount.text = GameController.Instance.useProfile.CurrentSameColorPowerup.ToString();
            }
        }

        public void SetupLevelText(string levelName)
        {
            levelText.text = String.Format("Level {0}", levelName);
        }

        private bool enablePowerupLost = true;
        public void ToggleEnablePowerupLost(bool value)
        {
            enablePowerupLost = value;
        }

        private void OnSettingBtnClicked()
        {
            if (UseProfile.IsCheatAd)
            {
                SettingPopup.Setup().Show();
                return;
            }
            //GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
           // {
                SettingPopup.Setup().Show();
            //});
        }

        private void OnReplayBtnClicked()
        {
            if (UseProfile.IsCheatAd)
            {
                SceneManager.LoadScene("Gameplay");
                return;
            }
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                SceneManager.LoadScene("Gameplay");
            });
        }

        public void UpdateMoveCountText(int moveCount)
        {
            moveCountText.text = moveCount.ToString();
            moveCountText.GetComponent<RectTransform>().DOScale(1.33f, .2f).SetEase(Ease.InOutFlash).OnComplete(() =>
            {
                moveCountText.GetComponent<RectTransform>().DOScale(1f, .15f).SetEase(Ease.InOutFlash);
            });
        }

        #region Powerups

        private void OnRandomizeBtnClicked()
        {
            if (!enablePowerupInteraction)
            {
                return;
            }
            GameController.Instance.musicManager.PlayClickSound();
            if(GameController.Instance.useProfile.CurrentRandomizePowerup > 0)
            {
                if(enablePowerupLost)
                    GameController.Instance.useProfile.CurrentRandomizePowerup -= 1;
                if (!GameplayController.Instance.level.board.isTutorialStopped)
                {
                    GameplayController.Instance.level.board.StopTutorial();
                }
                UpdatePowerups();
                GameplayController.Instance.level.board.RandomizeBoard();
            }
            else
            {
                if (UseProfile.IsCheatAd)
                {
                    GameController.Instance.useProfile.CurrentRandomizePowerup = 3;
                    UpdatePowerups();
                    return;
                }
                GameController.Instance.admobAds.ShowVideoReward(
                   actionReward: () =>
                   {
                       GameController.Instance.useProfile.CurrentRandomizePowerup = 3;
                       UpdatePowerups();
                   },
                   actionNotLoadedVideo: ()=> 
                   {
                       GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
               (
               randomizePowerUpBtn.transform.position,
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
        }

        private void OnEraseBtnClicked()
        {
            if (!enablePowerupInteraction)
            {
                return;
            }
            GameController.Instance.musicManager.PlayClickSound();
            if (GameController.Instance.useProfile.CurrentErasePowerup > 0)
            {
                enablePowerupInteraction = false;
                if (enablePowerupLost)
                    GameController.Instance.useProfile.CurrentErasePowerup -= 1;
                UpdatePowerups();
                powerUpPhase = PowerUpPhase.Erase;
                GameplayController.Instance.level.board.StartAllDotsCircleEffects();
                GameplayController.Instance.level.board.TutorialDoSecondPhase();
            }
            else
            {
                if (UseProfile.IsCheatAd)
                {
                    GameController.Instance.useProfile.CurrentErasePowerup = 3;
                    UpdatePowerups();
                    return;
                }
                GameController.Instance.admobAds.ShowVideoReward(
                   actionReward: () =>
                   {
                       GameController.Instance.useProfile.CurrentErasePowerup = 3;
                       UpdatePowerups();
                   },
                   actionNotLoadedVideo: ()=> 
                   {
                       GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                (
                erasePowerUpBtn.transform.position,
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
        }

        private void OnSameColorBtnClicked()
        {
            if (!enablePowerupInteraction)
            {
                return;
            }
            GameController.Instance.musicManager.PlayClickSound();
            if (GameController.Instance.useProfile.CurrentSameColorPowerup > 0)
            {
                enablePowerupInteraction = false;
                if (enablePowerupLost)
                    GameController.Instance.useProfile.CurrentSameColorPowerup -= 1;
                UpdatePowerups();
                powerUpPhase = PowerUpPhase.SameColor;
                GameplayController.Instance.level.board.StartAllDotsCircleEffects();
                GameplayController.Instance.level.board.TutorialDoSecondPhase();
            }
            else
            {
                if (UseProfile.IsCheatAd)
                {
                    GameController.Instance.useProfile.CurrentSameColorPowerup = 3;
                    UpdatePowerups();
                    return;
                }
                Debug.Log("Show video");
                GameController.Instance.admobAds.ShowVideoReward(
                   actionReward: () =>
                   {
                       GameController.Instance.useProfile.CurrentSameColorPowerup = 3;
                       UpdatePowerups();
                   },
                   actionNotLoadedVideo: ()=> 
                   {
                       GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                (
                sameColorPowerUpBtn.transform.position,
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
        }

        #endregion
    }
}
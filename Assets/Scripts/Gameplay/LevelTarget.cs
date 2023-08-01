using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CatConnect
{
    public class LevelTarget : MonoBehaviour
    {
        public Material grayscaleMaterial;

        public List<Transform> targetItemsTransform;
        public List<TargetItem> targetItems;
        public List<Text> targetItemTexts;

        private Color targetTextOriginalColor = new Color(67f / 255f, 58f / 255f, 58f / 255f, 1);
        public void Setup(List<TargetItem> list)
        {
            targetItems = new List<TargetItem>();

            foreach (var i in list)
            {
                targetItems.Add(i);
            }
            Debug.Log("targetITems count:" + targetItems.Count);
            SetupLevelTargetAppearance();
        }

        public void SetupLevelTargetAppearance()
        {
            targetItemTexts = new List<Text>();

            foreach (var item in targetItems)
            {
                Debug.LogError(item.name);
                Transform targetItem = Instantiate(GameAssets.Instance.targetItemPrefab, GameplayController.Instance.gameplayUIController.levelTargetContainer);

                Image targetItemImage = targetItem.Find("Icon").GetComponent<Image>();
                Text targetItemText = targetItem.Find("Text").GetComponent<Text>();
                targetItemTexts.Add(targetItemText);

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

                targetItemText.text = "0/" + item.targetAmount.ToString();
            }
        }

        public void UpdateLevelTargetFields(LEVEL_TARGET_TYPE targetType, int amount)
        {
            for(int i = 0; i < targetItems.Count; i++)
            {
                Debug.LogAssertion(targetItems[i].name + " " + targetType);
                if (targetItems[i].name == targetType)
                {
                    int previousAmount = targetItems[i].currentAmount;
                    if (targetItems[i].currentAmount == targetItems[i].targetAmount)
                    {
                        continue;
                    }
                    Transform targetTransform = GameplayController.Instance.gameplayUIController.levelTargetContainer.Find(targetType.ToString());
                    Image bg = targetTransform.Find("Bg").GetComponent<Image>();

                    targetItems[i].currentAmount += amount;
                    if(targetItems[i].currentAmount >= targetItems[i].targetAmount)
                    {
                        GameController.Instance.musicManager.PlayLevelTargetSingleFilled();
                        targetTransform.Find("Check").GetComponent<RectTransform>().DOScale(1f, .66f).SetEase(Ease.OutBack);
                        bg.DOColor(Utility.GetColorAlphaTo1(Utility.GetColorRGB1From255Floats(0, 204, 102)), .15f).OnComplete(() =>
                        {
                            bg.DOColor(Utility.GetColorAlphaTo1(Color.white), .15f);
                        });
                        targetTransform.Find("Icon").GetComponent<Image>().material = grayscaleMaterial;
                        targetItems[i].currentAmount = targetItems[i].targetAmount;
                    }
                    else
                    {
                        GameController.Instance.musicManager.PlayLevelTargetUpdate();
                    }
                    if (targetTransform != null)
                    {
                        int tmp = i;
                        bg.DOColor(Utility.GetColorAlphaTo1(Utility.GetColorRGB1From255Floats(102, 255, 179)), .1f).OnComplete(() =>
                        {
                            bg.DOColor(Utility.GetColorAlphaTo1(Color.white), .1f);
                        });
                        targetItemTexts[tmp].DOColor(Utility.GetColorAlphaTo1(Utility.GetColorRGB1From255Floats(102, 255, 179)), .3f).OnComplete(() =>
                        {
                            targetItemTexts[tmp].DOColor(targetTextOriginalColor, .2f);
                        });
                        targetItemTexts[tmp].GetComponent<RectTransform>().DOScale(1.4f, .3f).SetEase(Ease.InOutFlash).OnComplete(() =>
                        {
                            targetItemTexts[tmp].GetComponent<RectTransform>().DOScale(1f, .2f).SetEase(Ease.Linear);
                        });
                        targetItemTexts[tmp].text = targetItems[tmp].currentAmount + "/" + targetItems[tmp].targetAmount;
                    }

                    break;
                }
            }
        }

        private IEnumerator TargetTextAnimateSimulator(int targetOrder, int previousAmount)
        {
            Debug.Log(targetOrder + " " + previousAmount);
            targetItemTexts[targetOrder].GetComponent<RectTransform>().DOScale(1.2f, .66f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                targetItemTexts[targetOrder].GetComponent<RectTransform>().DOScale(1f, .66f);
            });

            float duration = 2f;
            int loopCount = 5;
            int step = (targetItems[targetOrder].currentAmount - previousAmount) / loopCount;
            for(int i = 1; i <= loopCount; i++)
            {
                if(i == loopCount)
                {
                    targetItemTexts[targetOrder].text = targetItems[targetOrder].currentAmount + "/" + targetItems[targetOrder].targetAmount;
                }
                targetItemTexts[targetOrder].text = (previousAmount + step) + "/" + targetItems[targetOrder].targetAmount;
                previousAmount += step;
                yield return new WaitForSeconds(duration / (float)loopCount);
            }
        }

        public bool CheckLevelTargetFulfilled()
        {
            foreach (var targetItem in targetItems)
            {
                if(targetItem.currentAmount < targetItem.targetAmount)
                {
                    return false;
                }
            }
            GameController.Instance.musicManager.PlayLevelTargetCompletedlyFulfilled();
            return true;
        }
        
        public bool IsLevelHasWoolTarget()
        {
            foreach (var targetItem in targetItems)
            {
                if (targetItem.name == LEVEL_TARGET_TYPE.Wool)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetCurrentWoolTarget()
        {
            foreach (var targetItem in targetItems)
            {
                if (targetItem.name == LEVEL_TARGET_TYPE.Wool)
                {
                    return targetItem.currentAmount;
                }
            }
            return 0;
        }

        public int GetMaximumWoolTarget()
        {
            foreach (var targetItem in targetItems)
            {
                if (targetItem.name == LEVEL_TARGET_TYPE.Wool)
                {
                    return targetItem.targetAmount;
                }
            }
            return 0;
        }
    }

    [System.Serializable]
    public class TargetItem
    {
        public LEVEL_TARGET_TYPE name;
        public int targetAmount;
        public int currentAmount;

        public TargetItem(LEVEL_TARGET_TYPE name, int targetAmount, int currentAmount)
        {
            this.name = name;
            this.targetAmount = targetAmount;
            this.currentAmount = currentAmount;
        }
    }
}
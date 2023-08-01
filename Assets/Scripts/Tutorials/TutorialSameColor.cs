using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialSameColor : MonoBehaviour
{
    public Transform hand;
    public Text guideText;

    Transform randomizeBtnTransform;

    public bool isAnim;

    private void Awake()
    {
        isAnim = false;
        randomizeBtnTransform = GameplayController.Instance.gameplayUIController.sameColorPowerUpBtn.transform;
    }

    public void Setup()
    {
        GameplayController.Instance.gameplayUIController.ToggleEnablePowerupLost(false);
        hand.position = randomizeBtnTransform.position;
        isAnim = true;
        guideText.GetComponent<RectTransform>().DOScale(1, .75f);
        AnimLoop();
    }

    public void AnimLoop()
    {
        if (isAnim)
        {
#if UNITY_EDITOR
            Debug.Log("anim looop");
#endif
            hand.position = randomizeBtnTransform.position;
            hand.DOScale(.45f, .75f).OnComplete(() =>
            {
                hand.DOScale(.33f, .75f).OnComplete(() =>
                {
                    AnimLoop();
                });
            });
        }
    }

    public void DoSecondPhase()
    {
        hand.GetComponent<SpriteRenderer>().enabled = false;
        isAnim = false;

        guideText.text = "Tap a cat to erase all similar cats!";
        //guideText.transform.DOMove(cell12 + Vector3.up, 1f).SetEase(Ease.OutQuad);
    }

    public void DeleteTutorial()
    {
        GameplayController.Instance.gameplayUIController.ToggleEnablePowerupLost(true);
        GameController.Instance.AnalyticsController.LogTutLevelEnd(UseProfile.CurrentLevel);
        isAnim = false;
        hand.DOKill();
        Destroy(this.gameObject);
    }
}
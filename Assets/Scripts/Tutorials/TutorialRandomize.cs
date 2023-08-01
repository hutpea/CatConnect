using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialRandomize : MonoBehaviour
{
    public Transform hand;
    public Text guideText;

    Transform randomizeBtnTransform;

    public bool isAnim;

    private void Awake()
    {
        isAnim = false;
        randomizeBtnTransform = GameplayController.Instance.gameplayUIController.randomizePowerUpBtn.transform;
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

    public void DeleteTutorial()
    {
        GameplayController.Instance.gameplayUIController.ToggleEnablePowerupLost(true);
        GameController.Instance.AnalyticsController.LogTutLevelEnd(UseProfile.CurrentLevel);
        isAnim = false;
        hand.DOKill();
        Destroy(this.gameObject);
    }
}
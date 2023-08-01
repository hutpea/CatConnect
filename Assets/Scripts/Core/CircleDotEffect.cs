using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CircleDotEffect : MonoBehaviour
{
    public Image image;
    private RectTransform rect;
    private bool isAnim = true;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    DOT_TYPE dotType;

    public void StartAnimOnce(DOT_TYPE value)
    {
        StartCoroutine(AnimLoopOnce(value));
    }

    private IEnumerator AnimLoopOnce(DOT_TYPE value)
    {
        StartAnim(value);
        yield return new WaitForSeconds(0.95f);
        image.enabled = false;
        isAnim = false;
    }

    public void StartAnim(DOT_TYPE dotType)
    {
        this.dotType = dotType;
        isAnim = true;
        image.enabled = true;
        image.color = Utility.GetColorFromDotType(dotType);
        AnimLoop();
    }

    public void AnimLoop()
    {
        if (isAnim)
        {
            rect.DOScale(1.25f, 1f).OnUpdate(() =>
            {
                image.DOFade(.3f, 1f);
            }).OnComplete(() =>
            {
                rect.localScale = new Vector3(1, 1, 1);
                image.color = Utility.GetColorFromDotType(dotType);
                rect.DOKill();
                image.DOKill();
                AnimLoop();
            });
        }
    }

    public void Stop()
    {
        image.enabled = false;
        isAnim = false;
    }
}

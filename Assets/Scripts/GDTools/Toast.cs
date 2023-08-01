using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CatConnect
{
    public class Toast : MonoBehaviour
    {
        public static Toast i;

        public Transform toastBox;

        private void Awake()
        {
            if(i != this && i != null)
            {
                Destroy(i.gameObject);
            }
            else
            {
                i = this;
            }
        }

        private void Start()
        {
            toastBox.gameObject.SetActive(false);
        }

        public void SetupFloatingToast(string msg , Color contentColor, Color backgroundColor, float duration)
        {
            StartCoroutine(FloatingToastCoroutine(msg, contentColor, backgroundColor, duration));
        }

        private IEnumerator FloatingToastCoroutine(string msg, Color contentColor, Color backgroundColor, float duration)
        {
            toastBox.gameObject.SetActive(true);
            toastBox.GetComponent<Image>().DOFade(1, .1f);
            toastBox.Find("ContentText").GetComponent<Text>().DOFade(1, .1f);
            toastBox.Find("ContentText").GetComponent<Text>().text = msg;
            toastBox.Find("ContentText").GetComponent<Text>().color = contentColor;
            toastBox.GetComponent<Image>().color = backgroundColor;
            yield return new WaitForSeconds(duration);
            toastBox.GetComponent<Image>().DOFade(0, .5f);
            toastBox.Find("ContentText").GetComponent<Text>().DOFade(0, .5f).OnComplete(() =>
            {
                toastBox.gameObject.SetActive(false);
            });
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TEST : MonoBehaviour
{
    public void CallCoroutine(int i, float f, string s)
    {
        StartCoroutine(TestCoroutine(i, f, s));
    }
    private IEnumerator TestCoroutine(int paramInt, float paramFloat, String paramString)
    {
        yield break;
    }
}

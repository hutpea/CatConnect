using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SettingPopup : BaseBox
{
    private static SettingPopup instance;

    public static SettingPopup Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SettingPopup>(PathPrefabs.SETTING_BOX));
            instance.Init();
        }
        instance.OnShow();
        return instance;
    }

    public Ease toggleEaseType = Ease.InOutQuad;
    public Color toggleOnColor = Utility.GetColorAlphaTo1(Utility.GetColorRGB1From255Floats(0, 139, 150));
    public Color toggleOffColor = Utility.GetColorAlphaTo1(Utility.GetColorRGB1From255Floats(252, 148, 11));

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle hapticToggle;
    [SerializeField] private RectTransform musicToggleHandleRect;
    [SerializeField] private RectTransform sfxToggleHandleRect;
    [SerializeField] private RectTransform hapticToggleHandleRect;

    private Image musicToggleBackground;
    private Image musicOnIcon;
    private Image musicOffIcon;
    private RectTransform musicOnPos;
    private RectTransform musicOffPos;

    private Image sfxToggleBackground;
    private Image sfxOnIcon;
    private Image sfxOffIcon;
    private RectTransform sfxOnPos;
    private RectTransform sfxOffPos;

    private void Init()
    {
        confirmButton.onClick.AddListener(OnReplayBtnClicked);
        closeButton.onClick.AddListener(OnClickConfirmButton);
        musicToggle.onValueChanged.AddListener(OnMusicToggleValueChanged);
        sfxToggle.onValueChanged.AddListener(OnSfxToggleValueChanged);

        musicToggleBackground = musicToggle.transform.Find("Background").GetComponent<Image>();
        musicOnPos = musicToggle.transform.Find("Background").Find("onPos").GetComponent<RectTransform>();
        musicOffPos = musicToggle.transform.Find("Background").Find("offPos").GetComponent<RectTransform>();
        musicOnIcon = musicOffPos.transform.Find("onIcon").GetComponent<Image>();
        musicOffIcon = musicOnPos.transform.Find("offIcon").GetComponent<Image>();

        sfxToggleBackground = sfxToggle.transform.Find("Background").GetComponent<Image>();
        sfxOnPos = sfxToggle.transform.Find("Background").Find("onPos").GetComponent<RectTransform>();
        sfxOffPos = sfxToggle.transform.Find("Background").Find("offPos").GetComponent<RectTransform>();
        sfxOnIcon = sfxOffPos.transform.Find("onIcon").GetComponent<Image>();
        sfxOffIcon = sfxOnPos.transform.Find("offIcon").GetComponent<Image>();
    }

    private void OnShow()
    {
        Debug.Log("OnShow SettingPopup");
        if (GameController.Instance.useProfile.OnMusic)
        {
            Debug.Log("music:on");
            musicToggleHandleRect.DOAnchorPos(musicOnPos.anchoredPosition, .1f).SetEase(toggleEaseType);
            musicToggleBackground.DOColor(toggleOnColor, .1f).SetEase(toggleEaseType);
            musicOnIcon.enabled = true;
            musicOffIcon.enabled = false;
        }
        else
        {
            Debug.Log("music:off");
            musicToggleHandleRect.DOAnchorPos(musicOffPos.anchoredPosition, .1f).SetEase(toggleEaseType);
            musicToggleBackground.DOColor(toggleOffColor, .1f).SetEase(toggleEaseType);
            musicOnIcon.enabled = false;
            musicOffIcon.enabled = true;
        }

        if (GameController.Instance.useProfile.OnSound)
        {
            Debug.Log("sfx:on");
            sfxToggleHandleRect.DOAnchorPos(sfxOnPos.anchoredPosition, .25f).SetEase(toggleEaseType);
            sfxToggleBackground.DOColor(toggleOnColor, .25f).SetEase(toggleEaseType);
            sfxOnIcon.enabled = true;
            sfxOffIcon.enabled = false;
        }
        else
        {
            Debug.Log("sfx:off");
            sfxToggleHandleRect.DOAnchorPos(sfxOffPos.anchoredPosition, .25f).SetEase(toggleEaseType);
            sfxToggleBackground.DOColor(toggleOffColor, .25f).SetEase(toggleEaseType);
            sfxOnIcon.enabled = false;
            sfxOffIcon.enabled = true;
        }
    }

    private void OnMusicToggleValueChanged(bool on)
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (!on)
        {
            musicToggleHandleRect.DOAnchorPos(musicOnPos.anchoredPosition, .25f).SetEase(toggleEaseType);
            musicToggleBackground.DOColor(toggleOnColor, .25f).SetEase(toggleEaseType);
            musicOnIcon.enabled = true;
            musicOffIcon.enabled = false;
            GameController.Instance.useProfile.OnMusic = true;
        }
        else
        {
            musicToggleHandleRect.DOAnchorPos(musicOffPos.anchoredPosition, .25f).SetEase(toggleEaseType);
            musicToggleBackground.DOColor(toggleOffColor, .25f).SetEase(toggleEaseType);
            musicOnIcon.enabled = false;
            musicOffIcon.enabled = true;
            GameController.Instance.useProfile.OnMusic = false;
        }
    }

    private void OnSfxToggleValueChanged(bool on)
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (!on)
        {
            sfxToggleHandleRect.DOAnchorPos(sfxOnPos.anchoredPosition, .25f).SetEase(toggleEaseType);
            sfxToggleBackground.DOColor(toggleOnColor, .25f).SetEase(toggleEaseType);
            sfxOnIcon.enabled = true;
            sfxOffIcon.enabled = false;
            GameController.Instance.useProfile.OnSound = true;
        }
        else
        {
            sfxToggleHandleRect.DOAnchorPos(sfxOffPos.anchoredPosition, .25f).SetEase(toggleEaseType);
            sfxToggleBackground.DOColor(toggleOffColor, .25f).SetEase(toggleEaseType);
            sfxOnIcon.enabled = false;
            sfxOffIcon.enabled = true;
            GameController.Instance.useProfile.OnSound = false;
        }
    }

    private void OnClickConfirmButton()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Close();
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

}

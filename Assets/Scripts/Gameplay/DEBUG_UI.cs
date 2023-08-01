using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DEBUG_UI : MonoBehaviour
{
    public Button debug_1_btn;
    public Button debug_2_btn;
    public Button debug_cheat_ads_btn;
    public InputField levelIF;
    public Button debug_play_btn;

    public Color adsCheatOff;
    public Color adsCheatOn;

    private void Start()
    {
        debug_1_btn.onClick.AddListener(OnDebug1BtnClicked);
        debug_2_btn.onClick.AddListener(OnDebug2BtnClicked);
        debug_cheat_ads_btn.onClick.AddListener(OnDebugCheatAdsBtnClicked);
        debug_play_btn.onClick.AddListener(OnDebugPlayBtnClicked);

        if (UseProfile.IsCheatAd)
        {
            debug_cheat_ads_btn.GetComponent<Image>().color = Utility.GetColorAlphaTo1(adsCheatOn);
            debug_cheat_ads_btn.transform.Find("Text").GetComponent<Text>().text = "AD:ON";
        }
        else
        {
            debug_cheat_ads_btn.GetComponent<Image>().color = Utility.GetColorAlphaTo1(adsCheatOff);
            debug_cheat_ads_btn.transform.Find("Text").GetComponent<Text>().text = "AD:OFF";
        }
    }

    private void OnDebug1BtnClicked()
    {
        string s = "";
        
        for (int i = 1; i <= GameplayController.Instance.level.board.Height; i++)
        {
            for (int j = 1; j <= GameplayController.Instance.level.board.Width; j++)
            {
                if (GameplayController.Instance.level.board.gameDots[i, j])
                {
                    s += //GameplayController.Instance.level.board.gameDots[i, j].name
                        GameplayController.Instance.level.board.gameDots[i, j].dotType.ToString() 
                        + GameplayController.Instance.level.board.gameDots[i, j].Row + "," 
                        + GameplayController.Instance.level.board.gameDots[i, j].Column;
                }
                else
                {
                    s += "null";
                }
                s += " ";
            }
            s += "\n";
        }
        Debug.LogError(s);
    }

    private void OnDebug2BtnClicked()
    {

    }

    private void OnDebugCheatAdsBtnClicked()
    {
        UseProfile.IsCheatAd = !UseProfile.IsCheatAd;
        if (UseProfile.IsCheatAd)
        {
            debug_cheat_ads_btn.GetComponent<Image>().color = Utility.GetColorAlphaTo1(adsCheatOn);
            debug_cheat_ads_btn.transform.Find("Text").GetComponent<Text>().text = "AD:ON";
        }
        else
        {
            debug_cheat_ads_btn.GetComponent<Image>().color = Utility.GetColorAlphaTo1(adsCheatOff);
            debug_cheat_ads_btn.transform.Find("Text").GetComponent<Text>().text = "AD:OFF";
        }
    }

    private void OnDebugPlayBtnClicked()
    {
        try
        {
            int levelNum = 1;
            Int32.TryParse(levelIF.text, out levelNum);
            UseProfile.CurrentLevel = levelNum;
            GameController.Instance.LoadScene("Gameplay");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}

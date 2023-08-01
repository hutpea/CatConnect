using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public static class Utility
{
    public static Color GetColorFromDotType(DOT_TYPE dotType)
    {
        Color color = new Color();
        switch (dotType)
        {
            case DOT_TYPE.RedCat:
                {
                    color = GetColorRGB1From255Floats(233, 61, 82);
                    break;
                }
            case DOT_TYPE.YellowCat:
                {
                    color = GetColorRGB1From255Floats(255, 140, 0);
                    break;
                }
            case DOT_TYPE.WhiteCat:
                {
                    color = GetColorRGB1From255Floats(70, 70, 90);
                    break;
                }
            case DOT_TYPE.GreenCat:
                {
                    color = GetColorRGB1From255Floats(73, 209, 112);
                    break;
                }
            case DOT_TYPE.PurpleCat:
                {
                    color = GetColorRGB1From255Floats(185, 96, 230);
                    break;
                }
            case DOT_TYPE.PinkCat:
                {
                    color = GetColorRGB1From255Floats(235, 120, 217);
                    break;
                }
            case DOT_TYPE.Wool:
                {
                    color = GetColorRGB1From255Floats(181, 156, 127);
                    break;
                }
            default: color = Color.white; break;
        }
        return GetColorAlphaTo1(color);
    }

    public static Color GetColorAlphaTo1(Color color)
    {
        var temp = color;
        temp.a = 1;
        return temp;
    }

    public static Color GetColorAlphaToX(Color color, float x)
    {
        var temp = color;
        temp.a = x;
        return temp;
    }

    public static COLOR_TYPE FromDotTypeToColorType(DOT_TYPE dotType)
    {
        switch(dotType)
        {
            case DOT_TYPE.RedCat:
                {
                    return COLOR_TYPE.RedColor;
                }
            case DOT_TYPE.YellowCat:
                {
                    return COLOR_TYPE.YellowColor;
                }
            case DOT_TYPE.WhiteCat:
                {
                    return COLOR_TYPE.WhiteColor;
                }
            case DOT_TYPE.GreenCat:
                {
                    return COLOR_TYPE.GreenColor;
                }
            case DOT_TYPE.PurpleCat:
                {
                    return COLOR_TYPE.PurpleColor;
                }
            case DOT_TYPE.PinkCat:
                {
                    return COLOR_TYPE.PinkColor;
                }
            default:
                {
                    Debug.Log("You're trying to get colorType of a non-color dot");
                    return COLOR_TYPE.None;
                }
        }
    }

    public static Color GetColorRGB1From255Floats(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static Color GetRandomColor()
    {
        return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
    }

    public static LEVEL_TARGET_TYPE FromTileTypeToLevelTargetType(TILE_TYPE tileType)
    {
        LEVEL_TARGET_TYPE levelTargetType = LEVEL_TARGET_TYPE.Block;
        return levelTargetType;
    }

    public static LEVEL_TARGET_TYPE FromDotTypeToLevelTargetType(DOT_TYPE dotType)
    {
        LEVEL_TARGET_TYPE levelTargetType = LEVEL_TARGET_TYPE.RedCat;
        switch (dotType)
        {
            case DOT_TYPE.RedCat:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.RedCat;
                    break;
                }
            case DOT_TYPE.YellowCat:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.YellowCat;
                    break;
                }
            case DOT_TYPE.WhiteCat:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.WhiteCat;
                    break;
                }
            case DOT_TYPE.GreenCat:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.GreenCat;
                    break;
                }
            case DOT_TYPE.PurpleCat:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.PurpleCat;
                    break;
                }
            case DOT_TYPE.PinkCat:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.PinkCat;
                    break;
                }
            case DOT_TYPE.Wool:
                {
                    levelTargetType = LEVEL_TARGET_TYPE.Wool;
                    break;
                }
        }
        return levelTargetType;
    }

    public static LEVEL_TARGET_TYPE FromStringToLevelTargetType(string str)
    {
        switch (str)
        {
            case "Red Cat":
                {
                    return LEVEL_TARGET_TYPE.RedCat;
                }
            case "Yellow Cat":
                {
                    return LEVEL_TARGET_TYPE.YellowCat;
                }
            case "White Cat":
                {
                    return LEVEL_TARGET_TYPE.WhiteCat;
                }
            case "Green Cat":
                {
                    return LEVEL_TARGET_TYPE.GreenCat;
                }
            case "Purple Cat":
                {
                    return LEVEL_TARGET_TYPE.PurpleCat;
                }
            case "Pink Cat":
                {
                    return LEVEL_TARGET_TYPE.PinkCat;
                }
            case "Wool":
                {
                    return LEVEL_TARGET_TYPE.Wool;
                }
            case "Ice":
                {
                    return LEVEL_TARGET_TYPE.Ice;
                }
            case "Block":
                {
                    return LEVEL_TARGET_TYPE.Block;
                }
            default: return LEVEL_TARGET_TYPE.RedCat;
        }
    }

    public static DOT_TYPE FromIntToDotType(int intValue)
    {
        switch (intValue)
        {
            case 1:
                {
                    return DOT_TYPE.RedCat;
                }
            case 2:
                {
                    return DOT_TYPE.YellowCat;
                }
            case 3:
                {
                    return DOT_TYPE.WhiteCat;
                }
            case 4:
                {
                    return DOT_TYPE.GreenCat;
                }
            case 5:
                {
                    return DOT_TYPE.PurpleCat;
                }
            case 6:
                {
                    return DOT_TYPE.PinkCat;
                }
            default: return DOT_TYPE.None;
        }
    }

    public static CatAnimationData GetCatAnimationData(DOT_TYPE dotType)
    {
        switch (dotType)
        {
            case DOT_TYPE.RedCat:
                {
                    return GameAssets.Instance.catAnimationDatas[0];
                }
            case DOT_TYPE.YellowCat:
                {
                    return GameAssets.Instance.catAnimationDatas[1];
                }
            case DOT_TYPE.WhiteCat:
                {
                    return GameAssets.Instance.catAnimationDatas[2];
                }
            case DOT_TYPE.GreenCat:
                {
                    return GameAssets.Instance.catAnimationDatas[3];
                }
            case DOT_TYPE.PurpleCat:
                {
                    return GameAssets.Instance.catAnimationDatas[4];
                }
            case DOT_TYPE.PinkCat:
                {
                    return GameAssets.Instance.catAnimationDatas[5];
                }
        }
        return GameAssets.Instance.catAnimationDatas[0];
    }

    public static bool IsThisDotTypeAbleToConnect(DOT_TYPE dotType)
    {
        switch (dotType)
        {
            case DOT_TYPE.RedCat:
            case DOT_TYPE.YellowCat:
            case DOT_TYPE.WhiteCat:
            case DOT_TYPE.GreenCat:
            case DOT_TYPE.PurpleCat:
            case DOT_TYPE.PinkCat: return true;
            default: return false;
        }
    }
    public static bool IsThisDotTypeCatType(DOT_TYPE dotType)
    {
        switch (dotType)
        {
            case DOT_TYPE.RedCat:
            case DOT_TYPE.YellowCat:
            case DOT_TYPE.WhiteCat:
            case DOT_TYPE.GreenCat:
            case DOT_TYPE.PurpleCat:
            case DOT_TYPE.PinkCat: return true;
            default: return false;
        }
    }


    public static bool IsThisDotTypeColor(DOT_TYPE dotType)
    {
        switch (dotType)
        {
            case DOT_TYPE.RedCat:
            case DOT_TYPE.YellowCat:
            case DOT_TYPE.WhiteCat:
            case DOT_TYPE.GreenCat:
            case DOT_TYPE.PurpleCat:
            case DOT_TYPE.PinkCat: return true;
            default: return false;
        }
    }

    public static string AListToString<T>(List<T> list)
    {
        string s = "";
        foreach (var item in list)
        {
            s += item.ToString() + "=>";
        }
        return s;
    }

    public static string ListChildOfTransform(Transform t)
    {
        string s = "";
        foreach(var child in t)
        {
            s += child.ToString();
        }
        return s;
    }
    public static void Shuffle<T>(this IList<T> list, int seed)
    {
        System.Random rng = new System.Random(System.DateTime.Now.Second + seed);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }

    public static string RemoveRedundantSpaces(string text)
    {
        return Regex.Replace(text, @"\s+", " ").Trim();
    }
}
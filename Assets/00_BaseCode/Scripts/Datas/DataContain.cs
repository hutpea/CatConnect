using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatConnect;
using System.IO;

public class DataContain : MonoBehaviour
{
    public GiftDatabase giftDatabase;
    public List<LevelData> levelDataList = new List<LevelData>();

    public void Initialize()
    {
        LoadAllBoardDataToList();
    }

    public void LoadAllBoardDataToList()
    {
        /*if (Directory.Exists(Application.dataPath))
        {
            string levelsFolder = Application.dataPath + "/Resources/Levels";
            DirectoryInfo d = new DirectoryInfo(levelsFolder);
            foreach (var file in d.GetFiles("*.json"))
            {
                Debug.Log(file);
            }
        }*/
        /*for (int i = 1; i <= Context.MAX_LEVEL; i++)
        {
            //Debug.Log(i);
            var jsonTextFile = Resources.Load<TextAsset>("Levels/Level" + i);
            LevelData levelData = new LevelData();
            Debug.Log(jsonTextFile);
            levelData = JsonUtility.FromJson<LevelData>(jsonTextFile.text);
            levelDataList.Add(levelData);
        }*/
    }
}


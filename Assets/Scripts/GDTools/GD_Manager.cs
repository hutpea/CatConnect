using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft;

namespace CatConnect
{
    public class GD_Manager : MonoBehaviour
    {
        public static GD_Manager Instance;

        [Header("Prefabs")]
        public Transform cellPrefab;
        public Transform targetItemPrefab;

        [Header("UI")]
        public Transform boardContainer;
        public GridLayoutGroup gridLayout;
        public Transform targetContainer;

        public InputField levelNameIF;
        public InputField movesIF;
        public InputField heightIF;
        public InputField widthIF;
        public List<Button> catButtons;
        public List<Button> optionSpecialButtons;
        public List<Dropdown> targetDropdowns;
        public List<InputField> targetInputField;
        public Button addTargetItemButton;
        public Button removeAllTargetItemsButton;
        public Button createLevelButton;

        [Header("Manager")]
        public int boardHeight;
        public int boardWidth;
        public Transform[,] cellMatrix;
        public List<int> chooseCats;

        public GDItem chosenSpecialItem;

        private string savePath = "/Resources/Levels/";

        private void Awake()
        {
            if(Instance != this && Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            else
            {
                Instance = this;
            }

            heightIF.onValueChanged.AddListener(delegate { OnBoardSizeChanged(); });
            widthIF.onValueChanged.AddListener(delegate { OnBoardSizeChanged(); });
            for (int i = 0; i < catButtons.Count; i++)
            {
                int temp = i;
                catButtons[temp].onClick.AddListener(() => { OnClickCatItem(temp + 1); ReloadDropdownItem(); });
            }
            addTargetItemButton.onClick.AddListener(delegate { AddTargetItem(); });
            removeAllTargetItemsButton.onClick.AddListener(delegate { RemoveSelectedTargetItems(); });
            for (int i = 0; i < targetDropdowns.Count; i++)
            {
                int temp = i;
                List<string> m_DropOptions = new List<string> { "Ice", "Wool", "Block" };
                targetDropdowns[temp].AddOptions(m_DropOptions);
            }
            createLevelButton.onClick.AddListener(delegate { CreateLevelFile(); });

            chooseCats = new List<int>();
        }

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        public void OnClickCatItem(int id)
        {
            if (!chooseCats.Contains(id))
            {
                chooseCats.Add(id);
                catButtons[id - 1].transform.GetComponent<Image>().color = Color.green;
            }
            else
            {
                chooseCats.Remove(id);
                catButtons[id - 1].transform.GetComponent<Image>().color = Color.white;
            }
            string s = "";
            foreach(var item in chooseCats)
            {
                s += item + " ";
            }
            Debug.Log("Choose cats: " + s);
        }

        public void OnClickOptionSpecialItemButton(int id)
        {
            GDItem option = GDItem.None;
            switch (id)
            {
                case 0:
                    {
                        option = GDItem.Ice;
                        break;
                    }
                case 1:
                    {
                        option = GDItem.Wool;
                        break;
                    }
                case 2:
                    {
                        option = GDItem.Block;
                        break;
                    }
                case 3:
                    {
                        option = GDItem.Gap;
                        break;
                    }
                case 4:
                    {
                        option = GDItem.RESET;
                        break;
                    }
            }
            chosenSpecialItem = option;
            foreach(var i in optionSpecialButtons)
            {
                i.transform.GetComponent<Image>().color = Color.white;
            }
            
            optionSpecialButtons[id].transform.GetComponent<Image>().color = Color.green;
        }

        public void OnBoardSizeChanged()
        {
            Debug.Log("size change if1: " + heightIF.text + ", if2: " + widthIF.text);
            if(heightIF.text != "")
                boardHeight = System.Int32.Parse(heightIF.text);
            if (widthIF.text != "")
                boardWidth = System.Int32.Parse(widthIF.text);
            if(boardHeight <= 0 || boardWidth <= 0)
            {
                return;
            }

            gridLayout.constraintCount = boardWidth;
            cellMatrix = new Transform[boardHeight + 1, boardWidth + 1];

            Color color = Utility.GetColorAlphaTo1(Utility.GetRandomColor());

            foreach(Transform child in boardContainer)
            {
                Destroy(child.gameObject);
            }

            for(int i = 1; i <= boardHeight; i++)
            {
                for (int j = 1; j <= boardWidth; j++)
                {
                    var cell = Instantiate(cellPrefab, boardContainer);
                    Debug.Log(new Vector2(i, j));
                    cellMatrix[i, j] = cell;
                    cellMatrix[i, j].GetComponent<Image>().color = color;
                    cellMatrix[i, j].GetComponent<GD_Cell>().x = i;
                    cellMatrix[i, j].GetComponent<GD_Cell>().y = j;
                }
            }
        }

        public void AddTargetItem()
        {
            var item = Instantiate(targetItemPrefab, targetContainer);
            targetDropdowns.Add(item.Find("Dropdown").GetComponent<Dropdown>());
            targetInputField.Add(item.Find("amountIF").GetComponent<InputField>());
            ReloadDropdownItem();
        }

        public void RemoveSelectedTargetItems()
        {
            Debug.Log("remove selected targets");
            for(int i = 0; i < targetDropdowns.Count; i++)
            {
                if (targetDropdowns[i].transform.parent.GetComponent<GD_TargetItem>().isSelected)
                {
                    targetDropdowns.RemoveAt(i);
                    targetInputField.RemoveAt(i);
                }
            }
            targetDropdowns.RemoveAll(t => t.transform.parent.GetComponent<GD_TargetItem>().isSelected);
            targetInputField.RemoveAll(t => t.transform.parent.GetComponent<GD_TargetItem>().isSelected);

            foreach(Transform child in targetContainer)
            {
                if (child.GetComponent<GD_TargetItem>().isSelected)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void RemoveAllTargetItems()
        {
            targetDropdowns.Clear();
            targetInputField.Clear();
            foreach (Transform child in targetContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void ReloadDropdownItem()
        {
            for (int i = 0; i < targetDropdowns.Count; i++)
            {
                int temp = i;
                targetDropdowns[temp].options.Clear();
                List<string> specialItems = new List<string> { "Ice", "Wool", "Block" };
                targetDropdowns[temp].AddOptions(specialItems);
                List<string> m_DropOptions = GetTargetStringOptionsFromChosenCats();
                targetDropdowns[temp].AddOptions(m_DropOptions);
            }
        }

        public void CreateLevelFile()
        {
            if(levelNameIF.text == "")
            {
                Toast.i.SetupFloatingToast("Chưa đặt tên level !", Color.white, Color.red, 1.5f);
                return;
            }
            if (movesIF.text == "")
            {
                Toast.i.SetupFloatingToast("Chưa có số lượng move !", Color.white, Color.red, 1.5f);
                return;
            }
            if (heightIF.text == "" || widthIF.text == "")
            {
                Toast.i.SetupFloatingToast("Chưa có board (sai size) !", Color.white, Color.red, 1.5f);
                return;
            }
            if (chooseCats.Count <= 0)
            {
                Toast.i.SetupFloatingToast("Chưa chọn loại mèo !", Color.white, Color.red, 1.5f);
                return;
            }


            LevelData levelData = new LevelData();

            levelData.levelName = levelNameIF.text;
            levelData.moves = System.Int32.Parse(movesIF.text);
            levelData.boardHeight = boardHeight;
            levelData.boardWidth = boardWidth;
            levelData.chooseCats = chooseCats;

            List<LevelTargetJson> levelTargets = new List<LevelTargetJson>();

            for(int i = 0; i < targetDropdowns.Count; i++)
            {
                if (targetInputField[i].text != "")
                {
                    LevelTargetJson levelTargetJson = new LevelTargetJson();
                    int menuIndex = targetDropdowns[i].GetComponent<Dropdown>().value;
                    List<Dropdown.OptionData> menuOptions = targetDropdowns[i].GetComponent<Dropdown>().options;
                    string value = menuOptions[menuIndex].text;
                    levelTargetJson.type = value;
                    levelTargetJson.amount = System.Int32.Parse(targetInputField[i].text);
                    levelTargets.Add(levelTargetJson);
                }
                else
                {
                    Toast.i.SetupFloatingToast("Chưa điền số lượng của target !", Color.white, Color.red, 1.5f);
                    break;
                }
            }

            if(levelTargets.Count <= 0)
            {
                Toast.i.SetupFloatingToast("Chưa có target nào !", Color.white, Color.red, 1.5f);
                return;
            }

            levelData.levelTargets = levelTargets;

            string layout = "";
            for (int i = 1; i <= boardHeight; i++)
            {
                for (int j = 1; j <= boardWidth; j++)
                {
                    layout += cellMatrix[i, j].GetComponent<GD_Cell>().GetCellString();
                    if(j != boardWidth)
                    {
                        layout += "~";
                    }
                }
                if(i != boardHeight)
                {
                    layout += "#";
                }
            }

            levelData.boardLayout = layout;

            string json = JsonUtility.ToJson(levelData);

            File.WriteAllText(Application.persistentDataPath + "/Level" + levelData.levelName + ".json", json);
            File.WriteAllText(Application.dataPath + savePath + "/Level" + levelData.levelName + ".json", json);

            Debug.Log(json);
        }
        private List<string> GetTargetStringOptionsFromChosenCats(){
            List<string> list = new List<string>();
            foreach(var catID in chooseCats)
            {
                switch (catID)
                {
                    case 1: { list.Add("Red Cat"); break; }
                    case 2: { list.Add("Yellow Cat"); break; }
                    case 3: { list.Add("White Cat"); break; }
                    case 4: { list.Add("Green Cat"); break; }
                    case 5: { list.Add("Purple Cat"); break; }
                    case 6: { list.Add("Pink Cat"); break; }
                }
            }
            return list;
        }
    }

    [System.Serializable]
    public enum GDItem
    {
        None,
        Ice,
        Wool,
        Block,
        Gap,
        RESET
    }
}

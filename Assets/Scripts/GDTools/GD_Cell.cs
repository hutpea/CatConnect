using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatConnect
{
    public class GD_Cell : MonoBehaviour
    {
        public int x;
        public int y;

        private Transform tile;
        private Transform dot;

        //ice_dot_null

        private string tileString;
        private string dotString;
        private string machenicString;

        private void Awake()
        {
            tile = transform.Find("Tile");
            dot = transform.Find("Dot");
        }

        private void Start()
        {
            tileString = "";
            dotString = "any";
            machenicString = "";

            GetComponent<Button>().onClick.AddListener(delegate { OnClick(); });

            dot.GetComponent<Image>().sprite = GameAssets.Instance.catPrototypeSprite;
        }

        public void OnClick()
        {
            DrawItemToCell(GD_Manager.Instance.chosenSpecialItem);
        }

        public void DrawItemToCell(GDItem item)
        {
            Debug.Log("Draw " + item);
            tile.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 1);
            dot.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 1);
            switch (item)
            {
                case GDItem.RESET:
                    {
                        ResetCell();
                        break;
                    }
                case GDItem.Ice:
                    {
                        tile.GetComponent<Image>().sprite = GameAssets.Instance.icePhase1;
                        tileString = "ice";
                        break;
                    }
                case GDItem.Wool:
                    {
                        dot.GetComponent<Image>().sprite = GameAssets.Instance.wool;
                        dotString = "wool";
                        break;
                    }
                case GDItem.Block:
                    {
                        tile.GetComponent<Image>().sprite = GameAssets.Instance.blockSprite;
                        tileString = "block";
                        break;
                    }
                case GDItem.Gap:
                    {
                        tile.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 0);
                        dot.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 0);
                        dot.GetComponent<Image>().sprite = null;
                        tileString = "gap";
                        break;
                    }
            }
            if(dotString == "any")
            {
                if(tileString != "gap")
                {
                    dot.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 1);
                }
                else
                {
                    dot.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 0);
                }
                dot.GetComponent<Image>().sprite = GameAssets.Instance.catPrototypeSprite;
            }
        }

        public void ResetCell()
        {
            tileString = "";
            dotString = "any";
            tile.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 1);
            tile.GetComponent<Image>().sprite = null;
            dot.GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.white, 1);
            dot.GetComponent<Image>().sprite = GameAssets.Instance.catPrototypeSprite;
        }

        public string GetCellString()
        {
            return tileString + "_" + dotString + "_" + machenicString;
        }
    }
}

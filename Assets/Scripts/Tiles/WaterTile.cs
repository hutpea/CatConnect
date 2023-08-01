using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatConnect
{
    public class WaterTile : Tile
    {
        public bool isFilled = false;

        public override void Awake()
        {
            base.Awake();
            type = TILE_TYPE.Water;
        }

        public void FillWater()
        {
            GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.blue, .3f);
        }

        public void UnfillWater()
        {
            GetComponent<Image>().color = Utility.GetColorAlphaToX(Color.blue, 0f);
        }
    }
}
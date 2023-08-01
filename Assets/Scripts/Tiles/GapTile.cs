using System.Collections;
using UnityEngine;

namespace CatConnect
{
    public class GapTile : Tile
    {
        public override void Awake()
        {
            base.Awake();
            type = TILE_TYPE.Gap;
            transform.name = "Gap";
        }
    }
}
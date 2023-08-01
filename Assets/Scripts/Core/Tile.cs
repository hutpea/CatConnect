using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatConnect
{
    public class Tile : BoardOccupant
    {
        public TILE_TYPE type;

        public virtual void Awake()
        {

        }

        public virtual void OnHit(BoardOccupant hitSource, bool forceDestroy = false)
        {

        }

        public virtual void OnMatch(bool forceDestroy = false)
        {

        }
    }
}
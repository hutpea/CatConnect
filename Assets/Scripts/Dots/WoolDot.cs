using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatConnect
{
    public class WoolDot : Dot
    {
        protected Sprite _anchorSprite;

        protected override void Awake()
        {
            base.Awake();
            _anchorSprite = GameAssets.Instance.wool;
            transform.name = "Wool";
        }

        protected override void Start()
        {
            base.Start();
            Board.currentWoolDot += 1;
        }

        public override void OnPostDotFallAnimation()
        {
            if(Board.CheckThisPositionIsTheMostBelow(Row, Column))
            {
                Debug.LogError("Wool enter on postfallanim at " + new Vector2Int(Row, Column) + " isBottom = " + Board.CheckThisPositionIsTheMostBelow(Row, Column));
                Board.currentWoolDot -= 1;
                Board.waitDestroyPosition.Add(new Vector2Int(Row, Column));
            }
        }
    }
}
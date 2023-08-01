using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

namespace CatConnect
{
    public class IceTile : Tile
    {
        private SkeletonGraphic skeletonGraphic;

        public bool isRemoved;

        public override void Awake()
        {
            base.Awake();
            type = TILE_TYPE.Ice;
            HitPoints = 2;
            isRemoved = false;
            transform.name = "Ice";
            skeletonGraphic = GetComponent<SkeletonGraphic>();
        }
        
        public override void OnPostMatch()
        {
            if (!isRemoved)
            {
                HitPoints -= 1;
                UpdateRender();
                if (HitPoints <= 0)
                {
                    RemoveOutOfBoard();
                }
            }
        }

        private void UpdateRender()
        {
            Debug.Log("update render at HP = " + HitPoints);
            if (HitPoints == 1)
            {
                PlayCrackAnimation();
                transform.SetAsLastSibling();
            }
            else if (HitPoints == 0)
            {
                isRemoved = true;
                PlayBrokenAnimation();
                transform.SetAsLastSibling();
                return;
            }
            else
            {
                Debug.LogError("There is error at UpdateRender() of Block Tile at " + new Vector2Int(Row, Column));
            }
        }

        private void PlayCrackAnimation()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, GameAssets.Instance.iceCrack, false);
        }

        private void PlayBrokenAnimation()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, GameAssets.Instance.iceBroken, false);
        }

        private void RemoveOutOfBoard()
        {
            
            GameplayController.Instance.level.levelTarget.UpdateLevelTargetFields(LEVEL_TARGET_TYPE.Ice, 1);
            Board.RemoveBoardOccupantGameObject(this);
        }
    }
}
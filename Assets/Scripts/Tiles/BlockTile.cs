using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace CatConnect
{
    public class BlockTile : Tile
    {
        private bool _isRemoved;
        private SkeletonGraphic skeletonGraphic;

        public override void Awake()
        {
            base.Awake();
            type = TILE_TYPE.Block;
            HitPoints = 1;
            _isRemoved = false;
            transform.name = "Block";
            skeletonGraphic = GetComponent<SkeletonGraphic>();
        }

        public override void OnPostMatch()
        {
            if (!_isRemoved)
            {
                HitPoints -= 1;
                if (HitPoints <= 0)
                {
                    RemoveOutOfBoard();
                }
            }
        }

        private void PlayBoomAnimation()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, GameAssets.Instance.boxBoom, false);
        }
        private void RemoveOutOfBoard()
        {
            _isRemoved = true;
            GameplayController.Instance.level.levelTarget.UpdateLevelTargetFields(LEVEL_TARGET_TYPE.Block, 1);
            Board.RemoveBoardOccupantGameObject(this);
        }
    }
}

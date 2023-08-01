using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

namespace CatConnect
{
    public class AnyColorDot : Dot
    {
        protected COLOR_TYPE fillColor;
        private SkeletonGraphic skeletonGraphic;
        private CatAnimationData catAnimationData;
        protected override void Awake()
        {
            base.Awake();
            transform.name = "AnyColorDot";
            skeletonGraphic = GetComponent<SkeletonGraphic>();
            skeletonGraphic.raycastTarget = false;
        }

        public virtual void FillDot(COLOR_TYPE colorType)
        {
            Debug.Log("Fill dot at " + Row + "," + Column + " with " + colorType.ToString());
            catAnimationData = Utility.GetCatAnimationData(dotType);
            skeletonGraphic.skeletonDataAsset = catAnimationData.skeletonDataAsset;
            PlayAnimationIdle();
            switch (colorType)
            {
                case COLOR_TYPE.RedColor:
                    {
                        /*catAnimationData = Utility.GetCatAnimationData(dotType);
                        skeletonGraphic.Initialize(true);
                        skeletonGraphic.SetMaterialDirty();
                        skeletonGraphic.AnimationState.SetAnimation(0, catAnimationData.idle, true);*/
                        break;
                    }
                case COLOR_TYPE.YellowColor:
                    {
                        //transform.GetComponent<Image>().sprite = GameAssets.Instance.yellowCat;
                        break;
                    }
                case COLOR_TYPE.WhiteColor:
                    {
                        //transform.GetComponent<Image>().sprite = GameAssets.Instance.whiteCat;
                        break;
                    }
                case COLOR_TYPE.GreenColor:
                    {
                        //transform.GetComponent<Image>().sprite = GameAssets.Instance.greenCat;
                        break;
                    }
                case COLOR_TYPE.PurpleColor:
                    {
                        //transform.GetComponent<Image>().sprite = GameAssets.Instance.purpleCat;
                        break;
                    }
                case COLOR_TYPE.PinkColor:
                    {
                        //transform.GetComponent<Image>().sprite = GameAssets.Instance.pinkCat;
                        break;
                    }
            }
        }

        public void PlayAnimationIdle()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, catAnimationData.idle, true);
        }

        public void PlayAnimationChoosen()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, catAnimationData.choose, true);
        }

        public void PlayAnimationJump()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, catAnimationData.jumping, false);
        }

        public void PlayAnimationJumpingOut()
        {
            skeletonGraphic.Initialize(true);
            skeletonGraphic.SetMaterialDirty();
            skeletonGraphic.AnimationState.SetAnimation(0, catAnimationData.jumpingOut, false);
        }
    }
}

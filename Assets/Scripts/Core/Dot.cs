using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using MoreMountains.NiceVibrations;

namespace CatConnect
{
    public class Dot : BoardOccupant, IPointerDownHandler, IPointerEnterHandler
    {
        public DOT_TYPE dotType;
        private COLOR_TYPE _colorType;
        private bool enableConnect;
        public Transform dotTransform;
        public RectTransform dotRectTransform;
        public Image dotImage;

        [SerializeField] private bool _clickable;
        [SerializeField] private bool _selectableForPowerUp;

        public bool Clickable { get { return _clickable; } }
        public virtual COLOR_TYPE ColorType { get { return _colorType; } }

        protected virtual void Awake() 
        {
            dotType = DOT_TYPE.None;
            dotTransform = GetComponent<Transform>();
            dotRectTransform = GetComponent<RectTransform>();
            dotImage = GetComponent<Image>();
            name = "Dot";
        }

        protected override void Start()
        {
            base.Start();
        }

        public void SetupOnCreate(int row, int column)
        {
            Row = row;
            Column = column;

            Init(Utility.IsThisDotTypeColor(dotType));
        }

        public void Init(bool isColorDot = false)
        {
            enableConnect = false;
            if (Utility.IsThisDotTypeAbleToConnect(dotType))
            {
                enableConnect = true;
            }

            if (isColorDot)
            {
                _colorType = Utility.FromDotTypeToColorType(dotType);
                switch (dotType)
                {
                    case DOT_TYPE.RedCat:
                    case DOT_TYPE.YellowCat:
                    case DOT_TYPE.WhiteCat:
                    case DOT_TYPE.GreenCat:
                    case DOT_TYPE.PurpleCat:
                    case DOT_TYPE.PinkCat:
                        {
                            GetComponent<AnyColorDot>().FillDot(_colorType);
                            break;
                        }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(GameplayController.Instance.gameplayUIController.powerUpPhase == GameplayUIController.PowerUpPhase.Erase)
            {
                GameplayController.Instance.gameplayUIController.enablePowerupInteraction = true;
                GameplayController.Instance.level.board.StopAllDotsCircleEffects();
                if (!GameplayController.Instance.level.board.isTutorialStopped)
                {
                    GameplayController.Instance.level.board.StopTutorial();
                }
                GameplayController.Instance.level.board.Erase(Row, Column);
                GameplayController.Instance.gameplayUIController.powerUpPhase = GameplayUIController.PowerUpPhase.None;
                return;
            }
            else if(GameplayController.Instance.gameplayUIController.powerUpPhase == GameplayUIController.PowerUpPhase.SameColor)
            {
                if (!Utility.IsThisDotTypeColor(dotType))
                {
                    return;
                }

                GameplayController.Instance.gameplayUIController.enablePowerupInteraction = true;
                GameplayController.Instance.level.board.StopAllDotsCircleEffects();
                if (!GameplayController.Instance.level.board.isTutorialStopped)
                {
                    GameplayController.Instance.level.board.StopTutorial();
                }
                GameplayController.Instance.level.board.DeleteSameColorDots(dotType);
                GameplayController.Instance.gameplayUIController.powerUpPhase = GameplayUIController.PowerUpPhase.None;
                return;
            }

            if (GameplayController.Instance.level.board.enableTileInteraction && enableConnect)
            {
                Debug.Log("Dot clicked !");
                if (Utility.IsThisDotTypeCatType(dotType))
                {
                    GetComponent<AnyColorDot>().PlayAnimationChoosen();
                    DoDotScaleUpEffect();
                }
                GameplayController.Instance.connectController.isHolding = true;
                GameplayController.Instance.connectController.ClearAllLines();
                GameplayController.Instance.connectController.SetCurrentConnectedDotType(dotType);

                Vector2Int position = new Vector2Int(Row, Column);

                GameplayController.Instance.connectController.firstPoint = position;
                GameplayController.Instance.connectController.lastPoint = position;
                GameplayController.Instance.connectController.AddToConnectedTilesList(position, dotType);
                GameplayController.Instance.connectController.AddPointToLineRenderer(Camera.main.WorldToScreenPoint(Board.cellHolder[Row, Column].position));

                /*var circleDot = Instantiate(GameAssets.Instance.circleDotPrefab, transform.parent.Find("CellEffectHolder"));
                circleDot.SetAsFirstSibling();
                circleDot.GetComponent<CircleDotEffect>().Setup(dotType);*/
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("  [pointer-enter] at " + new Vector2Int(Row, Column));
            //Nếu như player chưa nhả chạm, thực hiện kiểm tra kết nối
            if (GameplayController.Instance.level.board.enableTileInteraction
               && GameplayController.Instance.connectController.isHolding
               && enableConnect)
            {
                MMVibrationManager.Haptic(HapticTypes.MediumImpact);
                Vector2Int position = ToVector2IntCoordinate();

                /*var circleDot = Instantiate(GameAssets.Instance.circleDotPrefab, transform.parent.Find("CellEffectHolder"));
                circleDot.SetAsFirstSibling();
                circleDot.GetComponent<CircleDotEffect>().Setup(dotType);*/

                //Nếu dot cùng màu với màu đang kết nối và nằm kế dot trước đó
                if (GameplayController.Instance.connectController.GetCurrentConnectedDotType() == dotType
                    && IsThisPositionNextToThisPosition(position, GameplayController.Instance.connectController.lastPoint))
                {
                    //Dot không có trong danh sách đã kết nối
                    if (!IsThisPositionExistsInCurrentConnectedTilesList(position))
                    {
                        Debug.Log("Not in list");

                        if (Utility.IsThisDotTypeCatType(dotType))
                        {
                            GetComponent<AnyColorDot>().PlayAnimationChoosen();
                            DoDotScaleUpEffect();
                        }

                        GameplayController.Instance.connectController.AddToConnectedTilesList(position, dotType);
                        GameplayController.Instance.connectController.AddPointToLineRenderer(Camera.main.WorldToScreenPoint(Board.cellHolder[Row, Column].position));
                    }
                    else
                    {

                        //Dot nằm trong danh sách đã có, nhưng là điểm ngay trước nó
                        if (IsThisPositionSecondLastPointInConnectedTilesList(position))
                        {
                            Debug.Log("In list, is second last point");
                            GameplayController.Instance.connectController.RemoveTopDot_Composite();
                            GameplayController.Instance.connectController.isMakeASquare = false;
                        }
                        //Dot nằm trong danh sách đã có, nhưng là điểm đầu tiên và có đúng 4 diem nối
                        else if (IsThisPositionFirstPointInConnectedTilesList(position)
                            && GameplayController.Instance.connectController.currentMatch.Count == 4)
                        {
                            Debug.Log("In list, is first point, make square");
                            MMVibrationManager.Haptic(HapticTypes.HeavyImpact);

                            GameplayController.Instance.connectController.AddToConnectedTilesList(position, dotType);
                            GameplayController.Instance.connectController.AddPointToLineRenderer(Camera.main.WorldToScreenPoint(Board.cellHolder[Row, Column].position));
                            GameplayController.Instance.connectController.isMakeASquare = true;
                            GameplayController.Instance.level.board.waitDestroyPosition.Clear();
                            List<Vector2Int> sameColorTiles = GameplayController.Instance.level.board.GetListOfSameColorTiles(
                                GameplayController.Instance.connectController.GetCurrentConnectedDotType(), 
                                GameplayController.Instance.connectController.GetDistinctCurrentMatch());
                            GameplayController.Instance.level.board.waitDestroyPosition.AddRange(sameColorTiles);
                        }
                    }
                }
            }
        }

        protected virtual void PlayAnimationChosen()
        {

        }

        private void DoDotScaleUpEffect()
        {
            dotTransform.DOScale(1.15f, 0.25f).SetEase(Ease.OutBack);
        }

        public void ResetDotScaleToNormal()
        {
            dotTransform.DOScale(1f, 0.25f).SetEase(Ease.InBack);
        }

        public virtual void OnBeforeDotFallAnimation() { }

        public virtual void OnPostDotFallAnimation() { }

        public virtual void OnHit(DOT_TYPE byDotType = 0) { }

        public virtual void OnHit(TILE_TYPE byTileType) { }

        protected void IncrementGoal() { }

        public virtual void OnDisconnect(bool afterSquare = false) { }

        public virtual void OnHover(bool forSquare = false) { }

        public void SetDotColor(DOT_TYPE dotType)
        {
            dotImage.color = Utility.GetColorFromDotType(dotType);
        }

        public bool IsThisPositionNextToThisPosition(Vector2Int p1, Vector2Int p2)
        {
            if (Mathf.Abs(p1.x - p2.x) == 1 && p1.y == p2.y || Mathf.Abs(p1.y - p2.y) == 1 && p1.x == p2.x)
            {
                return true;
            }
            return false;
        }

        public bool IsThisPositionExistsInCurrentConnectedTilesList(Vector2Int position)
        {
            foreach (Vector2Int i in GameplayController.Instance.connectController.currentMatch)
            {
                if (position == i)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsThisPositionFirstPointInConnectedTilesList(Vector2Int position)
        {
            return position == GameplayController.Instance.connectController.firstPoint;
        }
        public bool IsThisPositionSecondLastPointInConnectedTilesList(Vector2Int position)
        {
            return position == GameplayController.Instance.connectController.secondLastPoint;
        }
        public bool IsThisPositionLastPointInConnectedTilesList(Vector2Int position)
        {
            return position == GameplayController.Instance.connectController.lastPoint;
        }
        public bool IsThisPositionIsAPointBetweenConnectedTilesList(Vector2Int position)
        {
            if (IsThisPositionExistsInCurrentConnectedTilesList(position) && !IsThisPositionFirstPointInConnectedTilesList(position))
            {
                return true;
            }
            return false;
        }
    }
}
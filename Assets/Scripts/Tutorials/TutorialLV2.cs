using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Vectrosity;

public class TutorialLV2 : MonoBehaviour
{
    public Transform hand;
    public VectorLine vectorLine;
    public Texture thickLineTexture;

    public float cellOffset;

    Vector3[,] cellPos;

    public bool isAnim;

    private Vector3 originalScale;

    private void Awake()
    {
        isAnim = false;
        originalScale = this.transform.localScale;
        vectorLine = VectorLine.SetLine(Color.white, new Vector2(0, 0), new Vector2(0, 0));
        vectorLine.lineWidth = 0.08f;
        vectorLine.joins = Joins.Fill;
        vectorLine.texture = thickLineTexture;
        vectorLine.color = Utility.GetColorFromDotType(DOT_TYPE.RedCat);
        VectorLine.SetCanvasCamera(Camera.main);
        VectorLine.canvas.sortingOrder = 1;
    }

    public void Setup()
    {
        cellPos = new Vector3[5, 5];
        for(int i = 1; i <= 4; i++)
        {
            for (int j = 1; j <= 4; j++)
            {
                cellPos[i, j] = GameplayController.Instance.level.board.cellWorldPositions[i, j];
            }
        }
        hand.position = cellPos[1, 1];
        isAnim = true;
        AnimLoop();
    }

    private void Update()
    {
        /*vectorLine.points2.Clear();
        vectorLine.points2.Add(cellPos[1, 1]);
        vectorLine.points2.Add(hand.position);
        vectorLine.Draw();*/
    }

    public void AnimLoop()
    {
        if (isAnim)
        {
            Debug.Log(originalScale);
            Debug.Log("anim looop");
            hand.position = cellPos[2, 2];
            hand.DOMove(cellPos[2, 3], .75f).OnComplete(() =>
            {
                hand.DOMove(cellPos[3, 3], .75f).OnComplete(() =>
                {
                    hand.DOMove(cellPos[3, 2], .75f).OnComplete(() =>
                    {
                        hand.DOMove(cellPos[2, 2], .75f).OnComplete(() =>
                        {
                            hand.DOScale(0.33f, .75f).OnComplete(() =>
                            {
                                AnimLoop();
                            });
                        });
                    });
                });
            });
        }
    }

    public void DeleteTutorial()
    {
        GameController.Instance.AnalyticsController.LogTutLevelEnd(UseProfile.CurrentLevel);
        isAnim = false;
        hand.DOKill();
        VectorLine.Destroy(ref vectorLine);
        Destroy(this.gameObject);
    }
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using DG.Tweening;

public class ConnectController : MonoBehaviour
{
    public bool isHolding;

    public Texture thickLineTexture;

    public VectorLine vectorLine;

    public List<Vector2Int> currentMatch;
    public List<DOT_TYPE> matchTypes;

    public DOT_TYPE currentConnectedDotType;
    public Vector2Int firstPoint;
    public Vector2Int secondLastPoint;
    public Vector2Int lastPoint;
    public bool isMakeASquare;

    public bool inProcessPhase = false;
    public bool touchEndPhase = false;
    public bool onConnectEndFinish;
    private void Start()
    {
        /*
         * SET UP VECTROSITY LINE
         */
        vectorLine = VectorLine.SetLine(Color.white, new Vector2(0, 0), new Vector2(0, 0));
        vectorLine.lineWidth = 20f;
        vectorLine.joins = Joins.Fill;
        vectorLine.texture = thickLineTexture;
        VectorLine.SetCanvasCamera(Camera.main);
        VectorLine.canvas.sortingOrder = 0;

        isHolding = false;
        isMakeASquare = false;
        currentConnectedDotType = DOT_TYPE.None;
    }

    private void Update()
    {
        if (!inProcessPhase)
        {
            if (Input.GetMouseButtonUp(0))
            {
                inProcessPhase = true;
                Debug.Log("GetMouseButtonUp. TouchEndPhase = " + touchEndPhase);
                isHolding = false;
                ClearAllLines();
                if (currentMatch.Count > 1)
                {
                    if (!CheckAllMatchTypesIsSame())
                    {
                        inProcessPhase = false;
                        ResetVariables();
                        return;
                    }
                    if (!CheckIsMatchIsValid())
                    {
                        inProcessPhase = false;
                        ResetVariables();
                        return;
                    }
                    Debug.Log("Enter end phase");
                    if (!GameplayController.Instance.level.board.isTutorialStopped)
                    {
                        GameplayController.Instance.level.board.StopTutorial();
                    }
                    touchEndPhase = true;
                    StartCoroutine(DeleteAllConnectedTiles());
                }
                else
                {
                    inProcessPhase = false;
                    ResetVariables();
                }
            }
        }
    }

    private bool CheckIsMatchIsValid()
    {
        for (int i = 0; i < currentMatch.Count - 1; i++)
        {
            if (!IsThisPositionNextToThisPosition(currentMatch[i], currentMatch[i+1]))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsThisPositionNextToThisPosition(Vector2Int p1, Vector2Int p2)
    {
        if (Mathf.Abs(p1.x - p2.x) == 1 && p1.y == p2.y || Mathf.Abs(p1.y - p2.y) == 1 && p1.x == p2.x)
        {
            return true;
        }
        return false;
    }

    private bool CheckAllMatchTypesIsSame()
    {
        for(int i = 0; i < matchTypes.Count - 1; i++)
        {
            if (matchTypes[i] != matchTypes[i + 1])
            {
                return false;
            }
        }
        return true;
    }

    public DOT_TYPE GetCurrentConnectedDotType()
    {
        return this.currentConnectedDotType;
    }

    public void SetCurrentConnectedDotType(DOT_TYPE dotType)
    {
        vectorLine.color = Utility.GetColorFromDotType(dotType);
        this.currentConnectedDotType = dotType;
    }

    public void AddToConnectedTilesList(Vector2Int position, DOT_TYPE dotType)
    {
        GameController.Instance.musicManager.connectNumber++;
        secondLastPoint = lastPoint;
        this.currentMatch.Add(position);
        this.matchTypes.Add(dotType);
        lastPoint = position;
        GameController.Instance.musicManager.PlayConnectingDotSound();
    }

    public void RemoveTopDot_Composite()
    {
        GameplayController.Instance.level.board.ResetDotsScale(currentMatch[currentMatch.Count - 1]);
        GameController.Instance.musicManager.connectNumber--;

        currentMatch.RemoveAt(currentMatch.Count - 1);
        matchTypes.RemoveAt(matchTypes.Count - 1);
        lastPoint = currentMatch[currentMatch.Count - 1];
        if(currentMatch.Count > 1)
        {
            secondLastPoint = currentMatch[currentMatch.Count - 2];
        }

        vectorLine.points2.RemoveAt(vectorLine.points2.Count - 1);
        vectorLine.Draw();
    }

    public IEnumerator DeleteAllConnectedTiles()
    {
        onConnectEndFinish = false;
        GameplayController.Instance.level.board.OnConnectEnd(CatConnect.GameplayUIController.PowerUpPhase.None);
        yield return new WaitUntil(() => onConnectEndFinish == true);
        ResetVariables();
    }

    public void ResetVariables()
    {
        GameController.Instance.musicManager.connectNumber = 0;
        GameplayController.Instance.level.board.waitDestroyPosition.Clear();
        currentMatch.Clear();
        matchTypes.Clear();
        firstPoint = Vector2Int.zero;
        secondLastPoint = Vector2Int.zero;
        lastPoint = Vector2Int.zero;
        isMakeASquare = false;
        SetCurrentConnectedDotType(DOT_TYPE.None);
    }

    public void ClearAllLines()
    {
        Debug.Log("[UI] Clear all lines");
        GameplayController.Instance.level.board.ResetAllDotsScale(currentMatch);

        vectorLine.points2.Clear();
        vectorLine.Draw();
    }

    public void AddPointToLineRenderer(Vector2 point)
    {
        if (isHolding)
        {
            vectorLine.points2.Add(point);
            vectorLine.Draw();
        }
    }

    public List<Vector2Int> GetDistinctCurrentMatch()
    {
        return currentMatch.Distinct().ToList();
    }
}

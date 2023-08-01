using CatConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoardOccupant : MonoBehaviour
{
    protected const float SHRINK_OUT_TIME = 0.3f;
    protected const float EXPAND_IN_TIME = 1;

    private int _row;
    private int _col;
    private int _hitPoints;
    private Board _board;

    public virtual int MaxHitPoints { get; }
    public virtual int ScoreValue { get; }

    public virtual int Column { get { return _col; } set { _col = value; } }
    public virtual int Row { get { return _row; } set { _row = value; } }

    public int HitPoints { get { return _hitPoints; } set { _hitPoints = value; } }

    public virtual Board Board { get { return _board; } set { _board = value; } }
    public virtual Vector3 Position { get; set; }
    protected virtual void HandleColorUpdate() { }
    public virtual void OnPrepare() { }
    public virtual void OnPostMatch() { }
    public virtual void OnBoardFillFinished() { }
    //public virtual void OnHitByPowerUp(PowerUpHitInfo powerUpType) { }

    protected virtual void Start()
    {
        Board = GameplayController.Instance.level.board;
    }

    public BoardOccupant GetNeighbor(NeighborDirection relativeDirection)
    {
        //T result = default(T);
        BoardOccupant result = null;
        Vector2Int position = new Vector2Int(Row, Column);
        switch (relativeDirection)
        {
            case NeighborDirection.Above:
                {
                    if (Row <= 1)
                    {
                        return result;
                    }
                    position.x--;
                    break;
                }
            case NeighborDirection.Below:
                {
                    if (Row >= Board.Height)
                    {
                        return result;
                    }
                    position.x++;
                    break;
                }
            case NeighborDirection.Left:
                {
                    if (Column <= 1)
                    {
                        return result;
                    }
                    position.y--;
                    break;
                }
            case NeighborDirection.Right:
                {
                    if (Column >= Board.Width)
                    {
                        return result;
                    }
                    position.y++;
                    break;
                }
            default: break;
        }
        if (GetType().Equals(typeof(Dot)))
        {
            result = (Dot)Board.GetDot(position);
        }
        if (GetType().Equals(typeof(Tile)))
        {
            result = (Tile)Board.GetTile(position);
        }
        return result;
    }

    public bool IsAdjacentTo(BoardOccupant otherBoardOccupant)
    {
        if(!Board.IsValidPosition(Row, Column)
            || !Board.IsValidPosition(otherBoardOccupant.Row, otherBoardOccupant.Column))
        {
            return false;
        }
        int r1 = Row;
        int c1 = Column;
        int r2 = otherBoardOccupant.Row;
        int c2 = otherBoardOccupant.Column;

        return (r1 == r2 && Mathf.Abs(c1 - c2) == 1) || (c1 == c2 && Mathf.Abs(r1 - r2) == 1);
    }

    /*public List<BoardOccupant> GetAllAdjacentNeighbors()
    {
        List<BoardOccupant> list = new List<BoardOccupant>();
        if()
        return list;
    }*/

    protected Vector2Int ToVector2IntCoordinate()
    {
        return new Vector2Int(Row, Column);
    }
}
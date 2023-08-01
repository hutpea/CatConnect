using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatConnect
{
    public class WaterTileGlobalHandler : MonoBehaviour
    {
        public List<Vector2Int> wateredCells;
        private bool _isSpreadingWater;
        public bool IsSpreadingWater { get => _isSpreadingWater; set => _isSpreadingWater = value; }

        private void Awake()
        {
            wateredCells = new List<Vector2Int>();
        }

        public void SpreadingWater(bool[,] mat) 
        {
            string str = "";
            for (int i = 1; i <= GameplayController.Instance.level.board.Height; i++)
            {
                for (int j = 1; j <= GameplayController.Instance.level.board.Width; j++)
                {
                    str += mat[i, j].ToString() + " ";
                }
                str += "\n";
            }
            Debug.Log(str);

            List<Vector2Int> filledPositions = new List<Vector2Int>();

            foreach (var wateredCell in wateredCells)
            {
                for (int i = 1; i <= GameplayController.Instance.level.board.Height; i++)
                {
                    for (int j = 1; j <= GameplayController.Instance.level.board.Width; j++)
                    {
                        if (mat[i, j] && !wateredCells.Contains(new Vector2Int(i, j)) && !filledPositions.Contains(new Vector2Int(i, j)))
                        {
                            if (BreathFirstSearch(mat, wateredCell, new Vector2Int(i, j)))
                            {
                                string s = "";
                                foreach(Transform c in GameplayController.Instance.level.board.cellHolder[i, j].transform)
                                {
                                    s += c.name + " ";
                                }
                                Debug.Log("Cell" + new Vector2Int(i, j) + ":" + s);
                                var waterTileGO = Instantiate(GameAssets.Instance.waterTile, GameplayController.Instance.level.board.cellHolder[i, j].transform);
                                waterTileGO.GetComponent<WaterTile>().FillWater();
                                waterTileGO.GetComponent<WaterTile>().isFilled = true;
                                waterTileGO.SetAsFirstSibling();
                                filledPositions.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
            }

            foreach(var fillPos in filledPositions)
            {
                AddToWateredCellsList(fillPos);
            }

            _isSpreadingWater = false;
        }

        private IEnumerator SpreadingWaterCoroutine(bool[,] mat)
        {
            yield return null;
        }

        public void AddToWateredCellsList(Vector2Int position)
        {
            if (!wateredCells.Contains(position))
            {
                wateredCells.Add(position);
            }
            else
            {
                Debug.Log("This position is already in watered list !");
            }
        }

        public bool IsWaterShouldReachToThisPosition(Vector2Int position)
        {
            if (GameplayController.Instance.level.board.gameTiles[position.x, position.y] != null)
            {
                switch (GameplayController.Instance.level.board.GetTile(position).type)
                {
                    case TILE_TYPE.Gap:
                    case TILE_TYPE.Block:
                    case TILE_TYPE.BlockLeft:
                    case TILE_TYPE.BlockRight:
                    case TILE_TYPE.BlockUp:
                    case TILE_TYPE.BlockDown:
                    case TILE_TYPE.Ice:
                        return false;
                    default:
                        return true;
                }
            }
            else
            {
                return true;
            }
        }

        int[] rowNum = { -1, 0, 0, 1 };
        int[] colNum = { 0, -1, 1, 0 };

        struct QueueNode
        {
            public Vector2Int point;
            public int distance;

            public QueueNode(Vector2Int point, int distance)
            {
                this.point = point;
                this.distance = distance;
            }
        }

        private bool BreathFirstSearch(bool[,] mat, Vector2Int source, Vector2Int destination)
        {
            // check source and destination cell of the matrix have value 1
            if (!mat[source.x,source.y] || !mat[destination.x, destination.y])
                return false;

            bool[,] visited = new bool[mat.GetLength(0), mat.GetLength(1)];
            for(int i = 1; i <= GameplayController.Instance.level.board.Height; i++)
            {
                for (int j = 1; j <= GameplayController.Instance.level.board.Width; j++)
                {
                    visited[i, j] = false;
                }
            }

            // Mark the source cell as visited
            visited[source.x, source.y] = true;

            // Create a queue for BFS
            Queue<QueueNode> q = new Queue<QueueNode>();

            // Distance of source cell is 0
            QueueNode s = new QueueNode(source, 0);
            q.Enqueue(s);  // Enqueue source cell

            // Do a BFS starting from source cell
            while (!(q.Count == 0))
            {
                QueueNode curr = q.Peek();
                Vector2Int pt = curr.point;

                // If we have reached the destination cell,
                // we are done
                if (pt.x == destination.x && pt.y == destination.y)
                {
                    //return curr.dist;
                    return true;
                }

                // Otherwise dequeue the front
                // cell in the queue
                // and enqueue its adjacent cells
                q.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int row = pt.x + rowNum[i];
                    int col = pt.y + colNum[i];

                    // if adjacent cell is valid, has path and
                    // not visited yet, enqueue it.
                    if (GameplayController.Instance.level.board.IsValidPosition(row, col) && mat[row, col]
                       && IsWaterShouldReachToThisPosition(new Vector2Int(row, col))
                       && !visited[row, col])
                    {
                        // mark cell as visited and enqueue it
                        visited[row, col] = true;
                        QueueNode Adjcell = new QueueNode(new Vector2Int(row, col), curr.distance + 1);
                        q.Enqueue(Adjcell);
                    }
                }
            }

            // Return -1 if destination cannot be reached
            return false;
        }
    }
}

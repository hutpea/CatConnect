using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

namespace CatConnect
{
    public class Board : MonoBehaviour
    {
        [Header("UI Elements")]
        public Transform mainCanvas;
        public Transform boardUI;
        public Transform visualCellHolderContainerUI;
        public GameObject disableClickPanel;

        [Header("GlobalHandler")]
        public Ease catFallEaseType;

        [Header("Control")]
        private int _height;
        private int _width;
        public string[,] boardLayoutMatrix;
        public List<int> choosenCats;
        public Transform[,] cellHolder;
        public Transform[,] visualCellHolder;
        public Dot[,] gameDots;
        public Tile[,] gameTiles;

        public bool enableTileInteraction;
        public bool enablePowerupsInteraction;

        public bool isTileDeletionCompleted;
        public bool isTileMoveCompleted;
        public bool isCheckLevelStateCompleted;

        public int currentWoolDot;
        public int newDotsCount;

        public Vector3[,] cellWorldPositions;
        public List<Vector2Int> connectedTiles;
        public List<Vector2Int> waitDestroyPosition;
        public List<Vector2Int> hitTilesByMatch;

        public int Height { get => _height; set => _height = value; }
        public int Width { get => _width; set => _width = value; }

        public bool isTutorialStopped = false;

        public void Init()
        {
            //Time.timeScale = .2f;
            Canvas.ForceUpdateCanvases();

            if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
            {
                UseProfile.CurrentLevel = Context.MAX_LEVEL;
            }
            CreateBoard(UseProfile.CurrentLevel);
            GameController.Instance.AnalyticsController.LogStartLevel(UseProfile.CurrentLevel);
        }

        #region Setup methods

        private void CreateBoard(int levelNumber)
        {
            Debug.LogAssertion("BEGIN Creating board ...");
            var jsonTextFile = Resources.Load<TextAsset>("Levels/Level" + levelNumber);
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonTextFile.text);
            //LevelData levelData = GameController.Instance.dataContain.levelDataList[levelNumber - 1];
            if (levelData != null)
            {
                GameplayController.Instance.gameplayUIController.SetupLevelText(levelData.levelName);

                Height = levelData.boardHeight;
                Width = levelData.boardWidth;
                choosenCats = levelData.chooseCats;

                List<LevelTargetJson> levelTargetJsons = levelData.levelTargets;
                foreach (var i in levelTargetJsons)
                {
                    Debug.Log("target: " + i.type + " " + i.amount);
                }
                SetupLevelTarget(levelTargetJsons);

                GameplayController.Instance.level.SetupMoveCount(levelData.moves);

                ProcessBoardLayout(levelData.boardLayout);
                SetupCellHolder();
                StartCoroutine(CalculateCellWorldPosition());
                SetupTiles();
                SetupDots();

                isTileDeletionCompleted = false;
                isTileMoveCompleted = false;
                isCheckLevelStateCompleted = false;

                currentWoolDot = 0;
                newDotsCount = 0;

                enableTileInteraction = true;
                enablePowerupsInteraction = true;

                waitDestroyPosition = new List<Vector2Int>();
                hitTilesByMatch = new List<Vector2Int>();

                InitCircleDotsEffectOnBoard();

                Debug.LogAssertion("END Create board done !");
            }
            else
            {
                Debug.LogError(String.Format("Level data {0} not found", levelNumber.ToString()));
            }
        }

        /// <summary>
        /// Init level's data
        /// </summary>
        /// <param name="levelNumber">Level with this number will be initialized for getting data</param>
        private void ProcessBoardLayout(string boardLayoutString)
        {
            Debug.LogAssertion("Begin processing board layout ...");
            //Debug.Log(boardLayoutString);
            string s = "";
            boardLayoutMatrix = new string[_height + 1, _width + 1];
            string[] lines = boardLayoutString.Split('#');
            for (int line = 0; line < lines.Length; line++)
            {
                lines[line] = Utility.RemoveRedundantSpaces(lines[line]);
                string[] cellsInLine = lines[line].Split('~');
                //Debug.Log(line + " " + cellsInLine.Length);
                for (int word = 0; word < cellsInLine.Length; word++)
                {
                    //Debug.Log(cellsInLine[word]);
                    boardLayoutMatrix[line + 1, word + 1] = cellsInLine[word];
                    s += boardLayoutMatrix[line + 1, word + 1] + " ";
                    /*Debug.Log("word: " + word + ", cellAtWord: " + cellsInLine[word]);
                    string celTypeString = cellsInLine[word][0].ToString();
                    if (celTypeString == "c")
                    {
                        string[] cellParts = cellsInLine[word].Split('_');
                    }*/
                }
                s += "\n";
            }
            Debug.Log(s);
        }

        //Create cell holders for containing tiles/dots/machenics
        private void SetupCellHolder()
        {
            GridLayoutGroup gridLayoutGroup = boardUI.GetComponent<GridLayoutGroup>();
            gridLayoutGroup.constraintCount = _width;

            cellHolder = new Transform[_height + 1, _width + 1];
            cellWorldPositions = new Vector3[_height + 1, _width + 1];
            cellEffectHolders = new Transform[_height + 1, _width + 1];

            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    var cellHolderGO = Instantiate(GameAssets.Instance.cellHolderPrefab, boardUI);
                    cellHolder[i, j] = cellHolderGO;
                    cellEffectHolders[i, j] = cellHolder[i, j].GetChild(0);
                }
            }

            GridLayoutGroup gridLayoutGroup_visual = visualCellHolderContainerUI.GetComponent<GridLayoutGroup>();
            gridLayoutGroup_visual.constraintCount = _width;

            visualCellHolder = new Transform[_height + 1, _width + 1];

            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    var cellHolderGO = Instantiate(GameAssets.Instance.cellHolderVisualPrefab, visualCellHolderContainerUI);
                    visualCellHolder[i, j] = cellHolderGO;
                }
            }
        }
        /// <summary>
        /// Must wait a frame for GridLayout update its child right positions
        /// </summary>
        /// <returns></returns>
        private IEnumerator CalculateCellWorldPosition()
        {
            yield return new WaitForEndOfFrame();
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    cellWorldPositions[i, j] = cellHolder[i, j].position;
                }
            }

            SetUpTutorial();
        }

        //Generate tiles into cell holders
        private void SetupTiles()
        {
            gameTiles = new Tile[_height + 1, _width + 1];

            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    string[] cellParts = boardLayoutMatrix[i, j].Split('_');
                    gameTiles[i, j] = null;
                    switch (cellParts[0])
                    {
                        case "gap":
                            {
                                var tile = Instantiate(GameAssets.Instance.gapTile, cellHolder[i, j].transform);
                                visualCellHolder[i, j].GetComponent<Image>().enabled = false;
                                gameTiles[i, j] = tile.GetComponent<Tile>();
                                break;
                            }
                        case "block":
                            {
                                var tile = Instantiate(GameAssets.Instance.blockTile, cellHolder[i, j].transform);
                                tile.SetAsFirstSibling();
                                gameTiles[i, j] = tile.GetComponent<Tile>();
                                break;
                            }
                        case "ice":
                            {
                                var tile = Instantiate(GameAssets.Instance.iceTile, cellHolder[i, j].transform);
                                tile.SetAsFirstSibling();
                                gameTiles[i, j] = tile.GetComponent<Tile>();
                                break;
                            }
                    }
                }
            }
        }


        //Generate dots into cell holders
        private void SetupDots()
        {
            gameDots = new Dot[_height + 1, _width + 1];

            switch (UseProfile.CurrentLevel)
            {
                case 1:
                    {
                        for (int i = 1; i <= _height; i++)
                        {
                            for (int j = 1; j <= _width; j++)
                            {
                                var dotGO = SimplePool.Spawn(GameAssets.Instance.anyColorDot, Vector3.zero, Quaternion.identity);
                                dotGO.SetParent(cellHolder[i, j]);
                                gameDots[i, j] = dotGO.GetComponent<Dot>();
                                dotGO.SetParent(cellHolder[i, j]);
                                gameDots[i, j].dotRectTransform.anchoredPosition = Vector3.zero;
                                gameDots[i, j].dotRectTransform.localScale = new Vector3(1, 1, 1);
                                gameDots[i, j].dotType = DOT_TYPE.RedCat;
                                dotGO.SetAsFirstSibling();
                                GetDot(i, j).SetupOnCreate(i, j);
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        for (int i = 1; i <= _height; i++)
                        {
                            for (int j = 1; j <= _width; j++)
                            {
                                var dotGO = SimplePool.Spawn(GameAssets.Instance.anyColorDot, Vector3.zero, Quaternion.identity);
                                dotGO.SetParent(cellHolder[i, j]);
                                gameDots[i, j] = dotGO.GetComponent<Dot>();
                                dotGO.SetParent(cellHolder[i, j]);
                                gameDots[i, j].dotRectTransform.anchoredPosition = Vector3.zero;
                                gameDots[i, j].dotRectTransform.localScale = new Vector3(1, 1, 1);
                                gameDots[i, j].dotType = DOT_TYPE.YellowCat;
                                dotGO.SetAsFirstSibling();
                                GetDot(i, j).SetupOnCreate(i, j);
                            }
                        }
                        break;
                    }
                case 3:
                    {
                        int r = 0;
                        for (int i = 1; i <= _height; i++)
                        {
                            for (int j = 1; j <= _width; j++)
                            {
                                var dotGO = SimplePool.Spawn(GameAssets.Instance.anyColorDot, Vector3.zero, Quaternion.identity);
                                dotGO.SetParent(cellHolder[i, j]);
                                gameDots[i, j] = dotGO.GetComponent<Dot>();
                                dotGO.SetParent(cellHolder[i, j]);
                                gameDots[i, j].dotRectTransform.anchoredPosition = Vector3.zero;
                                gameDots[i, j].dotRectTransform.localScale = new Vector3(1, 1, 1);
                                gameDots[i, j].dotType = (r % 2 == 0) ? DOT_TYPE.YellowCat : DOT_TYPE.RedCat;
                                dotGO.SetAsFirstSibling();
                                GetDot(i, j).SetupOnCreate(i, j);
                                r++;
                            }
                            r++;
                        }
                        break;
                    }
                default:
                    {
                        for (int i = 1; i <= _height; i++)
                        {
                            for (int j = 1; j <= _width; j++)
                            {
                                Debug.Log(new Vector2Int(i, j));
                                string[] cellParts = boardLayoutMatrix[i, j].Split('_');
                                if (cellParts[0].Equals("gap") || cellParts[0].Equals("block"))
                                {
                                    continue;
                                }
                                switch (cellParts[1])
                                {
                                    case "any":
                                        {
                                            //var dotGO = Instantiate(GameAssets.Instance.anyColorDot, cellHolder[i, j]);
                                            var dotGO = SimplePool.Spawn(GameAssets.Instance.anyColorDot, Vector3.zero, Quaternion.identity);
                                            gameDots[i, j] = dotGO.GetComponent<Dot>();

                                            dotGO.SetParent(cellHolder[i, j]);
                                            gameDots[i, j].dotRectTransform.anchoredPosition = Vector3.zero;
                                            gameDots[i, j].dotRectTransform.localScale = new Vector3(1, 1, 1);

                                            gameDots[i, j].dotType = Utility.FromIntToDotType(choosenCats[UnityEngine.Random.Range(0, choosenCats.Count)]);

                                            //Level fix
                                            if(UseProfile.CurrentLevel == 16)
                                            {
                                                gameDots[i, j].dotType = (i == 1) ? DOT_TYPE.YellowCat : DOT_TYPE.RedCat;
                                            }

                                            dotGO.SetAsFirstSibling();
                                            GetDot(i, j).SetupOnCreate(i, j);
                                            break;
                                        }
                                    case "wool":
                                        {
                                            //var dotGO = Instantiate(GameAssets.Instance.woolDotPrefab, cellHolder[i, j]);
                                            var dotGO = SimplePool.Spawn(GameAssets.Instance.woolDotPrefab, Vector3.zero, Quaternion.identity);
                                            gameDots[i, j] = dotGO.GetComponent<Dot>();

                                            dotGO.SetParent(cellHolder[i, j]);
                                            gameDots[i, j].dotRectTransform.anchoredPosition = Vector3.zero;
                                            gameDots[i, j].dotRectTransform.localScale = new Vector3(1, 1, 1);

                                            gameDots[i, j].dotType = DOT_TYPE.Wool;
                                            dotGO.SetAsFirstSibling();
                                            GetDot(i, j).SetupOnCreate(i, j);
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private void SetupLevelTarget(List<LevelTargetJson> levelTargetsJson)
        {
            List<TargetItem> targetItems = new List<TargetItem>();
            foreach (var targetJson in levelTargetsJson)
            {
                LEVEL_TARGET_TYPE levelTargetType = Utility.FromStringToLevelTargetType(targetJson.type);
                int targetAmount = targetJson.amount;
                TargetItem targetItem = new TargetItem(levelTargetType, targetAmount, 0);
                targetItems.Add(targetItem);
            }
            if (targetItems.Count > 0)
            {
                GameplayController.Instance.level.levelTarget.Setup(targetItems);
            }
            else
            {
                Debug.LogError("There something wrong when fetch level targets");
            }
        }

        TutorialLV1 tutorialLV1;
        TutorialLV2 tutorialLV2;
        TutorialRandomize tutorialRandomize;
        TutorialErase tutorialErase;
        TutorialSameColor tutorialSameColor;
        public bool isDoneSecondPhase = false;

        private void SetUpTutorial()
        {
            GameController.Instance.AnalyticsController.LogTutLevelStart(UseProfile.CurrentLevel);
            switch (UseProfile.CurrentLevel)
            {
                case 1:
                    {
                        var tut = Instantiate(GameAssets.Instance.tutorialLV1Prefab);
                        tutorialLV1 = tut.GetComponent<TutorialLV1>();
                        tutorialLV1.Setup();
                        break;
                    }
                case 2:
                    {
                        var tut = Instantiate(GameAssets.Instance.tutorialLV2Prefab);
                        tutorialLV2 = tut.GetComponent<TutorialLV2>();
                        tutorialLV2.Setup();
                        break;
                    }
                case 3:
                    {
                        var tut = Instantiate(GameAssets.Instance.tutorialRandomizePrefab);
                        tutorialRandomize = tut.GetComponent<TutorialRandomize>();
                        tutorialRandomize.Setup();
                        break;
                    }
                case 4:
                    {
                        var tut = Instantiate(GameAssets.Instance.tutorialErasePrefab);
                        tutorialErase = tut.GetComponent<TutorialErase>();
                        tutorialErase.Setup();
                        break;
                    }
                case 5:
                    {
                        var tut = Instantiate(GameAssets.Instance.tutorialSameColorPrefab);
                        tutorialSameColor = tut.GetComponent<TutorialSameColor>();
                        tutorialSameColor.Setup();
                        break;
                    }
                default:
                    {

                        break;
                    }
            }
        }

        public void TutorialDoSecondPhase()
        {
            if (isDoneSecondPhase) return;
            switch (UseProfile.CurrentLevel)
            {
                case 4:
                    {
                        tutorialErase.DoSecondPhase();
                        break;
                    }
                case 5:
                    {
                        tutorialSameColor.DoSecondPhase();
                        break;
                    }
            }
            isDoneSecondPhase = true;
        }

        public void StopTutorial()
        {
            switch (UseProfile.CurrentLevel)
            {
                case 1:
                    {
                        tutorialLV1.DeleteTutorial();
                        break;
                    }
                case 2:
                    {
                        tutorialLV2.DeleteTutorial();
                        break;
                    }
                case 3:
                    {
                        tutorialRandomize.DeleteTutorial();
                        break;
                    }
                case 4:
                    {
                        tutorialErase.DeleteTutorial();
                        break;
                    }
                case 5:
                    {
                        tutorialSameColor.DeleteTutorial();
                        break;
                    }
                default:
                    {

                        break;
                    }
            }
            isTutorialStopped = true;
        }

        #endregion

        /// <summary>
        /// This function is called by ConnectController when finish a connect line or by using Erase powerup
        /// </summary>
        /// <param name="isErasePhase">Is Erase powerup calls this function ?</param>
        public void OnConnectEnd(GameplayUIController.PowerUpPhase powerUpPhase = GameplayUIController.PowerUpPhase.None)
        {
            StartCoroutine(OnConnectEndCoroutine(powerUpPhase));
        }

        /// <summary>
        /// The most important function in project, this coroutine handles all events happen after we connect a line;
        /// It handles jobs like deleting objects, obejects falling down, checking level state
        /// </summary>
        /// <param name="isErasePhase"></param>
        /// <returns></returns>
        public IEnumerator OnConnectEndCoroutine(GameplayUIController.PowerUpPhase powerUpPhase)
        {
            Debug.Log(">> Begin UniTask OnConnectEnds(), disable tileInteraction.");
            disableClickPanel.SetActive(true);
            enableTileInteraction = false;
            GameplayController.Instance.gameplayUIController.enablePowerupInteraction = false;

            switch (powerUpPhase)
            {
                case GameplayUIController.PowerUpPhase.None:
                    {
                        Debug.LogAssertion("NORMAL PHASE");
                        foreach (var tile in GameplayController.Instance.connectController.currentMatch)
                        {
                            connectedTiles.Add(tile);
                        }

                        if (GameplayController.Instance.connectController.currentMatch.Count > 1)
                        {
                            Debug.Log("More than 1 dot connected");
                            GameplayController.Instance.level.DecreaseMoveCount();
                            if (GameplayController.Instance.connectController.isMakeASquare)
                            {
                                Debug.Log("Make a square");
                                connectedTiles.RemoveAt(connectedTiles.Count - 1);
                                DeleteDotInTiles(connectedTiles, DESTROY_EFFECT_TYPE.BurstCircleParticleEffect);
                                DeleteDotInTiles(waitDestroyPosition, DESTROY_EFFECT_TYPE.BurstCircleParticleEffect);
                                hitTilesByMatch.AddRange(connectedTiles);
                                hitTilesByMatch.AddRange(waitDestroyPosition);
                                hitTilesByMatch = hitTilesByMatch.Distinct().ToList();
                            }
                            else
                            {
                                Debug.Log("Not make a square");
                                DeleteDotInTiles(connectedTiles, DESTROY_EFFECT_TYPE.BurstCircleParticleEffect);
                                hitTilesByMatch.AddRange(connectedTiles);
                                hitTilesByMatch = hitTilesByMatch.Distinct().ToList();
                            }

                            yield return new WaitUntil(() => isTileDeletionCompleted == true);
                        }
                        else
                        {
                            Debug.Log("Less than 2 dot connected, cancellationTokenSource Cancel()");
                            yield return new WaitForEndOfFrame();
                            ResetVariables();
                            Debug.Log(">> End UniTask OnConnectEnds(), enable tileInteraction.");
                            GameplayController.Instance.connectController.onConnectEndFinish = true;
                            yield break;
                        }
                        Debug.Log("Complete delete connected tiles phase");
                        break;
                    }
                case GameplayUIController.PowerUpPhase.Erase:
                    {
                        Debug.LogAssertion("ERASE PHASE");
                        DeleteDotInTiles(waitDestroyPosition, DESTROY_EFFECT_TYPE.BurstCircleParticleEffect, true);
                        yield return new WaitUntil(() => isTileDeletionCompleted == true);
                        break;
                    }
                case GameplayUIController.PowerUpPhase.SameColor:
                    {
                        Debug.LogAssertion("SAME COLOR PHASE");
                        DeleteDotInTiles(waitDestroyPosition, DESTROY_EFFECT_TYPE.BurstCircleParticleEffect, true);
                        yield return new WaitUntil(() => isTileDeletionCompleted == true);
                        break;
                    }
            }

            yield return new WaitForEndOfFrame();
            AllBoardOccupantsTriggerOnPostMatch();
            while (CheckBoardHasEmptyPositions())
            {
                RefillBoardRealEmptyPositions();
                yield return new WaitUntil(() => isTileMoveCompleted == true);

                if (CheckBoardHasOnlyTemporaryEmptyCell())
                {
                    break;
                }
            }

            yield return new WaitForEndOfFrame();
            waitDestroyPosition.Clear();
            AllDotsTriggerOnPostDotFallAnimation();
            int loopCount = 1;
            while (waitDestroyPosition.Count > 0)
            {
                Debug.Log("Wait destroy pos Count:" + waitDestroyPosition.Count);
                if (waitDestroyPosition.Count > 0)
                {
                    DeleteDotInTiles(waitDestroyPosition, DESTROY_EFFECT_TYPE.BurstCircleParticleEffect);
                    yield return new WaitUntil(() => isTileDeletionCompleted == true);
                }
                waitDestroyPosition.Clear();

                if (CheckBoardHasEmptyPositions())
                {
                    RefillBoardRealEmptyPositions();
                    yield return new WaitUntil(() => isTileMoveCompleted == true);
                }

                yield return new WaitUntil(() => CheckBoardHasOnlyTemporaryEmptyCell() == true);

                AllDotsTriggerOnPostDotFallAnimation();

                loopCount++;
            }

            CheckLevelState();

            GameplayController.Instance.connectController.touchEndPhase = false;
            GameplayController.Instance.connectController.onConnectEndFinish = true;
            yield return new WaitForEndOfFrame();
            ResetVariables();
            Debug.Log(">> End UniTask OnConnectEnds(), enable tileInteraction.");
        }

        private void AllDotsTriggerOnPostDotFallAnimation()
        {
            Debug.Log("AllBoardOccupantsTriggerOnPostFall() begins excecuting ...");
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameDots[i, j] != null)
                    {
                        if (GetDot(i, j).dotType == DOT_TYPE.Wool)
                        {
                            GetDot(i, j).OnPostDotFallAnimation();
                        }
                    }
                }
            }
        }

        private void AllBoardOccupantsTriggerOnPostMatch()
        {
            Debug.Log("AllBoardOccupantsTriggerOnPostMatch() begins excecuting ...");
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameTiles[i, j] != null)
                    {
                        Debug.Log(" Tile type at " + new Vector2Int(i, j) + "=" + GetTile(i, j).type);
                        if (GetTile(i, j).type == TILE_TYPE.Ice && hitTilesByMatch.Contains(new Vector2Int(i, j)))
                        {
                            Debug.Log("Ice here " + i + "," + j);
                            GetTile(i, j).transform.GetComponent<IceTile>().OnPostMatch();
                        }
                        else if (GetTile(i, j).type == TILE_TYPE.Block)
                        {
                            foreach (var hitTile in hitTilesByMatch)
                            {
                                if (IsThisCellAdjacentToThisCell(hitTile, new Vector2Int(i, j)))
                                {
                                    GetTile(i, j).transform.GetComponent<BlockTile>().OnPostMatch();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ResetVariables()
        {
            connectedTiles.Clear();
            waitDestroyPosition.Clear();
            hitTilesByMatch.Clear();

            disableClickPanel.SetActive(false);
            enableTileInteraction = true;
            GameplayController.Instance.gameplayUIController.enablePowerupInteraction = true;
            GameplayController.Instance.connectController.inProcessPhase = false;
        }

        public void DeleteDotInTiles(List<Vector2Int> tiles, DESTROY_EFFECT_TYPE destroyEffectType = DESTROY_EFFECT_TYPE.BurstCircleParticleEffect, bool IsFromPowerup = false)
        {
            isTileDeletionCompleted = false;
            List<Vector2Int> tempTiles = new List<Vector2Int>();
            foreach (var item in tiles)
            {
                tempTiles.Add(item);
            }
            Debug.Log("deleted occupant deleted = " + tempTiles.Count);

            for (int i = 0; i < tempTiles.Count; i++)
            {
                int index = i;
                int tileCount = tempTiles.Count;
                Debug.Log(tempTiles[index]);
                Dot dot = GetDot(tempTiles[index].x, tempTiles[index].y);
                DOT_TYPE dotType = dot.dotType;
                float dotAnchorY = dot.GetComponent<RectTransform>().anchoredPosition.y;
                Debug.LogAssertion(index + " " + dotAnchorY);
                if (dot != null)
                {
                    Debug.Log("begin delete tile's dot at " + tempTiles[index] + ", temp-tiles-count:" + tempTiles.Count);
                    /*
                    dot.transform.SetParent(mainCanvas);
                    Vector3 dotJumpEndPoint = (tempTiles[index].y <= _width / 2) ?
                    new Vector2(-Screen.width / 2f - 65f, dot.GetComponent<RectTransform>().anchoredPosition.y)
                    : new Vector2(Screen.width / 2f + 65f, dot.GetComponent<RectTransform>().anchoredPosition.y);
                    Debug.Log("end-jump-point:" + dotJumpEndPoint);
                    GameplayController.Instance.level.levelTarget.UpdateLevelTargetFields(Utility.FromDotTypeToLevelTargetType(dotType), 1);
                    if (Utility.IsThisDotTypeCatType(dotType))
                    {
                        dot.transform.GetComponent<AnyColorDot>().PlayAnimationJump();
                    }
                    if (index >= tempTiles.Count - 1)
                    {
                        isTileDeletionCompleted = true;
                    }
                    dot.GetComponent<RectTransform>().DOJumpAnchorPos(dotJumpEndPoint, 50f, 1, 1f, false).OnComplete(() =>
                    {
                        if (index >= tempTiles.Count - 1)
                            {
                                isTileDeletionCompleted = true;
                            }
                        Debug.Log("complete delete dot");
                        Destroy(dot.gameObject);
                    });
                    */

                    //dot.transform.SetParent(mainCanvas);
                    dot.transform.SetAsFirstSibling();
                    GameplayController.Instance.level.levelTarget.UpdateLevelTargetFields(Utility.FromDotTypeToLevelTargetType(dotType), 1);
                    if (Utility.IsThisDotTypeCatType(dotType))
                    {
                        dot.transform.GetComponent<AnyColorDot>().PlayAnimationJumpingOut();
                    }
                    if (IsFromPowerup)
                    {
                        if (CheckCellIsIceType(tempTiles[index]))
                        {
                            IceTile ice = GetTile(tempTiles[index]).GetComponent<IceTile>();
                            if (ice != null)
                            {
                                ice.OnPostMatch();
                            }
                        }
                        if(dot.dotType == DOT_TYPE.Wool)
                        {
                            currentWoolDot -= 1;
                        }
                    }
                    dot.dotRectTransform.DOKill();
                    dot.dotRectTransform.DOScale(1.2f, .35f).OnComplete(() =>
                    {
                        var burstCircleParticleEffect = Instantiate(GameAssets.Instance.burstCircleParticleEffect, cellWorldPositions[tempTiles[index].x, tempTiles[index].y], Quaternion.identity);
                        burstCircleParticleEffect.SetAsFirstSibling();
                        ParticleSystem.MainModule main = burstCircleParticleEffect.GetComponent<ParticleSystem>().main;
                        main.startColor = Utility.GetColorFromDotType(dotType);
                        dot.dotRectTransform.DOScale(0f, .35f).OnComplete(() =>
                        {
                            if (index >= tempTiles.Count - 1)
                            {
                                GameController.Instance.musicManager.PlayCatPopSound();
                                isTileDeletionCompleted = true;
                            }
                            Debug.Log("complete delete dot");
                            gameDots[tempTiles[index].x, tempTiles[index].y] = null;
                            SimplePool.Despawn(dot.gameObject);
                            //Destroy(dot.gameObject);
                        });
                    });
                }
            }
        }

        public void RefillBoardRealEmptyPositions()
        {
            Debug.Log("Start refill all null tiles of board size[hw] " + new Vector2Int(_height, _width));
            isTileMoveCompleted = false;
            int totalRealEmpties = 0;

            for (int j = 1; j <= _width; j++)
            {
                List<Vector2Int> emptyTiles = new List<Vector2Int>();
                for (int i = _height; i >= 1; i--)
                {
                    Debug.Log("xet tai " + new Vector2Int(i, j));
                    if (IsPositionAbleToBeRefilled(i, j))
                    {
                        Debug.Log("Empty tiles " + new Vector2Int(i, j));
                        emptyTiles.Add(new Vector2Int(i, j));
                    }
                }
                if (emptyTiles.Count > 0)
                {
                    Debug.Log("Shift down col " + j);
                    Debug.LogError("Empty tiles: " + Utility.AListToString<Vector2Int>(emptyTiles));
                    totalRealEmpties += emptyTiles.Count;


                    Vector2Int lowestEmptyPosition = emptyTiles[0];
                    List<Vector2Int> availableSlots = new List<Vector2Int>();
                    List<Vector2Int> upperDots = new List<Vector2Int>();
                    List<Tuple<Vector2Int, Vector2Int>> preparedPairs = new List<Tuple<Vector2Int, Vector2Int>>();

                    bool hasBlock = false;

                    Debug.LogError("LowestEmptyPos: " + lowestEmptyPosition);
                    for (int i = _height; i >= 1; i--)
                    {
                        if (CheckPositionHasDotInIce(new Vector2Int(i, j)))
                        {
                            Debug.LogError("shift-down available detect ice in tile at " + new Vector2Int(i, j));
                            continue;
                        }
                        if (CheckCellIsGapTile(new Vector2Int(i, j)))
                        {
                            Debug.LogError("shift-down available detect gap til!e at " + new Vector2Int(i, j));
                            continue;
                        }
                        if (CheckCellIsBlockType(new Vector2Int(i, j)) && i < lowestEmptyPosition.x)
                        {
                            Debug.LogError("shift-down available detect block tile at " + new Vector2Int(i, j));
                            hasBlock = true;
                            break;
                        }
                        if (i <= lowestEmptyPosition.x)
                        {
                            availableSlots.Add(new Vector2Int(i, j));
                        }
                    }
                    Debug.LogError("Has block upper: " + hasBlock);
                    Debug.LogError("Available slots: " + Utility.AListToString<Vector2Int>(availableSlots));

                    for (int i = _height; i >= 1; i--)
                    {
                        if (CheckPositionHasDotInIce(new Vector2Int(i, j)))
                        {
                            Debug.LogError("shift-down upper detect ice in tile at " + new Vector2Int(i, j));
                            continue;
                        }
                        if (CheckCellIsGapTile(new Vector2Int(i, j)))
                        {
                            Debug.LogError("shift-down upper detect gap tile at " + new Vector2Int(i, j));
                            continue;
                        }
                        if (CheckCellIsBlockType(new Vector2Int(i, j)) && i < lowestEmptyPosition.x)
                        {
                            Debug.LogError("shift-down upper detect block tile at " + new Vector2Int(i, j));
                            break;
                        }
                        if (i <= lowestEmptyPosition.x && !IsPositionEmpty(i, j))
                        {
                            upperDots.Add(new Vector2Int(i, j));
                        }
                    }
                    Debug.LogError("UpperDots: " + Utility.AListToString<Vector2Int>(upperDots));

                    /*if((availableSlots.Count == 0 || upperDots.Count == 0) && j != _width)
                    {
                        continue;
                    }*/

                    int index = 0;
                    foreach (var tile in availableSlots)
                    {
                        if (index < upperDots.Count)
                        {
                            preparedPairs.Add(new Tuple<Vector2Int, Vector2Int>(upperDots[index], tile));
                            index++;
                        }
                        else if (index >= upperDots.Count && !hasBlock)
                        {
                            preparedPairs.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(-999, -999), tile));
                        }
                    }

                    Debug.LogError("PreparedPairs: " + Utility.AListToString<Tuple<Vector2Int, Vector2Int>>(preparedPairs));

                    ShiftDownPreparedPairsPosition(preparedPairs);
                }
            }

            Debug.Log("total-real-empties:" + totalRealEmpties);
            if (totalRealEmpties == 0)
            {
                isTileMoveCompleted = true;
            }
        }

        private void ShiftDownPreparedPairsPosition(List<Tuple<Vector2Int, Vector2Int>> preparedPairs)
        {
            //DEBUG
            string preparePairsString = "";
            foreach (var pair in preparedPairs)
            {
                preparePairsString += pair.Item1 + " to " + pair.Item2 + "\n";
            }
            Debug.Log(preparePairsString);
            //
            for (int i = 0; i < preparedPairs.Count; i++)
            {
                int index = i;
                int totalCount = preparedPairs.Count;

                if (preparedPairs[index].Item1 != new Vector2Int(-999, -999))
                {
                    Dot dot = GetDot(preparedPairs[index].Item1.x, preparedPairs[index].Item1.y);
                    Debug.Log("pp: " + preparedPairs[index]);
                    dot.Row = preparedPairs[index].Item2.x;
                    dot.Column = preparedPairs[index].Item2.y;
                    dot.transform.SetParent(mainCanvas);
                    Debug.Log(dot.transform.parent);
                    /*dot.GetComponent<RectTransform>().DOMove(cellHolder[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y].transform.position, .3f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        Debug.Log("oncomplete: " + preparedPairs[index]);
                        dot.transform.SetParent(cellHolder[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y].transform);
                        if (index >= totalCount - 1)
                        {
                            GetDot(preparedPairs[index].Item2.x, preparedPairs[index].Item2.y).GetComponent<RectTransform>().DOKill();
                            isTileMoveCompleted = true;
                        }
                    });*/
                    dot.transform.DOKill();
                    gameDots[preparedPairs[index].Item1.x, preparedPairs[index].Item1.y] = null;
                    dot.transform.DOMove(cellWorldPositions[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y], .36f).SetEase(catFallEaseType).OnComplete(() =>
                    {
                        Debug.Log("oncomplete: " + preparedPairs[index]);
                        dot.transform.SetParent(cellHolder[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y].transform);
                        dot.transform.SetAsFirstSibling();
                        gameDots[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y] = dot;
                        if (index >= totalCount - 1)
                        {
                            GetDot(preparedPairs[index].Item2.x, preparedPairs[index].Item2.y).GetComponent<RectTransform>().DOKill();
                            isTileMoveCompleted = true;
                        }
                    });
                }
                else
                {
                    Debug.Log("is level has wool:" + GameplayController.Instance.level.levelTarget.IsLevelHasWoolTarget());
                    Debug.Log("maximum wool target" + GameplayController.Instance.level.levelTarget.GetMaximumWoolTarget());
                    newDotsCount++;
                    int maxWoolAllowInBoard = GameplayController.Instance.level.levelTarget.GetMaximumWoolTarget() == GameplayController.Instance.level.levelTarget.GetCurrentWoolTarget() ? 1 : GameplayController.Instance.level.levelTarget.GetMaximumWoolTarget() - GameplayController.Instance.level.levelTarget.GetCurrentWoolTarget() + 1;

                    if (newDotsCount >= 5
                        && GameplayController.Instance.level.levelTarget.IsLevelHasWoolTarget()
                        && currentWoolDot < maxWoolAllowInBoard)
                    {
                        Transform dotTrans = Instantiate(GameAssets.Instance.woolDotPrefab, cellWorldPositions[1, preparedPairs[index].Item2.y] + new Vector3(0, 1, 0), Quaternion.identity, mainCanvas);
                        gameDots[1, preparedPairs[index].Item2.y] = dotTrans.GetComponent<Dot>();
                        Dot dot = dotTrans.GetComponent<Dot>();
                        dot.dotType = DOT_TYPE.Wool;
                        dot.SetupOnCreate(preparedPairs[index].Item2.x, preparedPairs[index].Item2.y);
                        dot.transform.DOKill();
                        dot.transform.DOMove(cellWorldPositions[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y], .36f).SetEase(catFallEaseType).OnComplete(() =>
                        {
                            dot.transform.SetParent(cellHolder[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y].transform);
                            dot.transform.SetAsFirstSibling();
                            gameDots[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y] = dot;
                            if (index >= totalCount - 1)
                            {
                                GetDot(preparedPairs[index].Item2.x, preparedPairs[index].Item2.y).GetComponent<RectTransform>().DOKill();
                                isTileMoveCompleted = true;
                            }
                        });
                        newDotsCount = 0;
                    }
                    else
                    {
                        Transform dotTrans = Instantiate(GameAssets.Instance.anyColorDot, cellWorldPositions[1, preparedPairs[index].Item2.y] + new Vector3(0, 1, 0), Quaternion.identity, mainCanvas);
                        gameDots[1, preparedPairs[index].Item2.y] = dotTrans.GetComponent<Dot>();
                        Dot dot = dotTrans.GetComponent<Dot>();
                        dot.dotType = Utility.FromIntToDotType(choosenCats[UnityEngine.Random.Range(0, choosenCats.Count)]);
                        dot.SetupOnCreate(preparedPairs[index].Item2.x, preparedPairs[index].Item2.y);
                        dot.transform.DOKill();
                        dot.transform.DOMove(cellWorldPositions[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y], .36f).SetEase(catFallEaseType).OnComplete(() =>
                        {
                            dot.transform.SetParent(cellHolder[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y].transform);
                            dot.transform.SetAsFirstSibling();
                            gameDots[preparedPairs[index].Item2.x, preparedPairs[index].Item2.y] = dot;
                            if (index >= totalCount - 1)
                            {
                                GetDot(preparedPairs[index].Item2.x, preparedPairs[index].Item2.y).GetComponent<RectTransform>().DOKill();
                                isTileMoveCompleted = true;
                            }
                        });
                    }
                }
            }
        }

        public void CreateEffectOnTiles(List<Vector2Int> tiles, DESTROY_EFFECT_TYPE effect)
        {
            foreach (var tile in tiles)
            {
                switch (effect)
                {
                    case DESTROY_EFFECT_TYPE.CircleScaleEffect:
                        {
                            cellEffectHolders[tile.x, tile.y].SetAsFirstSibling();
                            circleDotEffects[tile.x, tile.y].StartAnimOnce(GetDot(tile.x, tile.y).dotType);
                            break;
                        }
                    case DESTROY_EFFECT_TYPE.BurstCircleParticleEffect:
                        {
                            var burstCircleParticleEffect = Instantiate(GameAssets.Instance.burstCircleParticleEffect, cellWorldPositions[tile.x, tile.y], Quaternion.identity);
                            burstCircleParticleEffect.SetAsFirstSibling();
                            burstCircleParticleEffect.GetComponent<ParticleSystem>().startColor = Utility.GetColorFromDotType(GetDot(tile.x, tile.y).dotType);
                            break;
                        }
                    default: break;
                }
            }
        }

        public void MoveDotToOtherCell(Vector2Int p1, Vector2Int p2, Dot[,] previousGameDots)
        {
            Transform dotTransform = GetDot(p1).transform;
            Debug.Log(dotTransform);
            dotTransform.DOMove(cellWorldPositions[p2.x, p2.y], 1f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                Vector2Int temp = p2;
                gameDots[temp.x, temp.y] = previousGameDots[p1.x, p1.y];
                gameDots[temp.x, temp.y].Row = temp.x;
                gameDots[temp.x, temp.y].Column = temp.y;
                dotTransform.SetParent(cellHolder[temp.x, temp.y]);
                dotTransform.SetAsFirstSibling();
                if (!enableTileInteraction)
                {
                    enableTileInteraction = true;
                    GameplayController.Instance.gameplayUIController.enablePowerupInteraction = true;
                }
            });
        }

        public List<Vector2Int> GetListOfSameColorTiles(DOT_TYPE dotType, List<Vector2Int> excludedTiles = null)
        {
            List<Vector2Int> results = new List<Vector2Int>();
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (excludedTiles != null)
                    {
                        if (excludedTiles.Contains(new Vector2Int(i, j)))
                        {
                            Debug.Log("Ignore " + new Vector2Int(i, j));
                            continue;
                        }
                    }
                    if (gameDots[i, j] != null)
                    {
                        if (GetDot(i, j).dotType == dotType)
                        {
                            results.Add(new Vector2Int(i, j));
                        }
                    }
                }
            }
            return results;
        }

        public void ResetDotsScale(Vector2Int pos)
        {
            if (gameDots[pos.x, pos.y] != null)
            {
                Dot dot = GetDot(pos);
                dot.ResetDotScaleToNormal();
                if (Utility.IsThisDotTypeCatType(dot.dotType))
                {
                    //Debug.Log(GetComponent<AnyColorDot>());
                    dot.transform.GetComponent<AnyColorDot>().PlayAnimationIdle();
                }
            }
        }

        public void ResetAllDotsScale(List<Vector2Int> list)
        {
            foreach (var item in list)
            {
                if (gameDots[item.x, item.y] != null)
                {
                    Dot dot = GetDot(item);
                    dot.ResetDotScaleToNormal();
                    if (Utility.IsThisDotTypeCatType(dot.dotType))
                    {
                        dot.transform.GetComponent<AnyColorDot>().PlayAnimationIdle();
                    }
                }
            }
        }

        [HideInInspector] public CircleDotEffect[,] circleDotEffects;
        [HideInInspector] public Transform[,] cellEffectHolders;

        public void InitCircleDotsEffectOnBoard()
        {
            circleDotEffects = new CircleDotEffect[_height + 1, _width + 1];
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    var o = Instantiate(GameAssets.Instance.circleDotPrefab, cellEffectHolders[i, j]);
                    circleDotEffects[i, j] = o.GetComponent<CircleDotEffect>();
                    circleDotEffects[i, j].Stop();
                }
            }
        }
        public void StartAllDotsCircleEffects()
        {
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameTiles[i, j] != null)
                    {
                        if (gameTiles[i, j].type == TILE_TYPE.Gap ||
                        gameTiles[i, j].type == TILE_TYPE.Block ||
                        gameTiles[i, j].type == TILE_TYPE.Ice)
                        {
                            continue;
                        }
                    }
                    if (gameDots[i, j] != null)
                    {
                        if (gameDots[i, j].dotType == DOT_TYPE.Wool)
                        {
                            continue;
                        }
                        cellEffectHolders[i, j].SetAsFirstSibling();
                        circleDotEffects[i, j].StartAnim(gameDots[i, j].dotType);
                    }
                }
            }
        }

        public void StopAllDotsCircleEffects()
        {
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameDots[i, j] != null)
                    {
                        circleDotEffects[i, j].Stop();
                    }
                }
            }
        }

        #region Checker methods

        public bool IsPositionEmpty(int row, int column)
        {
            if (CheckCellIsGapTile(new Vector2Int(row, column)))
            {
                return false;
            }
            if (CheckCellIsBlockType(new Vector2Int(row, column)))
            {
                return false;
            }
            return gameDots[row, column] == null;
        }

        public bool IsPositionAbleToBeRefilled(int row, int column)
        {
            if (IsPositionEmpty(row, column))
            {
                if (row == 1)
                {
                    return true;
                }
                for (int i = row - 1; i >= 1; i--)
                {
                    if (i == 1 && (CheckCellIsGapTile(new Vector2Int(i, column))))
                    {
                        return true;
                    }
                    if (CheckCellIsBlockType(new Vector2Int(i, column)))
                    {
                        return false;
                    }
                    if (IsPositionEmpty(i, column))
                    {
                        continue;
                    }
                    if (CheckCellIsGapTile(new Vector2Int(i, column)) || CheckPositionHasDotInIce(new Vector2Int(i, column)))
                    {
                        continue;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsPositionNotContainAnyDots(int row, int column)
        {
            return gameDots[row, column] == null;
        }

        public bool CheckBoardHasEmptyPositions()
        {
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (IsPositionEmpty(i, j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckThisPositionIsTheMostBelow(int row, int col)
        {
            //Debug.LogError("Most below in col " + col + " is " + GetTheMostBelowOrderInColumn(col));

            if (!IsValidPosition(row, col))
            {
                //Debug.LogError("This position is invalid");
                return false;
            }
            if (row == GetTheMostBelowOrderInColumn(col))
            {
                return true;
            }
            return false;
        }

        public int GetTheMostBelowOrderInColumn(int col)
        {
            int res = Height;
            for (int i = Height; i >= 1; i--)
            {
                res = i;
                if (!IsValidPosition(i, col))
                {
                    break;
                }
                if (gameTiles[i, col] != null)
                {
                    if (GetTile(i, col).type == TILE_TYPE.Gap)
                    {
                        continue;
                    }
                }
                else
                {
                    break;
                }
            }
            return res;
        }

        public bool IsValidPosition(int row, int col)
        {
            if (row < 1 || col < 1 || row > Height || col > Width)
            {
                return false;
            }
            return true;
        }
        public bool IsValidPosition(Vector2Int position)
        {
            if (position.x < 1 || position.y < 1 || position.x > Height || position.y > Width)
            {
                return false;
            }
            return true;
        }


        public bool IsThisCellCanSpreadWater(int row, int col)
        {
            return true;
        }

        public bool IsThisCellAdjacentToThisCell(Vector2Int p1, Vector2Int p2)
        {
            if (!IsValidPosition(p1) || !IsValidPosition(p2))
            {
                Debug.Log("This cell out of board !");
                return false;
            }

            return (p1.x == p2.x && Mathf.Abs(p1.y - p2.y) == 1) || (p1.y == p2.y && Mathf.Abs(p1.x - p2.x) == 1);
        }

        public bool CheckPositionHasDotInIce(Vector2Int position)
        {
            if (gameDots[position.x, position.y] == null)
            {
                return false;
            }
            if (gameTiles[position.x, position.y] != null)
            {
                if (gameTiles[position.x, position.y].type == TILE_TYPE.Ice)
                {
                    if (!gameTiles[position.x, position.y].GetComponent<IceTile>().isRemoved)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckCellIsGapTile(Vector2Int position)
        {
            if (gameTiles[position.x, position.y] != null)
            {
                if (gameTiles[position.x, position.y].type == TILE_TYPE.Gap)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckCellIsBlockType(Vector2Int position)
        {
            if (gameTiles[position.x, position.y] != null)
            {
                //Debug.Log(GetTile(position).transform.name);
                if (gameTiles[position.x, position.y].type == TILE_TYPE.Block)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckCellIsIceType(Vector2Int position)
        {
            if (gameTiles[position.x, position.y] != null)
            {
                //Debug.Log(GetTile(position).transform.name);
                if (gameTiles[position.x, position.y].type == TILE_TYPE.Ice)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckBoardHasOnlyTemporaryEmptyCell()
        {
            int realEmpty = 0;
            int tempEmpty = 0;

            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (IsPositionEmpty(i, j))
                    {
                        Debug.Log("... Checking empty at " + new Vector2Int(i, j));
                        bool check = false;
                        for (int r = i - 1; r >= 1; r--)
                        {
                            if (IsPositionEmpty(r, j) || CheckCellIsGapTile(new Vector2Int(r, j)) || CheckPositionHasDotInIce(new Vector2Int(r, j)))
                            {
                                Debug.Log(">next");
                                continue;
                            }
                            if (CheckCellIsBlockType(new Vector2Int(r, j)))
                            {
                                Debug.Log(">check = true");
                                check = true;
                            }
                            else
                            {
                                Debug.Log(">check = false");
                                check = false;
                            }
                            Debug.Log(">break loop");
                            break;
                        }
                        Debug.Log("check[]" + check);
                        if (check)
                        {
                            tempEmpty++;
                        }
                        else
                        {
                            realEmpty++;
                        }
                    }
                }
            }
            Debug.Log("Real empty: " + realEmpty + ", Temp empty: " + tempEmpty);

            if (realEmpty > 0)
            {
                Debug.Log("Board still has " + realEmpty + " real empty");
                return false;
            }

            Debug.Log("Board has " + tempEmpty + " temp empty");
            return true;
        }

        public bool CheckCellIsAbleToRandomize(int row, int col)
        {
            if (gameTiles[row, col] != null)
            {
                if (gameTiles[row, col].type == TILE_TYPE.Gap ||
                gameTiles[row, col].type == TILE_TYPE.Block ||
                gameTiles[row, col].type == TILE_TYPE.Ice)
                {
                    return false;
                }
            }
            if (gameDots[row, col] != null)
            {
                switch (GetDot(row, col).dotType)
                {
                    case DOT_TYPE.RedCat:
                    case DOT_TYPE.YellowCat:
                    case DOT_TYPE.WhiteCat:
                    case DOT_TYPE.GreenCat:
                    case DOT_TYPE.PinkCat:
                    case DOT_TYPE.PurpleCat: return true;
                    default: return false;
                }
            }

            return false;
        }

        private int GetCurrentWoolDotInBoard()
        {
            int count = 0;
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameDots[i, j] != null)
                    {
                        if (GetDot(i, j).dotType == DOT_TYPE.Wool)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        #endregion

        #region Apply destructions

        public void RemoveBoardOccupantGameObject(BoardOccupant boardOccupant)
        {
            DestroyImmediate(boardOccupant.gameObject);
        }

        public void RemoveDotFromBoard(Dot dot)
        {
            dot.dotRectTransform.DOScale(0f, .3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                GameplayController.Instance.level.levelTarget.UpdateLevelTargetFields(Utility.FromDotTypeToLevelTargetType(dot.dotType), 1);
                Destroy(dot.gameObject);
            });
        }

        #endregion

        #region Powerups methods

        public void RandomizeBoard()
        {
            enableTileInteraction = false;
            GameplayController.Instance.gameplayUIController.enablePowerupInteraction = false;
            List<Tuple<Vector2Int, Vector2Int>> positionPairs = new List<Tuple<Vector2Int, Vector2Int>>();
            List<Vector2Int> firstPositions = new List<Vector2Int>();
            List<Vector2Int> secondsPositions = new List<Vector2Int>();

            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (CheckCellIsAbleToRandomize(i, j))
                    {
                        firstPositions.Add(new Vector2Int(i, j));
                        secondsPositions.Add(new Vector2Int(i, j));
                    }
                }
            }

            Utility.Shuffle(secondsPositions, UnityEngine.Random.Range(0, 1000));

            int r = 0;
            foreach (var i in firstPositions)
            {
                positionPairs.Add(new Tuple<Vector2Int, Vector2Int>(i, secondsPositions[r]));
                r++;
            }

            Debug.Log("move-cell-count:" + positionPairs.Count);
            Dot[,] previousGameDots = new Dot[_height + 1, _width + 1];
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    previousGameDots[i, j] = gameDots[i, j];
                }
            }
            foreach (var i in positionPairs)
            {
                Debug.Log(i.Item1 + " to " + i.Item2);
                MoveDotToOtherCell(i.Item1, i.Item2, previousGameDots);
            }
        }

        public void Erase(int row, int col)
        {
            DOKillAllDots();
            waitDestroyPosition.Add(new Vector2Int(row, col));
            OnConnectEnd(GameplayUIController.PowerUpPhase.Erase);
        }

        public void DeleteSameColorDots(DOT_TYPE dotType)
        {
            GameplayController.Instance.level.board.waitDestroyPosition.Clear();
            List<Vector2Int> sameColorTiles = GameplayController.Instance.level.board.GetListOfSameColorTiles(
                dotType,
                GameplayController.Instance.connectController.GetDistinctCurrentMatch());
            Debug.LogError("here2 + " + Utility.AListToString<Vector2Int>(sameColorTiles));
            GameplayController.Instance.level.board.CreateEffectOnTiles(sameColorTiles, DESTROY_EFFECT_TYPE.CircleScaleEffect);
            GameplayController.Instance.level.board.waitDestroyPosition.AddRange(sameColorTiles);
            OnConnectEnd(GameplayUIController.PowerUpPhase.SameColor);
        }

        public void CrossExplode()
        {

        }

        public void ShakeAllDotsOnBoard()
        {
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameDots[i, j] != null)
                    {
                        //GetDot(i, j).transform.DOShakePosition()
                        StartCoroutine(ShakeCoroutine(GetDot(i, j).transform));
                    }
                }
            }
        }

        public void DOKillAllDots()
        {
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    if (gameDots[i, j] != null)
                    {
                        GetDot(i, j).transform.DOKill();
                    }
                }
            }
        }

        private IEnumerator ShakeCoroutine(Transform t)
        {
            Debug.Log("shake " + t.name);
            while (true)
            {
                Debug.Log("shake");
                t.DOShakePosition(0.4f, 3, 25, 100, false, false).SetEase(Ease.Linear);
                yield return new WaitForSeconds(.8f);
            }
        }

        #endregion

        #region Getters/Setters

        public Dot GetDot(int row, int col)
        {
            return gameDots[row, col];
        }

        public Dot GetDot(Vector2Int position)
        {
            return gameDots[position.x, position.y];
        }

        public Tile GetTile(int row, int col)
        {
            return gameTiles[row, col];
        }

        public Tile GetTile(Vector2Int position)
        {
            return gameTiles[position.x, position.y];
        }

        #endregion

        #region State checking

        private float durationForEndGamePopupShow = 1f;

        public bool isLevelEnd = false;
        private bool firstLose = true;

        public void CheckLevelState()
        {
            if (isLevelEnd) return;
            //Win
            if (GameplayController.Instance.level.levelTarget.CheckLevelTargetFulfilled())
            {
                isLevelEnd = true;
                enableTileInteraction = false;
                enablePowerupsInteraction = false;
                GameController.Instance.AnalyticsController.LogLevelComplet(UseProfile.CurrentLevel);
                UseProfile.CurrentLevel += 1;
                StartCoroutine(WinCoroutine(durationForEndGamePopupShow));
                return;
            }
            //Lose
            if (GameplayController.Instance.level.moveCount <= 0)
            {
                isLevelEnd = true;
                enableTileInteraction = false;
                enablePowerupsInteraction = false;
                GameController.Instance.AnalyticsController.LogLevelFail(UseProfile.CurrentLevel);
                StartCoroutine(LoseCoroutine(durationForEndGamePopupShow));
            }
        }

        public IEnumerator WinCoroutine(float duration)
        {
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration * 2f / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            yield return new WaitForSeconds(duration *2f / 10f);
            Instantiate(GameAssets.Instance.confetties[UnityEngine.Random.Range(0, GameAssets.Instance.confetties.Count)], new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-2.5f, 3f), 0), Quaternion.identity);
            GameController.Instance.musicManager.LerpMusicSourceVolume(0f, 1f);
            if (GameController.Instance.IsShowRate())
            {
                RatePopup.Setup().Show();
            }
            else
            {
                WinPopup.Setup().Show();
            }
        }

        public IEnumerator LoseCoroutine(float duration)
        {
            yield return new WaitForSeconds(.6f);
            GameController.Instance.musicManager.LerpMusicSourceVolume(0f, 1f);
            if (firstLose)
            {
                LosePopup.Setup().Show();
                firstLose = false;
            }
            else
            {
                LosePopup.Setup(false, null, false).Show();
            }
            
        }

        #endregion

        #region Events

        public event Action OnBoardCreated;
        public event Action<IList<int>> OnDotConnected;
        public event Action<IList<int>> OnDotDisconnected;
        public event Action<IList<int>> OnDotMatch;
        public event Action<int> OnDotPress;
        public event Action<IList<int>> OnDotRelease;
        public event Action OnExplosion;
        public event Action OnHighlightSquare;

        #endregion

        #region Utility methods

        private int ToIndex(Vector2Int position)
        {
            int index = 1;

            return index;
        }

        private void ReorderTransform(Transform parent)
        {
            List<Transform> childs = new List<Transform>();
            foreach (Transform c in parent)
            {
                childs.Add(c);
            }
            foreach (var child in childs)
            {

            }
        }

        #endregion

        #region Debugs

        public void DebugBoard()
        {
            string boardStr = "";
            for (int i = 1; i <= _height; i++)
            {
                for (int j = 1; j <= _width; j++)
                {
                    boardStr += (int)GetDot(i, j).dotType + " ";
                }
                boardStr += "\n";
            }
            Debug.Log(boardStr);
        }

        #endregion
    }
}
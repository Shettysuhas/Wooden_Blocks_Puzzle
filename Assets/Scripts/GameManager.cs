using System.Linq; 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Level _level;
    [SerializeField] private BGCell _bgCellPrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private float _blockSpawnSize;
    [SerializeField] private float _blockHighLightSize;
    [SerializeField] private float _blockPutSize;
    [SerializeField] private List<Transform> blockSpawnPoints;
    [SerializeField] private InGameUiHandler _inGameUiHandler;
    private BGCell[,] bgCellGrid;
    private bool hasGameFinished;
    private Block currentBlock;
    private Vector2 currentPos, previousPos;
    private List<Block> gridBlocks;
    private bool initialBlock = true;
    private int blockIndex = 3;
    public static int blockstobeSpawnned;
    private bool isBlockedd;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        gridBlocks = new List<Block>();
        SpawnGrid();
        SpawnBlocks(0);
    }
    private void Update()
    {
        if (blockstobeSpawnned == 3)
        {
            SpawnBlocks(0);
        }
        if (hasGameFinished) return;
        MoveBlock();
    }

    private void SpawnGrid()
    {
        bgCellGrid = new BGCell[_level.Rows, _level.Columns];

        int centerStartRow = (_level.Rows / 2) - 1;
        int centerStartCol = (_level.Columns / 2) - 1;

        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                BGCell bgcell = Instantiate(_bgCellPrefab);
                bgcell.transform.position = new Vector3(j + 0.5f, i + 0.5f, 0f);
                bool isDark = ((i / 3) + (j / 3)) % 2 == 0;

                if (UiHandler._difficultyLevel == 2 &&
                    i >= centerStartRow && i < centerStartRow + 3 &&
                    j >= centerStartCol && j < centerStartCol + 3)
                {
                    isBlockedd = true;
                    bgcell.IsFilled = true;
                    bgcell.IsBlocked = true;
                }
                bgcell.Init(_level.Data[i * _level.Columns + j], isDark, isBlockedd);
                bgCellGrid[i, j] = bgcell;
                isBlockedd = false;
            }
        }
    }

    public void SpawnBlocks(int spawnIndex)
    {
        Vector3 startPos = Vector3.zero;
        startPos.x = 0.25f + (_level.Columns - _level.BlockColumns * _blockSpawnSize) * 0.5f;
        startPos.y = -_level.BlockRows * _blockSpawnSize + 0.25f - 1f;

        int randomNumber = Random.Range(0, _level.Blocks.Count);

        if (initialBlock || ((UiHandler._difficultyLevel == 2 || UiHandler._difficultyLevel == 1) && blockstobeSpawnned == 3))
        {
            blockstobeSpawnned = 0;
            for (int i = 0; i < 3; i++)
            {
                randomNumber = Random.Range(0, _level.Blocks.Count);
                Block block = Instantiate(_blockPrefab);
                Vector2Int blockPos = _level.Blocks[i].StartPos;
                Vector3 blockSpawnPos = startPos
                                        + new Vector3(blockPos.y, blockPos.x, 0) * (_blockSpawnSize);
                blockSpawnPos = blockSpawnPoints[i % 3].position;
                block.transform.position = blockSpawnPos;
                block.Init(_level.Blocks[randomNumber].BlockPositions, blockSpawnPos, _level.Blocks[i].Id, i % 3);
            }
        }
        else if (UiHandler._difficultyLevel == 0)
        {
            Block block = Instantiate(_blockPrefab);
            Vector2Int blockPos = _level.Blocks[blockIndex].StartPos;
            Vector3 blockSpawnPos = startPos
                                    + new Vector3(blockPos.y, blockPos.x, 0) * (_blockSpawnSize);
            blockSpawnPos = blockSpawnPoints[spawnIndex].position;
            block.transform.position = blockSpawnPos;
            block.Init(_level.Blocks[randomNumber].BlockPositions, blockSpawnPos, _level.Blocks[blockIndex].Id, spawnIndex);
        }

        initialBlock = false;

        float maxColumns = Mathf.Max(_level.Columns, _level.BlockColumns * _blockSpawnSize);
        float maxRows = _level.Rows + 2f + _level.BlockRows * _blockSpawnSize;
        Camera.main.orthographicSize = Mathf.Max(maxColumns, maxRows) * 0.95f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Columns * 0.5f;
        camPos.y = (_level.Rows + 0.5f + startPos.y) * 0.5f;
        Camera.main.transform.position = camPos;
    }
    public void MoveBlock()
    {
        Vector2 touchPos = Vector2.zero;
        bool touchBegan = false;
        bool touchMoved = false;
        bool touchEnded = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            touchBegan = touch.phase == TouchPhase.Began;
            touchMoved = touch.phase == TouchPhase.Moved;
            touchEnded = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }
        else
        {
            touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            touchBegan = Input.GetMouseButtonDown(0);
            touchMoved = Input.GetMouseButton(0);
            touchEnded = Input.GetMouseButtonUp(0);
        }

        if (touchBegan)
        {
            RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);
            if (!hit) return;
            currentBlock = hit.collider.transform.parent.GetComponent<Block>();
            if ((currentBlock == null) || (currentBlock.IsPlaced)) return;

            currentPos = touchPos;
            previousPos = touchPos;
            currentBlock.UpdatePos(touchPos);
            currentBlock.ElevateSprites();
            currentBlock.transform.localScale = Vector3.one * _blockHighLightSize;

            if (gridBlocks.Contains(currentBlock))
            {
                gridBlocks.Remove(currentBlock);
            }
            UpdateFilled();
            ResetHighLight();
            UpdateHighLight();
        }
        else if (touchMoved && currentBlock != null)
        {
            if ((currentBlock == null) || (currentBlock.IsPlaced)) return;
            currentBlock.UpdatePos(touchPos);
            previousPos = currentPos;
            ResetHighLight();
            UpdateHighLight();
        }
        else if (touchEnded && currentBlock != null)
        {
            if ((currentBlock == null) || (currentBlock.IsPlaced)) return;
            currentBlock.ElevateSprites(true);

            if (IsCorrectMove())
            {
                currentBlock.UpdateCorrectMove();
                currentBlock.transform.localScale = Vector3.one * _blockPutSize;
                gridBlocks.Add(currentBlock);
                currentBlock.SetPlaced();
            }
            else if (touchPos.y < 0)
            {
                currentBlock.UpdateStartMove();
                currentBlock.transform.localScale = Vector3.one * _blockSpawnSize;
            }
            else
            {
                currentBlock.UpdateIncorrectMove();
                if (currentBlock.CurrentPos.y > 0)
                {
                    gridBlocks.Add(currentBlock);
                    currentBlock.transform.localScale = Vector3.one * _blockPutSize;
                    currentBlock.SetPlaced();
                }
                else
                {
                    currentBlock.transform.localScale = Vector3.one * _blockSpawnSize;
                }
            }

            currentBlock = null;
            ResetHighLight();
            UpdateFilled();
            StartCoroutine(DelayedCheckGameover());
        }
    }

    private void ResetHighLight()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[i, j].IsBlocked)
                {
                    bgCellGrid[i, j].ResetHighLight();
                }
            }
        }
    }

    private void UpdateFilled()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[i, j].IsBlocked)
                {
                    bgCellGrid[i, j].IsFilled = false;
                }
            }
        }

        foreach (var block in gridBlocks)
        {
            foreach (var pos in block.BlockPositions())
            {
                if (IsValidPos(pos))
                {
                    bgCellGrid[pos.x, pos.y].IsFilled = true;
                }
            }
        }

        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();
        List<Vector2Int> getFullCubes = GetFullCubes();

        for (int i = 0; i < _level.Rows; i++)
        {
            bool isRowFull = true;
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[i, j].IsFilled)
                {
                    isRowFull = false;
                    break;
                }
            }

            if (isRowFull) fullRows.Add(i);
        }

        for (int j = 0; j < _level.Columns; j++)
        {
            bool isColFull = true;
            for (int i = 0; i < _level.Rows; i++)
            {
                if (!bgCellGrid[i, j].IsFilled)
                {
                    isColFull = false;
                    break;
                }
            }

            if (isColFull) fullCols.Add(j);
        }

        List<GameObject> cellsToDisable = new List<GameObject>();
        foreach (var block in gridBlocks)
        {
            List<GameObject> blockCells = block.GetCellsToDisable(fullRows, fullCols, getFullCubes);
            if (blockCells != null)
            {
                cellsToDisable.AddRange(blockCells);
            }
        }

        cellsToDisable = cellsToDisable.OrderBy(cell => cell.transform.position.y)
            .ThenBy(cell => cell.transform.position.x)
            .ToList();
        if (cellsToDisable != null)
        {
            StartCoroutine(DisableCellsOneByOne(cellsToDisable, 0.1f, fullRows, fullCols, getFullCubes));
        }

        foreach (int row in fullRows)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!bgCellGrid[row, j].IsBlocked)
                {
                    bgCellGrid[row, j].IsFilled = false;
                }
            }
        }

        foreach (int col in fullCols)
        {
            for (int i = 0; i < _level.Rows; i++)
            {
                if (!bgCellGrid[i, col].IsBlocked)
                {
                    bgCellGrid[i, col].IsFilled = false;
                }
            }
        }
        StartCoroutine(DelayedCheckGameover());
    }
    private IEnumerator DisableCellsOneByOne(List<GameObject> cells, float delay, List<int> fullRows, List<int> fullCols, List<Vector2Int> getFullCubes)
    {
        foreach (var cell in cells)
        {
            if (cell != null && cell.activeSelf)
            {
                cell.SetActive(false);
                ScoreMnager.Instance.AddScore(1);
                yield return new WaitForSeconds(delay);
            }
        }
        if (fullRows.Count > 0 || fullCols.Count > 0 || getFullCubes.Count > 0)
        {
            foreach (var block in gridBlocks)
            {
                block.DisableCellsInLine(fullRows, fullCols);
                block.DisableCellsInCubes(getFullCubes);
            }
        }
    }

    private List<Vector2Int> GetFullCubes()
    {
        List<Vector2Int> fullCubes = new List<Vector2Int>();

        for (int startX = 0; startX < _level.Rows; startX += 3)
        {
            for (int startY = 0; startY < _level.Columns; startY += 3)
            {
                bool isCubeFull = true;

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (!bgCellGrid[startX + i, startY + j].IsFilled)
                        {
                            isCubeFull = false;
                            break;
                        }
                    }
                    if (!isCubeFull) break;
                }

                if (isCubeFull)
                {
                    fullCubes.Add(new Vector2Int(startX, startY));
                }
            }
        }

        return fullCubes;
    }

    private void UpdateHighLight()
    {
        bool isCorrect = IsCorrectMove();
        foreach (var pos in currentBlock.BlockPositions())
        {
            if (IsValidPos(pos))
            {
                bgCellGrid[pos.x, pos.y].UpdateHighlight(isCorrect);
            }
        }
    }
    private bool IsCorrectMove()
    {
        foreach (var pos in currentBlock.BlockPositions())
        {
            if (!IsValidPos(pos) || bgCellGrid[pos.x, pos.y].IsFilled)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        if (UiHandler._difficultyLevel == 2)
        {
            int centerStartRow = (_level.Rows / 2) - 1;
            int centerStartCol = (_level.Columns / 2) - 1;

            if (pos.x >= centerStartRow && pos.x < centerStartRow + 3 &&
                pos.y >= centerStartCol && pos.y < centerStartCol + 3)
            {
                return false;
            }
        }

        return pos.x >= 0 && pos.x < _level.Rows && pos.y >= 0 && pos.y < _level.Columns;
    }

    private IEnumerator DelayedCheckGameover()
    {
        yield return new WaitForSeconds(0.3f);
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        Block[] liveBlocks = FindObjectsOfType<Block>();
        bool canPlaceAnyBlock = false;

        foreach (Block block in liveBlocks)
        {
            if (!block.IsPlaced)
            {
                bool canPlace = CanBlockBePlaced(block);

                if (canPlace)
                {
                    block.UnBlockBlockedBlock();
                    canPlaceAnyBlock = true;
                }
                else
                {
                    block.BlockedBlock();
                }
            }
        }

        if (!canPlaceAnyBlock)
        {
            Debug.LogError(" GAME OVER! No valid moves!");
            StartCoroutine(GameOver());
        }
    }

    private bool CanBlockBePlaced(Block block)
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                foreach (Vector2Int blockPos in block.BlockPositions())
                {
                    Vector2Int gridPos = new Vector2Int(j - blockPos.x, i - blockPos.y);
                    if (CanPlaceBlockAtPosition(block, gridPos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CanPlaceBlockAtPosition(Block block, Vector2Int gridPos)
    {
        int centerStartRow = (_level.Rows / 2) - 1;
        int centerStartCol = (_level.Columns / 2) - 1;

        foreach (Vector2Int pos in block.BlockPositions())
        {
            Vector2Int newPos = new Vector2Int(gridPos.x + pos.x, gridPos.y + pos.y);

            if (!IsValidPos(newPos) || bgCellGrid[newPos.x, newPos.y].IsFilled)
            {
                return false;
            }

            if (newPos.x >= centerStartRow && newPos.x < centerStartRow + 3 &&
                newPos.y >= centerStartCol && newPos.y < centerStartCol + 3)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);
        _inGameUiHandler.GameOver();
    }
}
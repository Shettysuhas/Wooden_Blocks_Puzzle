using System.Collections;
using System.Linq; 
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Utilities;

public class Block : MonoBehaviour
{
    public Vector3 CurrentPos => new Vector3(currentPos.x, currentPos.y, 0);

    [SerializeField] private float _blockSpawnSize;
    [SerializeField] private float yOffset = 0.5f;
    [SerializeField] private List<Sprite> _blockSprites;
    [SerializeField] private SpriteRenderer _blockPrefab;
    [SerializeField] private Sprite matchedSprite;
    [SerializeField] private Sprite _inCorrectSprite;
    [SerializeField] private Sprite _CorrectSprite;


    private Vector3 startPos;
    private Vector3 previousPos;
    private Vector3 currentPos;
    private List<SpriteRenderer> blockSprites;
    private List<Vector2Int> blockPositions;
    private int spawnPositon;

    private const int TOP = 1;
    private const int BOTTOM = 0;

    public bool IsPlaced;


    public void Init(List<Vector2Int> blocks, Vector3 start, int blockNum, int spawnPos)
    {
        startPos = start;
        previousPos = start;
        currentPos = start;
        spawnPositon = spawnPos;
        IsPlaced = false;
        blockPositions = new List<Vector2Int>(blocks);

        blockSprites = new List<SpriteRenderer>();
        for (int i = 0; i < blockPositions.Count; i++)
        {
            SpriteRenderer spawnedBlock = Instantiate(_blockPrefab, transform);
            spawnedBlock.sprite = _blockSprites[blockNum + 1];
            spawnedBlock.transform.localPosition = new Vector3(blockPositions[i].y,
                blockPositions[i].x, 0);
            blockSprites.Add(spawnedBlock);
        }
        transform.localScale = Vector3.one * _blockSpawnSize;
        ElevateSprites(true);
    }

    public void SetPlaced()
    {
        IsPlaced = true;
    }
    public void UpdatePos(Vector3 touchPos)
    {
        currentPos = new Vector3(touchPos.x, touchPos.y + yOffset,0); 
        transform.position = currentPos;
    }
    public void ElevateSprites(bool reverse = false)
    {
        foreach (var blockSprite in blockSprites)
        {
            blockSprite.sortingOrder = reverse ? BOTTOM : TOP;
        }
    }
    public List<Vector2Int> BlockPositions()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (var pos in blockPositions)
        {
            result.Add(pos + new Vector2Int(
                Mathf.FloorToInt(currentPos.y),
                Mathf.FloorToInt(currentPos.x)
            ));
        }

        return result;
    }

    public void UpdateIncorrectMove()
    {
        currentPos = previousPos;
        transform.position = currentPos;
    }

    public void UpdateStartMove()
    {
        currentPos = startPos;
        previousPos = startPos;
        transform.position = currentPos;
    }

    public void UpdateCorrectMove()
    {
        currentPos.x = Mathf.FloorToInt(currentPos.x) + 0.5f;
        currentPos.y = Mathf.FloorToInt(currentPos.y) + 0.5f;
        previousPos = currentPos;
        transform.position = currentPos;
        if (UiHandler._difficultyLevel == 2 || UiHandler._difficultyLevel == 1)
        {
            GameManager.blockstobeSpawnned++;
            return;
        }
        else
        {
            GameManager.Instance.SpawnBlocks(spawnPositon);
        }

    }
    public void DisableCellsInLine(List<int> rowsToDisable, List<int> colsToDisable)
    {
        
        for (int i = blockPositions.Count - 1; i >= 0; i--)
        {
            Vector2Int cellPos = BlockPositions()[i];
            
            if (rowsToDisable.Contains(cellPos.x) || colsToDisable.Contains(cellPos.y))
            {
                blockPositions.RemoveAt(i);
                blockSprites.RemoveAt(i);
            }
        }
    }

    public void DisableCellsInCubes(List<Vector2Int> cubesToDisable)
    {

        List<int> cellsToDisable = new List<int>();


        for (int i = 0; i < blockPositions.Count; i++)
        {
            Vector2Int cellPos = BlockPositions()[i];

            foreach (var cube in cubesToDisable)
            {
                if (cellPos.x >= cube.x && cellPos.x < cube.x + 3 &&
                    cellPos.y >= cube.y && cellPos.y < cube.y + 3)
                {
                    cellsToDisable.Add(i);
                    break;
                }
            }
        }

        for (int i = cellsToDisable.Count - 1; i >= 0; i--)
        {
            blockPositions.RemoveAt(cellsToDisable[i]);
            blockSprites.RemoveAt(cellsToDisable[i]);
        }
    }

    public void BlockedBlock()
    {
        foreach (var blockSprite in blockSprites)
        {
            blockSprite.sprite = _inCorrectSprite;
        }
    }

    public void UnBlockBlockedBlock()
    {
        
        foreach (var blockSprite in blockSprites)
        {
            blockSprite.sprite = _CorrectSprite;
        }
    }
    public List<GameObject> GetCellsToDisable(List<int> fullRows, List<int> fullCols, List<Vector2Int> fullCubes)
    {
        HashSet<GameObject> uniqueCells = new HashSet<GameObject>();

        for (int i = 0; i < blockPositions.Count; i++)
        {
            Vector2Int cellPos = BlockPositions()[i];

          
            if (fullRows.Contains(cellPos.x) || fullCols.Contains(cellPos.y))
            {
                GameObject cell = blockSprites[i].gameObject;
                if (cell != null && cell.activeSelf)
                {
                    uniqueCells.Add(cell);
                }
            }

            foreach (var cube in fullCubes)
            {
                if (cellPos.x >= cube.x && cellPos.x < cube.x + 3 &&
                    cellPos.y >= cube.y && cellPos.y < cube.y + 3)
                {
                    GameObject cell = blockSprites[i].gameObject;
                    if (cell != null && cell.activeSelf)
                    {
                        uniqueCells.Add(cell);
                    }
                    break; 
                }
            }
        }

        return uniqueCells.ToList();
    }
}

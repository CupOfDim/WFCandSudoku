using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class VoxelTilePlacerWFC : MonoBehaviour
{
    public List<VoxelTile> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);

    private VoxelTile[,] spawnedTiles;

    private Queue<Vector2Int> recalcPossibleTileQueue = new Queue<Vector2Int>();
    private List<VoxelTile>[,] possibleTiles;

    void Start()
    {
        spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            tilePrefab.CalcSideColors();
        }

        int countBeforeAdding = TilePrefabs.Count();
        for (int i = 0; i < countBeforeAdding; i++)
        {
            VoxelTile clone;
            switch (TilePrefabs[i].Rotation)
            {
                case VoxelTile.RotType.OnlyRot:
                    break;

                case VoxelTile.RotType.TwoRot:
                    TilePrefabs[i].Weight /= 2;
                    if (TilePrefabs[i].Weight <= 0) TilePrefabs[i].Weight = 1;


                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    TilePrefabs.Add(clone);
                    break;

                case VoxelTile.RotType.FourRot:
                    TilePrefabs[i].Weight /= 4;
                    if (TilePrefabs[i].Weight <= 0) TilePrefabs[i].Weight = 1;


                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    TilePrefabs.Add(clone);

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    TilePrefabs.Add(clone);

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    clone.Rotate90();
                    TilePrefabs.Add(clone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Generate();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (VoxelTile spawnedTile in spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }

            Generate();
        }
    }

    private void Generate()
    {
        possibleTiles = new List<VoxelTile>[MapSize.x, MapSize.y];

        for(int x = 0; x < MapSize.x; x++)
        {
            for(int y = 0; y < MapSize.y; y++)
            {
                possibleTiles[x, y] = new List<VoxelTile>(TilePrefabs);
            }
        }

        VoxelTile tileInCenter = GetRandomTile(TilePrefabs);
        possibleTiles[MapSize.x/2, MapSize.y/2] = new List<VoxelTile> { tileInCenter};

        recalcPossibleTileQueue.Clear();
        EnqueueNeighboursToRecall(new Vector2Int(MapSize.x / 2, MapSize.y / 2));

        GenerateAllPossibleTiles();

        PlaceAllTiles();
    }

    private void PlaceAllTiles()
    {
        for (int x = 1; x < MapSize.x - 1; x++)
        {
            for (int y = 1; y < MapSize.y-1; y++)
            {
                PlaceTile(x, y);
            }
        }
    }

    private void GenerateAllPossibleTiles()
    {
        int maxIter = MapSize.x*MapSize.y;
        int iter = 0;
        while (iter++ < maxIter)
        {
            int maxInnerIter = 300;
            int innerIter = 0;
            while (recalcPossibleTileQueue.Count > 0 && innerIter++<maxInnerIter)
            {
                Vector2Int pos = recalcPossibleTileQueue.Dequeue();

                if (pos.x == 0 || pos.y == 0 || pos.x == MapSize.x - 1 || pos.y == MapSize.y - 1)
                {
                    continue;
                }

                List<VoxelTile> possibleTilesHere = possibleTiles[pos.x, pos.y];
                int countRemoved = possibleTilesHere.RemoveAll(t => !IsTilePossible(t, pos));

                if (countRemoved > 0)
                {
                    EnqueueNeighboursToRecall(pos);
                }

                //TODO: что если possibleTilesHere стал пустой

                if(possibleTilesHere.Count == 0)
                {
                    //если зашли в тупик
                    possibleTilesHere.AddRange(TilePrefabs);
                    possibleTiles[pos.x+1, pos.y] = new List<VoxelTile>(TilePrefabs);
                    possibleTiles[pos.x - 1, pos.y] = new List<VoxelTile>(TilePrefabs);
                    possibleTiles[pos.x, pos.y+1] = new List<VoxelTile>(TilePrefabs);
                    possibleTiles[pos.x, pos.y - 1] = new List<VoxelTile>(TilePrefabs);

                    EnqueueNeighboursToRecall(pos);
                }
            }
            if (innerIter == maxInnerIter) break;

            List<VoxelTile> maxCountTile = possibleTiles[1, 1];
            Vector2Int maxCountTilePos = new Vector2Int(1, 1);

            for (int x = 1; x < MapSize.x-1; x++)
            {
                for (int y = 1; y < MapSize.y-1; y++)
                {
                    if (possibleTiles[x, y].Count > maxCountTile.Count)
                    {
                        maxCountTile = possibleTiles[x, y];
                        maxCountTilePos = new Vector2Int(x, y);
                    }
                }
            }

            if(maxCountTile.Count == 1)
            {
                return;
            }

            VoxelTile tileToCollapse = GetRandomTile(maxCountTile);
            possibleTiles[maxCountTilePos.x, maxCountTilePos.y] = new List<VoxelTile> { tileToCollapse };
            EnqueueNeighboursToRecall(maxCountTilePos);
        }
        Debug.Log("Count of Iterations go out to the board");
    }

    private bool IsTilePossible(VoxelTile tile, Vector2Int pos)
    {
        bool isAllRightImpossible = possibleTiles[pos.x-1, pos.y].All(rightTile=>!CanAppendTile(tile,rightTile,Direction.Right));
        if(isAllRightImpossible) return false;

        bool isAllLeftImpossible = possibleTiles[pos.x + 1, pos.y].All(leftTile => !CanAppendTile(tile, leftTile, Direction.Left));
        if (isAllLeftImpossible) return false;

        bool isAllForwardImpossible = possibleTiles[pos.x, pos.y - 1].All(forwardTile => !CanAppendTile(tile, forwardTile, Direction.Forward));
        if (isAllForwardImpossible) return false;

        bool isAllBackImpossible = possibleTiles[pos.x, pos.y + 1].All(backTile => !CanAppendTile(tile, backTile, Direction.Back));
        if (isAllBackImpossible) return false;

        return true;
    }

    private void EnqueueNeighboursToRecall(Vector2Int pos)
    {
        recalcPossibleTileQueue.Enqueue(new Vector2Int(pos.x+1, pos.y));
        recalcPossibleTileQueue.Enqueue(new Vector2Int(pos.x-1, pos.y));
        recalcPossibleTileQueue.Enqueue(new Vector2Int(pos.x, pos.y+1));
        recalcPossibleTileQueue.Enqueue(new Vector2Int(pos.x, pos.y-1));
    }

    private void PlaceTile(int x, int y)
    {
        //foreach (VoxelTile tilePrefab in TilePrefabs)
        //{
        //    if (CanAppendTile(spawnedTiles[x - 1, y], tilePrefab, Direction.Left) &&
        //        CanAppendTile(spawnedTiles[x + 1, y], tilePrefab, Direction.Right) &&
        //        CanAppendTile(spawnedTiles[x, y - 1], tilePrefab, Direction.Back) &&
        //        CanAppendTile(spawnedTiles[x, y + 1], tilePrefab, Direction.Forward))
        //    {
        //        availableTiles.Add(tilePrefab);
        //    }
        //}

        if (possibleTiles[x, y].Count == 0) return;

        VoxelTile selectedTile = GetRandomTile(possibleTiles[x, y]);

        spawnedTiles[x, y] = Instantiate(selectedTile, new Vector3(x, 0, y) * selectedTile.TileSideVoxel * selectedTile.VoxelSize, selectedTile.transform.rotation);
    }

    private VoxelTile GetRandomTile(List<VoxelTile> availableTiles)
    {
        List<int> chances = new List<int>();
        for (int i = 0; i < availableTiles.Count; i++)
        {
            chances.Add(availableTiles[i].Weight);
        }

        float value = UnityEngine.Random.Range(0, chances.Sum());
        float sum = 0;

        for (int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if (value < sum)
            {
                return availableTiles[i];
            }
        }

        return availableTiles[availableTiles.Count - 1];
    }

    private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Direction direction)
    {
        if (existingTile == null) return true;

        if (direction == Direction.Right)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft);
        }
        else if (direction == Direction.Left)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight);
        }
        else if (direction == Direction.Forward)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack);
        }
        else if (direction == Direction.Back)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsBack, tileToAppend.ColorsForward);
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.forward/right/left/back", nameof(direction));
        }
    }
}

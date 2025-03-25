using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SudokuWFCgen : MonoBehaviour
{
    [SerializeField] private List<CellTile> cells = new();
    private CellTile currentCell;
    private bool _started;
    private bool _nextStep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitNeighbors();
        StartGenerate();
    }

    private void Update()
    {
        if(Application.isPlaying && !_started) return;
    }

    public void StartGenerate()
    {
        _started = true;
        SetRandomCell();
        AvailableNextStep();
        
        try
        {
            StartCoroutine(WaveFunction());
        } catch (Exception e)
        {
            SetDefault();
            StartGenerate();
            Debug.LogException(e);
        }
    }

    public void RestartGenerate()
    {
        SetDefault();
        StartGenerate();

    }

    private IEnumerator WaveFunction()
    {
        while(cells.Count != 0)
        {
            currentCell = MinCell();
            currentCell.SetValue();
            if(UnityEngine.Random.Range(0,4)%2 == 0)
            {
                currentCell.ShowValue();
            }
            cells.Remove(currentCell);
            yield return null;
        }
        
    }

    private CellTile MinCell()
    {
        CellTile minCell = cells[0];
        foreach(var cell in cells)
        {
            if(cell.GetLengthList() < minCell.GetLengthList())
            {
                minCell = cell;
            }
        }
        return minCell;
    }

    private void AvailableNextStep()
    {
        _nextStep = true;
    }

    private void SetRandomCell()
    {
        int index = UnityEngine.Random.Range(0, cells.Count);
        cells[index].SetValue();
        cells.RemoveAt(index);
    }

    public void SetDefault()
    {
        StopAllCoroutines();
        cells = transform.GetComponentsInChildren<CellTile>().ToList();
        foreach(var cell in cells)
        {
            cell.SetDefault();
            cell.HideValue();
        }
    }

    public void InitNeighbors()
    {
        ClearNeighbors();
        for (int i = 0; i < cells.Count; i++)
        {
            for (int j = 0; j < cells.Count; j++)
            {
                if(i == j) continue;

                bool neighborX = Math.Abs(cells[i].transform.position.x - cells[j].transform.position.x) <= 1.3 + 0.001;
                bool neighborZ = Math.Abs(cells[i].transform.position.z - cells[j].transform.position.z) <= 1.3 + 0.001;

                if (neighborX && neighborZ)
                    cells[i].Neighbors.Add(cells[j]);

                if (cells[i].GroupIndex == cells[j].GroupIndex)
                    cells[i].NeighborsGroup.Add(cells[j]);

                if (Mathf.Approximately(cells[i].transform.position.x, cells[j].transform.position.x))
                    cells[i].NeighboursX.Add(cells[j]);

                if (Mathf.Approximately(cells[i].transform.position.z, cells[j].transform.position.z))
                    cells[i].NeighboursY.Add(cells[j]);
            }
        }

    }

    private void ClearNeighbors()
    {
        foreach(var cell in cells)
        {
            cell.Neighbors.Clear();
            cell.NeighborsGroup.Clear();
            cell.NeighboursX.Clear();
            cell.NeighboursY.Clear();
        }
    }
}

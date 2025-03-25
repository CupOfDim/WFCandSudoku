using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ThreeToThree : MonoBehaviour
{
    public List<CellTile> Cells = new();
    public float Offset = 0.8f;

    private void Start()
    {

        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                Cells[x*3+y].transform.position = new Vector3(x, 0, y)* Offset;
            }
        }
        Generate();
    }

    private void Generate()
    {
        foreach(var cell in Cells)
        {
            cell.SetValue();
        }
    }
}

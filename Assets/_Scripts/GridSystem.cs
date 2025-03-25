using System;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class GridSystem : MonoBehaviour
{
    [SerializeField] private Vector2 _gridSize = Vector2.one;
    [SerializeField] private Vector2 _cellSize = Vector2.one;
    [SerializeField] private Vector2 _spacing;
    private List<Transform> _gridObjects = new();

    private void Update()
    {
        if (Application.isPlaying) return;

        CheckChildren();
        UpdateCellPositions();
    }

    private List<Transform> GetGridObjects()
    {
        List<Transform> gridObjects = new List<Transform>();
        for (int i = 0; i< transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);  
            gridObjects.Add(child);
        }
        return gridObjects;
    }

    private void UpdateCellPositions()
    {
        int xOffset;
        float zPos = transform.position.z;
        for(int i=0; i< _gridObjects.Count; i++)
        {
            xOffset = i % (int)_gridSize.x;
            float xPos = transform.position.x;
            if(xOffset == 0 && i != 0)
            {
                zPos += _cellSize.y + _spacing.y;
            }
            else
            {
                xPos = transform.position.x + _spacing.x * xOffset + _cellSize.x * xOffset;
            }

            _gridObjects[i].localPosition = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z+zPos);
        }
    }

    private void CheckChildren()
    {
        int count = transform.childCount;
        if(count == _gridObjects.Count) return;

        _gridObjects = GetGridObjects();
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class CellTile : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private int _groupIndex;
    [SerializeField] private CanvasController _canvasController;

    public int GroupIndex
    {
        get => _groupIndex;
        set => _groupIndex = value;
    }

    [SerializeField] private int _value;
    [SerializeField] private TextMeshPro _text;
    

    [SerializeField] private List<int> _possibleNumbers = new List<int>();
    [SerializeField] private List<CellTile> _neighbours = new();
    [SerializeField] private List<CellTile> _neighboursGroup = new();
    public List<CellTile> NeighboursX = new();
    public List<CellTile > NeighboursY = new();
    private bool Active = false;

    private Material _originalMaterial;
    private Renderer _renderer;

    public List<CellTile> NeighborsGroup
    {
        get => _neighboursGroup;
        set => _neighboursGroup = value;
    }

    public List<CellTile> Neighbors
    {
        get => _neighbours;
        set => _neighbours = value;
    }


    void Awake()
    {
        if(_text == null)
            _text = GetComponentInChildren<TextMeshPro>();
        _renderer = GetComponent<Renderer>();
        _originalMaterial = _renderer.material;
    }

    public void SetDefault()
    {
        _possibleNumbers = Enumerable.Range(1,9).ToList();
        _value = 0;
        //_text.text = _value.ToString();
    }

    public void SetValue()
    {
        if (_possibleNumbers.Count == 0)
        {
            SudokuWFCgen sudoku = FindFirstObjectByType<SudokuWFCgen>();
            if (sudoku != null)
            {
                sudoku.SetDefault();
                sudoku.StartGenerate();
            }
            else
            {
                Debug.LogError("SudokuWFCgen не найден в сцене!");
            }
            return;
        }
        _value = _possibleNumbers[Random.Range(0, _possibleNumbers.Count)];
        _possibleNumbers.Clear();
        //_text.text = _value.ToString();
        BlockValueInNeighbour();
    }

    public void RemoveValue(int value)
    {
        _possibleNumbers.Remove(value);
    }

    private void BlockValueInNeighbour()
    {
        foreach (var neighbour in _neighboursGroup)
        {
            neighbour.RemoveValue(_value);
        }
        foreach(var neighbourX in NeighboursX)
        {
            neighbourX.RemoveValue(_value);
        }
        foreach( var neighbourY in NeighboursY)
        {
            neighbourY.RemoveValue(_value);
        }
    }

    public int GetValue()
    {
        return _value;
    }

    public int GetLengthList()
    {
        return _possibleNumbers.Count;
    }

    public void ShowValue()
    {
        _text.text = _value.ToString();
        Active = true;
    }

    public void HideValue()
    {
        _text.text = null;
        Active = false;
        RestoreOriginalMaterial();
    }

    void OnMouseDown()
    {
        if (_canvasController != null&& !_canvasController.isActiveAndEnabled && !Active)
        {
            _canvasController.OpenCanvas(this);
        }
    }

    public void ApplyMaterial(Material material)
    {
        _renderer.material = material;
    }

    public void RestoreOriginalMaterial()
    {
        _renderer.material = _originalMaterial;
    }
}

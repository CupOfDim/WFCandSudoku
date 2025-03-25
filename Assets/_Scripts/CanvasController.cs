using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Button[] _numberButtons;
    [SerializeField] private Button _closeButton;

    private CellTile _currentTile;

    private Material[] _materials;

    private void Start()
    {
        _canvas.SetActive(false);

        foreach (var button in _numberButtons)
        {
            int number = int.Parse(button.GetComponentInChildren<TMP_Text>().text);
            button.onClick.AddListener(() => OnNumberSelected(number));
        }

        _closeButton.onClick.AddListener(CloseCanvas);
        _materials = new Material[2];
        _materials[0] = Resources.Load<Material>("_Materials/Wrong");
        _materials[1] = Resources.Load<Material>("_Materials/True");
    }

    public void OpenCanvas(CellTile tile)
    {
        _currentTile = tile;
        _canvas.SetActive(true);
    }

    private void OnNumberSelected(int number)
    {
        if (_currentTile != null)
        {
            if(number == _currentTile.GetValue())
            {
                _currentTile.ApplyMaterial(_materials[1]);
                _currentTile.ShowValue();
            }
            else
            {
                _currentTile.ApplyMaterial(_materials[0]);
            }
        }
        CloseCanvas();
    }

    public void CloseCanvas()
    {
        _canvas.SetActive(false);
    }
}

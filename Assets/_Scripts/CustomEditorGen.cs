using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SudokuWFCgen))]
public class CustomEditorGen : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SudokuWFCgen exmp = (SudokuWFCgen)target;

        if (GUILayout.Button("Generate"))
        {
            exmp.StartGenerate();
        }

        if (GUILayout.Button("Reset"))
        {
            exmp.SetDefault();
        }

        if (GUILayout.Button("Regenerate"))
        {
            exmp.RestartGenerate();
        }
    }
}

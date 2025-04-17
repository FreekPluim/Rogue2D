using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RealtimeDungeonGenerator))]
public class DungeonFloorGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RealtimeDungeonGenerator gen = (RealtimeDungeonGenerator)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Test Object Overlap"))
        {
            gen.TestBuildingOverlap();
        }
    }
}

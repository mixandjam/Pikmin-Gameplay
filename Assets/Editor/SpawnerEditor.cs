using UnityEngine;
using UnityEditor;

// draw a red circle around the scene cube

[CustomEditor(typeof(PikminSpawner))]
public class SpawnerEditor : Editor
{
    void OnSceneGUI()
    {
        PikminSpawner obj = (PikminSpawner)target;

        Handles.color = Color.blue;
        Handles.DrawWireDisc(obj.transform.position, new Vector3(0, 1, 0), obj.radius);
    }
}
using UnityEngine;
using UnityEditor;

// draw a red circle around the scene cube

[CustomEditor(typeof(CarryObject))]
public class CarryEditor : Editor
{
    void OnSceneGUI()
    {
        CarryObject obj = (CarryObject)target;

        Handles.color = Color.red;
        Handles.DrawWireDisc(obj.transform.position, new Vector3(0, 1, 0), obj.radius);
    }
}
using System;
using System.Collections;
using System.Collections.Concurrent;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JSONSerializer))]
public class ConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Get the target object
        JSONSerializer JSONScript = (JSONSerializer)target;

        // Create a button and call SaveScene when pressed
        if (GUILayout.Button("Save Config"))
        {
            JSONScript.SaveScene(); // Call the function from the script
        }
        if (GUILayout.Button("Load Config"))
        {
            JSONScript.LoadScene();
        }
    }
}
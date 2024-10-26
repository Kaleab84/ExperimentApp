using System;
using System.Collections;
using System.Collections.Concurrent;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoadScenes))]
public class ConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Get the target object
        LoadScenes saveConfig = (LoadScenes)target;

        // Create a button and call SaveScene when pressed
        if (GUILayout.Button("Save Config"))
        {
            saveConfig.SaveScene(); // Call the function from the script
        }
        if (GUILayout.Button("Load Config"))
        {
            saveConfig.LoadScene();
        }
    }
}
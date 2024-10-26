using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Obstacle
{
    public Vector3 Scale;
    public Vector3 Position;
}

[System.Serializable]
public class SceneConfig
{
    #region Hallway Data
    public float[] HallwayData = new float[Enum.GetValues(typeof(HallFaceEnum)).Length];
    #endregion

    #region Obstacles
    public List<Obstacle> Obstacles = new List<Obstacle>();
    #endregion
}

[System.Serializable]
public class AllSceneConfigurations
{
    public List<SceneConfig> Scenes = new List<SceneConfig>();
}
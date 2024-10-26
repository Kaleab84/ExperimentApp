using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;
using System.Linq;

[System.Serializable]
public class LoadScenes : MonoBehaviour
{
    [SerializeField] private int sceneNumber = 0;
    public int SceneNumber { get { return sceneNumber; } set { sceneNumber = value; } }

    public void SaveScene()
    {
            string json = File.ReadAllText(Application.dataPath + "/SceneConfigurationFile.json");  // Retrieving all scenes to modify them
            AllSceneConfigurations All = JsonUtility.FromJson<AllSceneConfigurations>(json);

        if (SceneNumber < All.Scenes.Count)
        {
            SceneConfig newScene = new SceneConfig();   // Scene to insert

            #region Store Hallway Data
            if (Pastel.Instance != null)
            {
                newScene.HallwayData[0] = Pastel.Instance.Length;
                newScene.HallwayData[1] = Pastel.Instance.Width;
                newScene.HallwayData[2] = Pastel.Instance.Height;
                newScene.HallwayData[3] = Pastel.Instance.Scale;
            }
            else
            {
                Debug.Log("Instance is null");
            }
            #endregion

            #region Store Obstacles
            // Find the "obstacles" GameObject
            Transform obstaclesTransform = transform.Find("Obstacles");

            if (obstaclesTransform != null)
            {
                // Loop through all children of the "obstacles" GameObject
                foreach (Transform child in obstaclesTransform)
                {
                    Obstacle obstacle = new Obstacle();
                    obstacle.Scale = child.localScale;
                    obstacle.Position = child.position;
                    newScene.Obstacles.Add(obstacle);
                }
            }
            else
            {
                Debug.LogWarning("Obstacles object not found!");
            }
            #endregion

            All.Scenes[SceneNumber] = newScene;
            json = JsonUtility.ToJson(All, true);
            File.WriteAllText(Application.dataPath + "/SceneConfigurationFile.json", json);
        }
    }

    public void LoadScene()
    {
        string json = File.ReadAllText(Application.dataPath + "/SceneConfigurationFile.json");
        AllSceneConfigurations All = JsonUtility.FromJson<AllSceneConfigurations>(json);

        if (sceneNumber < All.Scenes.Count)
        {
            SceneConfig scene = All.Scenes[SceneNumber];

            GameObject GameScene = new GameObject("Game Scene");

            #region Load Hallway
            GameObject hallway = new GameObject("Hallway");
            hallway.transform.SetParent(GameScene.transform);
            HallwayComp[] Faces = new HallwayComp[Enum.GetValues(typeof(HallFaceEnum)).Length];
            for (int i = 0; i < Faces.Length; i++)
            {
                HallwayComp obj = GameObject.CreatePrimitive(PrimitiveType.Plane).AddComponent<HallwayComp>();
                obj.Face = (HallFaceEnum)i;
                obj.name = ((HallFaceEnum)i).ToString();
                obj.transform.SetParent(hallway.transform); // organize them in the editor under one transform(parent prefab)
                Faces[i] = obj;
                SetHallwayFaces(scene, Faces[i]); // Fix rotation, scale & position
            }
            #endregion

            #region Load Obstacles
            GameObject staticObstacles = new GameObject("Static Obstacles");
            staticObstacles.transform.SetParent(GameScene.transform);
            for (int i = 0; i < scene.Obstacles.Count; i++)
            {
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.transform.localScale = scene.Obstacles[i].Scale;
                obstacle.transform.position = scene.Obstacles[i].Position;
                obstacle.transform.SetParent(staticObstacles.transform);
            }
            #endregion
        }
    }


    public void SetHallwayFaces(SceneConfig sceneConfig, HallwayComp obj)
    {
        int pos;

        switch (obj.Face)
        {
            case HallFaceEnum.Left:
                //Rotation
                obj.transform.localEulerAngles = new Vector3(0, 180, 90);
                //Scale
                obj.gameObject.transform.localScale = new Vector3(sceneConfig.HallwayData[2], 1, sceneConfig.HallwayData[0]) / 10;

                //Position
                pos = (obj.Face == HallFaceEnum.Left) ? -1 : 1;
                obj.gameObject.transform.position = new Vector3(sceneConfig.HallwayData[1] * pos, sceneConfig.HallwayData[2], sceneConfig.HallwayData[0]) / 2;
                break;
            case HallFaceEnum.Right:
                obj.transform.localEulerAngles = new Vector3(0, 180, -90);

                obj.gameObject.transform.localScale = new Vector3(sceneConfig.HallwayData[2], 1, sceneConfig.HallwayData[0]) / 10;

                pos = (obj.Face == HallFaceEnum.Left) ? -1 : 1;
                obj.gameObject.transform.position = new Vector3(sceneConfig.HallwayData[1] * pos, sceneConfig.HallwayData[2], sceneConfig.HallwayData[0]) / 2;
                break;
            case HallFaceEnum.Close:
                obj.transform.localEulerAngles = new Vector3(-270, 0, 0);

                obj.gameObject.transform.localScale = new Vector3(sceneConfig.HallwayData[1], 1, sceneConfig.HallwayData[2]) / 10;

                pos = (obj.Face == HallFaceEnum.Close) ? 0 : 2;
                obj.gameObject.transform.position = new Vector3(0, sceneConfig.HallwayData[2], sceneConfig.HallwayData[0] * pos) / 2;
                break;
            case HallFaceEnum.Far:
                obj.transform.localEulerAngles = new Vector3(270, 0, 0);

                obj.gameObject.transform.localScale = new Vector3(sceneConfig.HallwayData[1], 1, sceneConfig.HallwayData[2]) / 10;

                pos = (obj.Face == HallFaceEnum.Close) ? 0 : 2;
                obj.gameObject.transform.position = new Vector3(0, sceneConfig.HallwayData[2], sceneConfig.HallwayData[0] * pos) / 2;
                break;

            case HallFaceEnum.Ceiling:
                obj.transform.localEulerAngles = new Vector3(180, 0, 0);

                obj.gameObject.transform.localScale = new Vector3(sceneConfig.HallwayData[1], 1, sceneConfig.HallwayData[0]) / 10;

                pos = (obj.Face == HallFaceEnum.Ceiling) ? 2 : 0;
                obj.gameObject.transform.position = new Vector3(0, sceneConfig.HallwayData[2] * pos, sceneConfig.HallwayData[0]) / 2;
                break;
            case HallFaceEnum.Floor:
                obj.gameObject.transform.localScale = new Vector3(sceneConfig.HallwayData[1], 1, sceneConfig.HallwayData[0]) / 10;

                pos = (obj.Face == HallFaceEnum.Ceiling) ? 2 : 0;
                obj.gameObject.transform.position = new Vector3(0, sceneConfig.HallwayData[2] * pos, sceneConfig.HallwayData[0]) / 2;
                break;
        }
    }
}

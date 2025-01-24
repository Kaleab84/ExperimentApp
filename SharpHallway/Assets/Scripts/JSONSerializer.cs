using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class JSONSerializer : MonoBehaviour
{
    //Singleton
    private static JSONSerializer instance;
    public static JSONSerializer Instance { get { return instance; } private set { instance = value; } }

    [SerializeField] private int sceneNumber = 0;
    public int SceneNumber { get { return sceneNumber; } set { sceneNumber = value; } }

    public void SaveScene()
    {
        string json = File.ReadAllText(Application.dataPath + "/SceneConfigurationFile.json");  // Retrieving all scenes to modify them
        AllScenesInfo All = JsonUtility.FromJson<AllScenesInfo>(json);

        if (SceneNumber < All.Scenes.Count)
        {
            SceneInfo newScene = new SceneInfo();   // Scene to insert
            newScene.SceneNumber = SceneNumber;

            #region Store Hallway Data
            // Maps data from a static reference "Pastel.Instance" to a new one.
            if (Pastel.Instance != null)
            {
                newScene.HallwayLength = Pastel.Instance.Length;
                newScene.HallwayWidth = Pastel.Instance.Width;
                newScene.HallwayHeight = Pastel.Instance.Height;
                newScene.HallwayScale = Pastel.Instance.Scale;

                newScene.HallwayLeftMaterialPath = GetMaterialPath(Pastel.Instance.Left.gameObject);
                newScene.HallwayFarMaterialPath = GetMaterialPath(Pastel.Instance.Far.gameObject);
                newScene.HallwayCeilingMaterialPath = GetMaterialPath(Pastel.Instance.Ceiling.gameObject);
                newScene.HallwayRightMaterialPath = GetMaterialPath(Pastel.Instance.Right.gameObject);
                newScene.HallwayCloseMaterialPath = GetMaterialPath(Pastel.Instance.Close.gameObject);
                newScene.HallwayFloorMaterialPath = GetMaterialPath(Pastel.Instance.Floor.gameObject);
            }
            else
            {
                Debug.Log("Pastel Instance is null");
            }
            #endregion

            #region Store Static Obstacles
            // Finds the "Static Obstacles" GameObject
            Transform staticObstaclesTransform = transform.Find("Static Obstacles");

            if (staticObstaclesTransform != null)
            {
                // Loops through all children of the "obstacles" GameObject and creates a copy.
                foreach (Transform staticObstacle in staticObstaclesTransform)
                {
                    ObstacleInfo staticObstacleCopy = new ObstacleInfo();
                    staticObstacleCopy.Name = staticObstacle.name;
                    staticObstacleCopy.tag = staticObstacle.tag;
                    staticObstacleCopy.Scale = staticObstacle.localScale;
                    staticObstacleCopy.Position = staticObstacle.position;
                    staticObstacleCopy.ObstacleMaterialPath = GetMaterialPath(staticObstacle.gameObject);
                    newScene.Obstacles.Add(staticObstacleCopy);
                }
            }
            else
            {
                Debug.LogWarning("Static Obstacles object not found!");
            }
            #endregion

            #region Store Moving Obstacles
            // Find the "Moving Obstacles" GameObject
            Transform movingObstaclesTransform = transform.Find("Moving Obstacles");

            if (movingObstaclesTransform != null)
            {
                // Loop through all children of the "Moving Obstacles" GameObject
                foreach (Transform movingObstacle in movingObstaclesTransform)
                {
                    MovingObstacleComp movingObstacleScript = movingObstacle.GetComponent<MovingObstacleComp>();

                    ObstacleInfo movingObstacleCopy = new ObstacleInfo();
                    movingObstacleCopy.Name = movingObstacle.name;
                    movingObstacleCopy.Scale = movingObstacle.localScale;
                    movingObstacleCopy.StartPoint = movingObstacleScript.StartPoint;
                    movingObstacleCopy.EndPoint = movingObstacleScript.EndPoint;
                    movingObstacleCopy.RespawnDelay = movingObstacleScript.RespawnDelay;
                    movingObstacleCopy.Speed = movingObstacleScript.Speed;
                    movingObstacleCopy.BackNForth = movingObstacleScript.BackNForth;
                    movingObstacleCopy.ObstacleMaterialPath = GetMaterialPath(movingObstacle.gameObject);
                    newScene.MovingObstacles.Add(movingObstacleCopy);
                }
            }
            else
            {
                Debug.LogWarning("No Moving Obstacles object found!");
            }
            #endregion

            All.Scenes[SceneNumber] = newScene;
            json = JsonUtility.ToJson(All, true);
            File.WriteAllText(Application.dataPath + "/SceneConfigurationFile.json", json);
        }
        else
        {
            Debug.Log($"Scene out of bound (Scenes Capped to 8)");
        }
    }

    public void LoadScene(int? loadSceneNumber = null)
    {
        // If Called from Game Editor, will be assigned class variable "SceneNumber" by default, else sceneNumber passed explicitly
        loadSceneNumber = (loadSceneNumber == null) ? SceneNumber : loadSceneNumber;
        SceneNumber = loadSceneNumber.Value;

        string json = File.ReadAllText(Application.dataPath + "/SceneConfigurationFile.json");  // Load the json file
        AllScenesInfo All = JsonUtility.FromJson<AllScenesInfo>(json);

        if (loadSceneNumber < All.Scenes.Count)
        {
            SceneInfo scene = All.Scenes[loadSceneNumber.Value];    // Load the dictated scene

            #region Load Hallway
            // Fetches data from the JSON to a static reference of the Hallway "Pastel.Instance"
            Pastel.Instance.Length = scene.HallwayLength;
            Pastel.Instance.Width = scene.HallwayWidth;
            Pastel.Instance.Height = scene.HallwayHeight;
            Pastel.Instance.Scale = scene.HallwayScale;

            Pastel.Instance.Floor.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.HallwayFloorMaterialPath);
            Pastel.Instance.Left.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.HallwayLeftMaterialPath);
            Pastel.Instance.Far.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.HallwayFarMaterialPath);
            Pastel.Instance.Ceiling.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.HallwayCeilingMaterialPath);
            Pastel.Instance.Right.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.HallwayRightMaterialPath);
            Pastel.Instance.Close.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.HallwayCloseMaterialPath);
            #endregion

            #region Load Static Obstacles 

            //Delete Previous Scene Static Obstacles
            Transform staticObstaclesTransform = transform.Find("Static Obstacles");
            if (staticObstaclesTransform != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(staticObstaclesTransform.gameObject);
                }
                else
                {
                    DestroyImmediate(staticObstaclesTransform.gameObject);
                }
            }

            //Load New Scene Obstacles
            GameObject staticObstaclesParent = new GameObject("Static Obstacles");
            staticObstaclesParent.transform.parent = Pastel.Instance.transform;

            for (int i = 0; i < scene.Obstacles.Count; i++)
            {
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = scene.Obstacles[i].Name;
                obstacle.tag = scene.Obstacles[i].tag;
                obstacle.transform.localScale = scene.Obstacles[i].Scale;
                obstacle.transform.position = scene.Obstacles[i].Position;
                obstacle.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.Obstacles[i].ObstacleMaterialPath);
                obstacle.transform.SetParent(staticObstaclesParent.transform);

                // Test code >>
                if (obstacle.name == "FinishZone")
                {
                    obstacle.GetComponent<BoxCollider>().isTrigger = true;
                    obstacle.GetComponent<Renderer>().enabled = false;
                }
            }
            #endregion

            #region Load Moving Obstacles

            //Delete Previous Scene Moving Obstacles
            Transform movingObstaclesTransform = transform.Find("Moving Obstacles");
            if (movingObstaclesTransform != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(movingObstaclesTransform.gameObject);
                }
                else
                {
                    DestroyImmediate(movingObstaclesTransform.gameObject);
                }
            }

            GameObject movingObstaclesParent = new GameObject("Moving Obstacles");
            movingObstaclesParent.transform.SetParent(Pastel.Instance.transform);

            for (int i = 0; i < scene.MovingObstacles.Count; i++)
            {
                // Create the actual moving obstacle GameObject
                GameObject movingObstacle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                movingObstacle.transform.name = scene.MovingObstacles[i].Name;
                movingObstacle.transform.localScale = scene.MovingObstacles[i].Scale;
                movingObstacle.transform.position = scene.MovingObstacles[i].StartPoint;
                movingObstacle.GetComponent<Renderer>().sharedMaterial = GetMaterial(scene.MovingObstacles[i].ObstacleMaterialPath);
                movingObstacle.transform.SetParent(movingObstaclesParent.transform);

                // Add the MovingObstacle script to the obstacle
                MovingObstacleComp movingObstacleScript = movingObstacle.AddComponent<MovingObstacleComp>();
                movingObstacleScript.StartPoint = scene.MovingObstacles[i].StartPoint;
                movingObstacleScript.EndPoint = scene.MovingObstacles[i].EndPoint;
                movingObstacleScript.Speed = scene.MovingObstacles[i].Speed;
                movingObstacleScript.RespawnDelay = scene.MovingObstacles[i].RespawnDelay;
                movingObstacleScript.BackNForth = scene.MovingObstacles[i].BackNForth;
                //movingObstacleScript.EventName = scene.MovingObstacles[i].Event;

                // Add a rigidbody to the obstacle
                //movingObstacle.AddComponent<Rigidbody>();

                //// Attach Wwiseattcher script to the obstacle, This attaches the sound event name directly you can change it in the inspector,
                ///but it is better to change it in the JSON file
    
                WwiseAttacher wwiseattcher = movingObstacle.AddComponent<WwiseAttacher>();
                wwiseattcher.eventName = "PLAY_PUSHING_GROCERY_CART_63821";
                Debug.Log($"Is {gameObject.name} active? {gameObject.activeInHierarchy}");


            }
			#endregion

			LogManager.Instance.NewTrial();
            Debug.Log(loadSceneNumber.Value);

        }
        else
        {
            Debug.Log($"Scene out of bound ");
        }
    }

    private Material GetMaterial(string materialPath)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    }

    private string GetMaterialPath(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            return AssetDatabase.GetAssetPath(renderer.sharedMaterial);
        }
        else
        {
            Debug.LogWarning("Object does not have a Renderer component or its sharedMaterial is null.");
            return null;
        }
    }

    private void OnEnable()
    {
        Instance = this;
    }
}
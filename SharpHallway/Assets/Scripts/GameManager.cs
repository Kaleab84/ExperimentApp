using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<int> sceneOrder = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
    private int currentScene = 0;

    public Vector3 targetPosition = new Vector3();  // The position where the player triggers the next level
    public float TriggerRadius = 2f; // The radius within which the trigger activates
    public float LevelTransitionDelay = 1f; // Delay before loading next level

    public void Start()
    {
        ShuffleList(sceneOrder);    // Randomize scene order
        JSONSerializer.Instance.LoadScene(sceneOrder[currentScene++]);  // Load the first scene

        Vector3 farWallPosition = Pastel.Instance.Far.transform.position;   // Mark the targetPosition as the Far Wall position
        targetPosition = new Vector3(farWallPosition.x, transform.position.y, farWallPosition.z);
        Debug.Log($"Updated Target Position: {targetPosition} {Pastel.Instance.transform.position} {Pastel.Instance.Far.transform.localPosition}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning(collision.gameObject.name);
    }

    private void TriggerLevelTransition()
    {
        if (currentScene < 8)
        {
            JSONSerializer.Instance.LoadScene(sceneOrder[currentScene++]);
        }
        else
        {
            currentScene = 0;
        }

        GetComponent<FPSController>().enabled = false;  // FPSControler overrides position, so it has to be disabled while resetting the position.
        Vector3 closeWallPosition = Pastel.Instance.Close.transform.position; // Respawning position set to the "Close" Wall
        transform.position = new Vector3(closeWallPosition.x, 1, closeWallPosition.z); // position.y has to be 1 (floor level)
        GetComponent<FPSController>().enabled = true;

        Vector3 farWallPosition = Pastel.Instance.Far.transform.position;   // Mark the targetPosition as the Far Wall position
        targetPosition = new Vector3(farWallPosition.x, transform.position.y, farWallPosition.z);
        Debug.Log($"Updated Target Position: {targetPosition} Far wall position: {farWallPosition}");
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<int> sceneOrder = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
    private int currentScene = 0;

    public float LevelTransitionDelay = 1f; // Delay before loading next level
    public AudioSource audioPlayer;

    public void Start()
    {
        ShuffleList(sceneOrder);    // Randomize scene order
        JSONSerializer.Instance.LoadScene(sceneOrder[currentScene++]);  // Load the first scene
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject.Find("FinishZone").GetComponent<BoxCollider>().isTrigger = false;
        StartCoroutine(TriggerLevelTransition(LevelTransitionDelay));
    }

    IEnumerator TriggerLevelTransition(float delay)
    {
        audioPlayer.Play();
        yield return new WaitForSeconds(delay); // Wait for 'delay' seconds

        if (currentScene >= 8)
        {
            currentScene = 0;
        }

        JSONSerializer.Instance.LoadScene(sceneOrder[currentScene++]);
        //yield return new WaitForSeconds(2f);

        GetComponent<FPSController>().enabled = false;  // FPSControler overrides position, so it has to be disabled while resetting the position.
        Vector3 closeWallPosition = Pastel.Instance.Close.transform.position; // Respawning position set to the "Close" Wall
        transform.position = new Vector3(closeWallPosition.x, 1, closeWallPosition.z); // position.y has to be 1 (floor level)
        yield return new WaitForSeconds(0.1f);
        GetComponent<FPSController>().enabled = true;

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
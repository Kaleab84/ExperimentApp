using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } private set { instance = value; } }

    private List<int> sceneOrder = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
    private Player player;

    private int currentScene = 0;
    public static int CurrentScene { get; }

    private int repetitionCount = 0;

    public float LevelTransitionDelay = 1f; // Delay before loading next level
    public AudioSource audioPlayer;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        player = Player.Instance;
        ShuffleList(sceneOrder);    // Randomize scene order
        JSONSerializer.Instance.LoadScene(sceneOrder[currentScene++]);  // Load the first scene
    }

    private void OnTriggerEnter(Collider other)
    {
        //GameObject.Find("FinishZone").GetComponent<BoxCollider>().isTrigger = false;
        //StartCoroutine(TriggerLevelTransition(LevelTransitionDelay));
    }

    public void TriggerLevelTransition()
    {
        audioPlayer.Play();
        //yield return new WaitForSeconds(delay); // Wait for 'delay' seconds

        if (repetitionCount <= 3)
        {
            if (currentScene >= 8)
            {
                currentScene = 0;
                repetitionCount++;
            }

            StartCoroutine(player.ResetPos());
            JSONSerializer.Instance.LoadScene(sceneOrder[currentScene++]);
            player.Fin = false;
        }

        else
        {//end of game
            Debug.Log("fin.");
        }
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
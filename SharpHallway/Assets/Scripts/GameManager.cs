using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //JSONScript.LoadScene();   
    }
}
/*
 using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    public Vector3 targetPosition;  // The position where the player triggers the next level
    public float triggerRadius = 2f; // The radius within which the trigger activates
    public float levelTransitionDelay = 1f; // Delay before loading next level

    public void Update()
    {
        // Check if the player is close enough to the target position
        if (Vector3.Distance(transform.position, targetPosition) <= triggerRadius)
        {
            // If player is close enough, trigger the level transition
            TriggerLevelTransition();
        }
    }

    private void TriggerLevelTransition()
    {
        // Call a function to load the next level (for example, we use the SceneManager to load the next scene)
        Debug.Log("Player reached the target position! Transitioning to next level...");

        // Load the next level (example: by index)
        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            Debug.Log("No more levels! Returning to main menu or restarting.");
        }
    }
}
 */

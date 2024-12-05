using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Player : MonoBehaviour
{
	private static Player instance;
	public static Player Instance { get { return instance; } set{ instance = value; } }

    private HashSet<string> collisions = new HashSet<string>();
    private FPSController controller;
	
	private bool fin = false;

	public void Awake()
	{
		Instance = this;
		controller = gameObject.GetComponent<FPSController>();
	}

	public IEnumerator ResetPos()
	{
		controller.enabled = false;  // FPSControler overrides position, so it has to be disabled while resetting the position.
		Vector3 closeWallPosition = Pastel.Instance.Close.transform.position; // Respawning position set to the "Close" Wall
		gameObject.transform.position = new Vector3(closeWallPosition.x, 1, closeWallPosition.z); // position.y has to be 1 (floor level)

		yield return new WaitForSeconds(0.1f); //This is needed so that the character actually gets moved (unity issue prob)
		controller.enabled = true;
		fin = false;
	}
    

    public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		string name = hit.gameObject.name;
		if(collisions.Add(name)) {
			if(LogManager.Instance.trialLogger == null) { throw new NullReferenceException("Player: TrialLogger does not exist."); }
			LogManager.Instance.trialLogger.LogCollision(name);
		}
	}
	

    public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "FinishZone") {
			if (!fin) {
				fin = true;
				GameManager.Instance.TriggerLevelTransition();
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Player : MonoBehaviour
{
	private static Player instance;
	public static Player Instance { get { return instance; } set{ instance = value; } }

	private FPSController controller;
	private bool fin = false;

	private void Awake()
	{
		Instance = this;
		controller = gameObject.GetComponent<FPSController>();
	}

	public IEnumerator ResetPos()
	{
		controller.enabled = false;  // FPSControler overrides position, so it has to be disabled while resetting the position.

		Vector3 closeWallPosition = Pastel.Instance.Close.transform.position; // Respawning position set to the "Close" Wall
		//gameObject.transform.position = new Vector3(closeWallPosition.x, 1, closeWallPosition.z); // position.y has to be 1 (floor level)
		gameObject.transform.position = new Vector3(0,1,0);

		yield return new WaitForSeconds(1);
		controller.enabled = true;
		fin = false;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//Debug.Log($"{hit.gameObject.name}");


	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "FinishZone") {
			if (!fin) {
				fin = true;
				GameManager.Instance.TriggerLevelTransition();
			}
		}
	}
}

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSpy : MonoBehaviour
{
	public static TransformSpy instance;
	public TrialLogger trialLogger { get; private set; }

	private string path;

	private void Awake()
	{
		instance = this;
		if(gameObject == null){ throw new MissingReferenceException("TransformSpy: TransformSpy must be attached to player camera for logging to occur."); }

		//TEMPORARY CHANGE ASAP
		path = Path.Combine(Application.persistentDataPath, "tempLog.csv");
	}

	private void FixedUpdate()
	{
		if(trialLogger != null) {
			Debug.Log("WriteLog");
			trialLogger.WriteLog();
		}
	}

	public void NewTrial() {
		if(gameObject == null){ throw new MissingReferenceException("TransformSpy: TransformSpy must be attached to player camera for logging to occur."); }
		if(trialLogger != null){ trialLogger.Dispose(); }

		trialLogger = new TrialLogger(gameObject.transform, path);
		trialLogger.Start();
	}
}

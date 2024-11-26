using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public class TransformSpy : MonoBehaviour
{
	public static TransformSpy instance;
	public TrialLogger trialLogger { get; private set; }

	private string path;

    public void Awake() {
		instance = this;
		if(gameObject == null){ throw new MissingReferenceException("TransformSpy: TransformSpy must be attached to player camera for logging to occur."); }

		//TEMPORARY CHANGE ASAP
		path = Path.Combine(Application.persistentDataPath, "tempLog.csv");
	}

	//Whenever the expirement starts (IE Trial 1), create a directory inside persistant data path with a unique name (Maybe millisecond timecode put through simple cypher).
	//Use that directory for the rest of the trials. Create a new one for each expirement (if that wasn't already apparent)

	//The path would be something like "{Application.PersistantDataPath}\{UID}\{TrailNum}.csv" or whatever it looks like after combining...

	public void FixedUpdate() {
		if(trialLogger != null) {
			Debug.Log("WriteLog");
			trialLogger.WriteLog();
		}
	}

	public string GenerateExperimentID() {
		long timeCodeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		StringBuilder sb = new StringBuilder();

		long right = timeCodeMs % 10; //I guess we are using longs now... (it doesn't matter because this happens infrequently)

		while(timeCodeMs > 0) {
			right = (int)(timeCodeMs % 10); //Gets the rightmost digit

			//Apply cypher here (append to sb when done)

			timeCodeMs /= 10; //Gets rid of the rightmost digit
		}
	}


    public void NewTrial() {
		if(gameObject == null){ throw new MissingReferenceException("TransformSpy: TransformSpy must be attached to player camera for logging to occur."); }
		if(trialLogger != null){ trialLogger.Dispose(); }

		trialLogger = new TrialLogger(gameObject.transform, path);
		trialLogger.Start();
	}
}

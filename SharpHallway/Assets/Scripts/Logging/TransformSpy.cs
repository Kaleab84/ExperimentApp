using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Buffers.Text;
using System.Linq;

public class TransformSpy : MonoBehaviour
{
    //Looking at this code has given me a small handful of diseases but at least it works :)  -- It's mostly the naming schemes and the orginization of the program (TransformSpy should not be the logging manager [hey thats a good file name!])

	public static TransformSpy instance;
	public TrialLogger trialLogger { get; private set; }

    private static string experimentID;
	private string basePath;
    private static int trialNum = 0;

    public static string ExperimentID{ get{ return experimentID; } }
    public static int TrialNum { get{ return trialNum; } private set{ trialNum = value; } }

    public void Awake() {
		instance = this;
		if(gameObject == null){ throw new MissingReferenceException("TransformSpy: TransformSpy must be attached to player camera for logging to occur."); }
        CreateBasePath();
	}

	public void FixedUpdate() {
		if(trialLogger != null) {
			trialLogger.WriteLog();
		}
	}

    public void NewTrial() {
        if(TrialNum == 0) { CreateBasePath(); }
        TrialNum++;
        if(gameObject == null) { throw new MissingReferenceException("TransformSpy: TransformSpy must be attached to player camera for logging to occur."); }
        if(trialLogger != null) { trialLogger.Dispose(); }

        Debug.Log(JSONSerializer.Instance.SceneNumber);
        trialLogger = new TrialLogger(gameObject.transform, Path.Combine(basePath, $"{TrialNum}.csv"));
        trialLogger.Start();
    }

    //Notes for future more cognizant self:
    // System must account for current trial loop
    // basepath is path of expierement folder, which is insider expierments folder.

    //Right now all files get overriden?
    //Scene number is always 7, when testing logging reported that the scene would change to level n and 7 where n was seemingly random as expected, but then level 7 was logged every single time.
    //Currently for some reason only one csv is created per expierment, and it is always titled 7.

    //This isn't even a problem becasue we want to use trial numbers instead of level numbers, but they are still important for data collection.

    private void CreateBasePath() {
        experimentID = GenerateExperimentID();
        basePath = Path.Combine(Application.persistentDataPath, "Experiments", experimentID);
        string directoryPath = Path.GetDirectoryName(basePath);

        if(Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }
    }

    public string GenerateExperimentID() {
		long uuid = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		long key = 482338932;

		uuid = (uuid % 1000000007) ^ key;

        return Base36(uuid);
	}

    // bbbxxxxgggbbbbb

    private string Base36(long num) {
        //Takes number and converts it into base 36, the same as any other base.

        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder sb = new StringBuilder();

        bool isNeg = num < 0;
        num = Math.Abs(num);

        //do while
        do {
            sb.Append(chars[(int)(num % 36)]);
            num /= 36;
        } while(num > 0);

        return sb.ToString();
    }
}

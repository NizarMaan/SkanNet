using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataSetActivator : MonoBehaviour {
    public Vuforia.VuforiaConfiguration configurationFile_setter;
    public static Vuforia.VuforiaConfiguration configurationFile;

    // Use this for initialization
    void Awake () {
        configurationFile = configurationFile_setter;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

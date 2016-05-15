using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityOSC;


public class OSCFaceExample1 : MonoBehaviour {

	// Use this for initialization of the OSC system
	void Start () {
		Debug.Log ("OSCFaceExampe1 Start");
		// We want to receive data from our "Processing" application, which is sending on port 8000
		OSC.ReceiverPort(8000);
		// Register our "callback" method for messages labeled with address "/processing/mouse/"
		OSC.OnReceive("/processing/face/", FaceHandler); 
		OSC.OnReceive("/processing/faceOff/", FaceOffHandler); 
	}
	
	// FaceHandler is our "callback" method, that will be called (by OSC)
	// It gets a List of <OSCArguments> representing the data being sent.
	// We expect two float values, representing a mouse position. (More data fields, if present, are ignored)
	public void FaceHandler(List<OSCArgument> data) {
		// data[0] and data[1[ are the first two (and in this case: only) list elements
		// We expect floats, so we extract their floatValue. Other options: intValue, stringValue
		float x = (data[0].floatValue - 0.5f)*2; // incoming date between 0 and 1. Center in unity is 0 so transform to -x to + x. Use scalefactor for fullscreen. 
		float y = (0.5f - data[1].floatValue)*1; // incoming date between 0 and 1. Center in unity is 0 so transform to -x to + x. Use scalefactor for fullscreen.
		float z = transform.position.z;  // we don't want to modify the current z value
		float scale = 10.0f;
		//Debug.Log ("Mouse Position: "  + x  + ", " + y + "\n");
		Vector3 newPos = new Vector3(scale*x, scale*y, z);
		transform.position = newPos;
	}

	public void FaceOffHandler(List<OSCArgument> data) {
		GameObject gameObject = GameObject.Find("FaceCapsule");
		if( gameObject != null){
			Destroy(gameObject);
		}
	}

}

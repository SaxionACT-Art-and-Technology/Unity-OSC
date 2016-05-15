// An example class for receiving OSC messages. 
// Here we expect messages from a Processing application that sends mouse positions (x,y) via port 8000
// The script should be added as a Unity Component to some 3D Unity object, so we can modify its position
// when we receive mouse position data via OSC.
// In Unity there is an extra Window: OSC Helper, that shows messages received (useful for debugging)
// This scripts modifies the (x,y) coordinates of the Unity 3D object to which the script is attached,
// and also prints Debug messages in the Console, with the mouse positions.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityOSC;

public class SceneChangeTest : MonoBehaviour {
	
	// Use this for initialization of the OSC system
	void Start () {
		Debug.Log ("SceneChangeTest Start");
		// We want to receive data from our "Processing" application, which is sending on port 8000
		OSC.ReceiverPort(8000);
		// Register our "callback" method for messages labeled with address "/processing/mouse/"
		OSC.OnReceive("/processing/mouse/", MouseHandler); 

	}
	
	// MouseHandler is our "callback" method, that will be called (by OSC)
	// It gets a List of <OSCArguments> representing the data being sent.
	// We expect two float values, representing a mouse position. (More data fields, if present, are ignored)
	public void MouseHandler(List<OSCArgument> data) {
		// data[0] and data[1[ are the first two (and in this case: only) list elements
		// We expect floats, so we extract their floatValue. Other options: intValue, stringValue
		float x = data[0].floatValue;
		float y = data[1].floatValue;
		float z = transform.position.z;  // we don't want to modify the current z value
		float scale = 10.0f;
		//Debug.Log ("Mouse Position: "  + x  + ", " + y + "\n");
		Vector3 newPos = new Vector3(scale*x, scale*y, z);
		transform.position = newPos;
		if (transform.position.y >= 1.5f && transform.position.x >= 9.5f ){
			Invoke ("LoadLevel", 1);
		}
	}

	void LoadLevel() {

		Debug.Log ("LoadLevel ..." );
		OSCHandler.Instance.DestroyOSCconnections();
		Application.LoadLevel("Scene2");
	}
	
}


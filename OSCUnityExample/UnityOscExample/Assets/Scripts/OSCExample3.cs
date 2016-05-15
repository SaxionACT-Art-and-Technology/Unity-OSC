// An example class for receiving and sending OSC messages with more than one remote partners
// Here we expect messages from a Processing application that sends mouse positions (x,y) via port 8000
// The script should be added as a Unity Component to some 3D Unity object.
	
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityOSC;

public class OSCExample3 : MonoBehaviour {

	// we are going to use two senders, for sending to two different remote apps
	string senderName1, senderName2;
	static int counter = 0; // used for incrementing message value

	// Use this for initialization
	void Start () {
		// We want to receive data from our applications, which are sending on port 7000 and port 8000
		OSC.ReceiverPort(7000);
		OSC.ReceiverPort(8000);

		// We want to send data to our Processing applications, via port 50505 and 9000. 
		// We expect that the Processing applications are running on the same machine, so we use
		// the special "local host" IP address 127.0.0.1
		// Since we now have two "senders", we record their send names.
		senderName1=OSC.SenderAddress("127.0.0.1", 5050); 
		senderName2=OSC.SenderAddress("127.0.0.1", 9000); 

		// Register our "callback" method for messages labeled with address "/processing/mouse/"
		// This applies to messages from  both port 7000 and port 8000.
		OSC.OnReceive("/processing/mouse/", MouseHandler); 
	}
	
	// MouseHandler is our "callback" method, that will be called (by OSC)
	// It gets a List of <OSCArguments> representing the data being sent.
	// We expect two float values, representing a mouse position, and an extra string value.
	public void MouseHandler(List<OSCArgument> data) {
		// data[0] and data[1[ are floats with a mouse position, 
		// data[2] is a string with some "message text"
		float x = data[0].floatValue;
		float y = data[1].floatValue;
		string msg = data [2].stringValue;
		float z = transform.position.z;
		float scale = 10.0f;
		Debug.Log ("Mouse position: "  + x  + ", " + y + " Text message: " + msg + "\n");
		Vector3 newPos = new Vector3(scale*x, scale*y, z);
		transform.position = newPos;
		// Send different messages to our two remote applications.
		counter++;
		OSC.Send(senderName1, "/unity/test", (counter + 111) );
		OSC.Send(senderName2, "/unity/test", (counter + 2222) );
	}
		
}


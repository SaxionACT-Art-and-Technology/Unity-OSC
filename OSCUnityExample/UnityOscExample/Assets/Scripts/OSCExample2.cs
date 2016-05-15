// An example class for receiving and sendingOSC messages. 
// Here we expect messages from a Processing application that sends mouse positions (x,y) via port 8000
// The script should be added as a Unity Component to some 3D Unity object.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityOSC;

public class OSCExample2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// We want to receive data from our "Processing" application, which is sending on port 8000
		OSC.ReceiverPort(8000);

		// We want to send data to our Processing application, via port 9000. 
		// We expect that the Processing application is running on the same machine, so we use
		// the special "local host" IP address 127.0.0.1
		OSC.SenderAddress("127.0.0.1", 9000);

		// Register our "callback" method for messages labeled with address "/processing/mouse/"
		OSC.OnReceive("/processing/mouse/", MouseHandler); 
	}

	// MouseHandler is our "callback" method, that will be called (by OSC)
	// It gets a List of <OSCArguments> representing the data being sent.
	// We expect two float values, representing a mouse position, and an extra string value.
	public void MouseHandler(List<OSCArgument> data) {
		// data[0] and data[1[ are floats with a mouse position, 
		// data[2] is a string with some "message text"
		int dataCount = data.Count;  // The length/size/count of the data List
		float x = data[0].floatValue;  // In C# you can "get" List elements using "array notation".
		float y = data[1].floatValue;
		string msg = data[2].stringValue;
		float z = transform.position.z;
		float scale = 10.0f;
		Debug.Log ("DataCount: " + dataCount + " Mouse position: "  + x  + ", " + y + " Text message: " + msg + "\n");
		Vector3 newPos = new Vector3(scale*x, scale*y, z);
		transform.position = newPos;

		// send either single values (ints, floats, or strings), or send Lists of values.
//		OSC.Send("/unity/test", 666 );      // send a single int value
//		OSC.Send("/unity/test", 666.666f ); // send a float value
//		OSC.Send("/unity/test", "666-666" );
		//Create Lists of data, either by using arrays, or by adding elemnts to a list:
		List<object> dataList = new List<object>(new object[]{666, 777.777f, "six"});
		dataList.Add (888); // add some more...
		dataList.Add ("nine");
		// dataList now contains 5 elements
		// send it back to the processing app:
		OSC.Send ("/unity/test", dataList);


		
	}
	

}

// Author: Job Zwiers
// This is an addition to the OSC package by orge Garcia Martin
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;

namespace UnityOSC {



	// The message handler delegate represents "callback" methods for handling incoming data,
	// in the form of Lists of OSCArguments
    public delegate void OSCMessageHandler(List<OSCArgument> data);



	public class OSC : MonoBehaviour {

//		public void Awake() {
//			Debug.Log ("OSC Awake: don't destroy");
//			DontDestroyOnLoad (this);
//		}

		// A dictionary with, for each OSC "address", a list of message handlers, to be called
		// when a message for some ODSC address arrives. 
		Dictionary<string, List<OSCMessageHandler>> handlers = new Dictionary<string, List<OSCMessageHandler>>();

		// We want a single instance of OSC, so we make the constructor private
		// and we have a public (static) Instance property. 
		private OSC() {}


		// This instance represents the OSC system for users. 
		private static OSC rc = null;
		public static OSC Instance {
			get {
				if (rc == null) {
					Debug.Log ("Create NEW OSC Instance");
					rc = new GameObject ("OSC").AddComponent<OSC>();
				} else {
					Debug.Log ("Reuse existing OSC Instance");
				}

				return rc;

			}
		}

		// Announce that a specified IPO address and port will be used for sending data.
		// The name of the new "sender" is returned. 
		// Several senders are allowed by calling this method more than once.
		public static String SenderAddress(String ipAddress, int port) {
			Dictionary<string, ClientLog> clients = OSCHandler.Instance.Clients;
			String clientName = "OSCClient-" + ipAddress+"-" +port;
			if (clients.ContainsKey (clientName)) {
				//Debug.Log ("Duplicate Sender address/port " + ipAdress + "/" + port + " ignored");
			} else {
				OSCHandler.Instance.CreateClient (clientName, IPAddress.Parse (ipAddress), port);
			}
			return clientName;
		}
		
		// Announce that a specified port will be used for receiving OSC messages.
		// Several ports are allowed, by calling this method with different port values.
		public static void ReceiverPort(int port) {
			OSC inst = OSC.Instance;
			Dictionary<string, ServerLog> servers = OSCHandler.Instance.Servers;
			string serverName = "OscServer-" + port;
			if (servers.ContainsKey(serverName)) {
				Debug.Log ("Duplicate Receiver port " + port + " ignored");
			} else {
				Debug.Log ("Receiver port " + port);
				OSCHandler.Instance.CreateServer (serverName, port);
			}
		}


		// Register a message handler for the specified OSC address.
		public static void OnReceive (String address, OSCMessageHandler handler) {
			OSC.Instance.onReceive (handler, address);
		}

		// The private message handler registering
		private void onReceive(OSCMessageHandler handler, string address) {
			if (! handlers.ContainsKey(address)){
				handlers.Add (address, new List<OSCMessageHandler>(4));
			}
			List<OSCMessageHandler> addressList ;
			handlers.TryGetValue(address, out addressList);
			addressList.Add(handler);
		}

//		// send a value using a specified sender name, and a specified OSC address.
//		public static void Send(String sender, String address, int value ) {
//			OSCHandler.Instance.SendMessageToClient<int> (sender, address, value);
//		}

		// send a value using a specified sender name, and a specified OSC address.
		public static void Send(String sender, String address, object value ) {
			OSCHandler.Instance.SendMessageToClient (sender, address, value);
		}

		// send a value for a specified OSC address, using all senders. 
		public static void Send(String address, object value ) {
			Dictionary<string, ClientLog> clients = OSCHandler.Instance.Clients;
			foreach (KeyValuePair<string, ClientLog> item in clients) {
				String clientId = item.Key;
			    OSCHandler.Instance.SendMessageToClient (clientId, address, value);
			}
		}

		// send a value using a specified sender name, and a specified OSC address.
		public static void Send(String sender, String address, List<object> valueList ) {
			OSCHandler.Instance.SendMessageToClient (sender, address, valueList);
		}

		// send a value for a specified OSC address, using all senders. 
		public static void Send(String address, List<object> valueList  ) {
			Dictionary<string, ClientLog> clients = OSCHandler.Instance.Clients;
			foreach (KeyValuePair<string, ClientLog> item in clients) {
				String clientId = item.Key;
				OSCHandler.Instance.SendMessageToClient (clientId, address, valueList);
			}
		}
	
		// Use this for initialization
		void Start () {
			Debug.Log ("OSC Start...");
			Application.runInBackground = true;// run even when losing focus	
		}



		// The main "loop" for handling messages
		void FixedUpdate () {
			if ( ! OSCHandler.Instance.hasData()) return;
			String oscAddress = OSCHandler.Instance.getPacketAddress();

			//Debug.Log ("oscAddress = " +oscAddress);
			List<OSCArgument> oscData = OSCHandler.Instance.getPacketData();

			OSCHandler.Instance.consumeData();

			if (handlers.ContainsKey(oscAddress)) {
				List<OSCMessageHandler> handlerList ;
				handlers.TryGetValue(oscAddress, out handlerList);
				foreach  (OSCMessageHandler handler in handlerList) {
					handler(oscData);
				}
			} else {
				Debug.Log ("No handlers found for " + oscAddress);
			}
		} 

	}
}

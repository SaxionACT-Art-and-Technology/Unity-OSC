//
//	  UnityOSC - Open Sound Control interface for the Unity3d game engine	  
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//
//	  Inspired by http://www.unifycommunity.com/wiki/index.php?title=AManagerClass

// This class has been modified by Job Zwiers, to circumvent the "logging" style for receiving messages.
// Basically OSCHandler keeps lists of senders/receiver, called "clients" and "servers", for dealing with OSC style messages.
// The original OSCHandler then relies of ServerLogs and ClientLogs for "logging" packets. 
// The addition circumvents these "logs", and defines a single packet address/packet data combo, 
// representing the most recently received OSC packet.
// A boolean "hasData" denotes whether this "latest packet data" has been "consumed" by the OSC user application. 
// While not consumed, newly arriving packets are currently being ignored. 

using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;
using UnityOSC;
using UnityEditor;

/// <summary>
/// Models a log of a server composed by an OSCServer, a List of OSCPacket and a List of
/// strings that represent the current messages in the log.
/// </summary>
public struct ServerLog
{
	public OSCServer server;
	public List<OSCPacket> packets;
	public List<string> log;
}

/// <summary>
/// Models a log of a client composed by an OSCClient, a List of OSCMessage and a List of
/// strings that represent the current messages in the log.
/// </summary>
public struct ClientLog
{
	public OSCClient client;
	public List<OSCMessage> messages;
	public List<string> log;
}

public delegate void OSCMessageHandler(List<OSCArgument> data);

/// <summary>
/// Handles all the OSC servers and clients of the current Unity game/application.
/// Tracks incoming and outgoing messages.
/// </summary>
public class OSCHandler : MonoBehaviour
{
	#region Singleton Constructors
	static OSCHandler()
	{
	}

	OSCHandler()
	{
	}

	public static OSCHandler Instance 
	{
	    get 
		{
	        if (_instance == null) 
			{
				_instance = new GameObject ("OSCHandler").AddComponent<OSCHandler>();
				GameObjectUtility.SetParentAndAlign(GameObject.Find("OSCHandler"), GameObject.Find("OSC"));
				DontDestroyOnLoad (_instance);
	        }
	       
	        return _instance;
	    }
	}
	#endregion
	
	#region Member Variables
	private static OSCHandler _instance = null;
	private Dictionary<string, ClientLog> _clients = new Dictionary<string, ClientLog>();
	private Dictionary<string, ServerLog> _servers = new Dictionary<string, ServerLog>();

	private string packetAddress;
	private List<OSCArgument> packetData;
	private bool hasPacketData = false;
	

	private const int _loglength = 25;
	#endregion

	// Default Init, with no parameters
	public void Init() {
	}



	
	#region Properties

	public Dictionary<string, ClientLog> Clients
	{
		get
		{
			return _clients;
		}
	}
	
	public Dictionary<string, ServerLog> Servers
	{
		get
		{
			return _servers;
		}
	}




	#endregion
	
	#region Methods

	// Returns the OSC address for the latest OSC package
	public  String getPacketAddress() {
		return OSCHandler.Instance.packetAddress;
	}

	// Returns the (converted) OSC data for the latest OSC package
	public  List<OSCArgument> getPacketData() {
		return OSCHandler.Instance.packetData;
	}

	// Denotes whether new OSC address/data is available.
	public  bool hasData() {
		return OSCHandler.Instance.hasPacketData;
	}

	// Used to signal that the current OSC address/data has been read and consumed.
	// After this call, OSC address/data will be received as soon as the remote app send new data.
	public  void consumeData() {
		OSCHandler.Instance.hasPacketData = false;
	}

	/// <summary>
	/// Ensure that the instance is destroyed when the game is stopped in the Unity editor
	/// Close all the OSC clients and servers
	/// </summary>
	void OnApplicationQuit() 
	{
		DestroyOSCconnections();
	}

	public void DestroyOSCconnections(){
		foreach(KeyValuePair<string,ClientLog> pair in _clients)
		{
			pair.Value.client.Close();
		}
		
		foreach(KeyValuePair<string,ServerLog> pair in _servers)
		{
			pair.Value.server.Close();
		}
		
		_instance = null;
	}
	
	/// <summary>
	/// Creates an OSC Client (sends OSC messages) given an outgoing port and address.
	/// </summary>
	/// <param name="clientId">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="destination">
	/// A <see cref="IPAddress"/>
	/// </param>
	/// <param name="port">
	/// A <see cref="System.Int32"/>
	/// </param>
	public void CreateClient(string clientId, IPAddress destination, int port)
	{
		ClientLog clientitem = new ClientLog();
		clientitem.client = new OSCClient(destination, port);
		clientitem.log = new List<string>();
		clientitem.messages = new List<OSCMessage>();
		
		_clients.Add(clientId, clientitem);
		
		// Send test message
		string testaddress = "/test/alive/";
		OSCMessage message = new OSCMessage(testaddress, destination.ToString());
		message.Append(port); message.Append("OK");
		
		_clients[clientId].log.Add(String.Concat(DateTime.UtcNow.ToString(),".",
		                                         FormatMilliseconds(DateTime.Now.Millisecond), " : ",
		                                         testaddress," ", DataToString(message.Data)));
		_clients[clientId].messages.Add(message);
		
		_clients[clientId].client.Send(message);
	}
	
	/// <summary>
	/// Creates an OSC Server (listens to upcoming OSC messages) given an incoming port.
	/// </summary>
	/// <param name="serverId">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="port">
	/// A <see cref="System.Int32"/>
	/// </param>
	public void CreateServer(string serverId, int port)
	{
        OSCServer server = new OSCServer(port);
        server.PacketReceivedEvent += OnPacketReceived;

        ServerLog serveritem = new ServerLog();
        serveritem.server = server;
		serveritem.log = new List<string>();
		serveritem.packets = new List<OSCPacket>();
		
		_servers.Add(serverId, serveritem);
	}

	static Type tint = typeof(int);
	static Type tfloat = typeof(float);
	static Type tstring = typeof(string);


    void OnPacketReceived(OSCServer server, OSCPacket packet)
    {
		//Debug.Log ("OnPacketReceived " + packet.Data[0].ToString ());

		if (hasPacketData) return; // previous data not yet consumed: skip new data
		string oscAddress = packet.Address;
		List<object> data = packet.Data;

		int listSize = data.Count;

		List<OSCArgument> oscData = new List<OSCArgument>(listSize);

		for (int i=0; i<listSize; i++) {
			object dataObject = data[i];
			Type ot = dataObject.GetType();
			if (ot == tint ) {
				//Debug.Log ("Int type");
				int io = (int) dataObject;
				OSCArgument oscArg = new OSCArgument();
				oscArg.intValue = io;
				oscData.Add (oscArg);
			} else if (ot == tfloat) {
				//Debug.Log ("Float type");
				float fo = (float) dataObject;
				OSCArgument oscArg = new OSCArgument();
				oscArg.floatValue = fo;
				oscData.Add (oscArg);
			} else if (ot == tstring) {
				//Debug.Log ("String type");
				string so = (string) dataObject;
				OSCArgument oscArg = new OSCArgument();
				oscArg.stringValue = so;
				oscData.Add (oscArg);
			} else  {
				Debug.Log ("OSCHandler: packet with unknown data Type: " + ot);
			}

		}
		packetData = oscData;
		packetAddress = oscAddress;
		hasPacketData = true;
    }
	
	/// <summary>
	/// Sends an OSC message to a specified client, given its clientId (defined at the OSC client construction),
	/// OSC address and a single value. Also updates the client log.
	/// </summary>
	/// <param name="clientId">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="address">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="value">
	/// A <see cref="T"/>
	/// </param>
	public void SendMessageToClient<T>(string clientId, string address, T value)
	{
		List<object> temp = new List<object>();
		temp.Add(value);
		
		SendMessageToClient(clientId, address, temp);
	}
	
	/// <summary>
	/// Sends an OSC message to a specified client, given its clientId (defined at the OSC client construction),
	/// OSC address and a list of values. Also updates the client log.
	/// </summary>
	/// <param name="clientId">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="address">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="values">
	/// A <see cref="List<T>"/>
	/// </param>
	public void SendMessageToClient<T>(string clientId, string address, List<T> values)
	{	
		if(_clients.ContainsKey(clientId))
		{
			OSCMessage message = new OSCMessage(address);
		
			foreach(T msgvalue in values)
			{
				message.Append(msgvalue);
			}
			
			if(_clients[clientId].log.Count < _loglength)
			{
				_clients[clientId].log.Add(String.Concat(DateTime.UtcNow.ToString(),".",
				                                         FormatMilliseconds(DateTime.Now.Millisecond),
				                                         " : ", address, " ", DataToString(message.Data)));
				_clients[clientId].messages.Add(message);
			}
			else
			{
				_clients[clientId].log.RemoveAt(0);
				_clients[clientId].messages.RemoveAt(0);
				
				_clients[clientId].log.Add(String.Concat(DateTime.UtcNow.ToString(),".",
				                                         FormatMilliseconds(DateTime.Now.Millisecond),
				                                         " : ", address, " ", DataToString(message.Data)));
				_clients[clientId].messages.Add(message);
			}
			
			_clients[clientId].client.Send(message);
		}
		else
		{
			Debug.LogError(string.Format("Can't send OSC messages to {0}. Client doesn't exist.", clientId));
		}
	}
	
	/// <summary>
	/// Updates clients and servers logs.
	/// </summary>
	public void UpdateLogs()
	{
		foreach(KeyValuePair<string,ServerLog> pair in _servers)
		{
			if(_servers[pair.Key].server.LastReceivedPacket != null)
			{
				//Initialization for the first packet received
				if(_servers[pair.Key].log.Count == 0)
				{	
					_servers[pair.Key].packets.Add(_servers[pair.Key].server.LastReceivedPacket);
						
					_servers[pair.Key].log.Add(String.Concat(DateTime.UtcNow.ToString(), ".",
					                                         FormatMilliseconds(DateTime.Now.Millisecond)," : ",
					                                         _servers[pair.Key].server.LastReceivedPacket.Address," ",
					                                         DataToString(_servers[pair.Key].server.LastReceivedPacket.Data)));
					break;
				}
						
				if(_servers[pair.Key].server.LastReceivedPacket.TimeStamp
				   != _servers[pair.Key].packets[_servers[pair.Key].packets.Count - 1].TimeStamp)
				{	
					if(_servers[pair.Key].log.Count > _loglength - 1)
					{
						_servers[pair.Key].log.RemoveAt(0);
						_servers[pair.Key].packets.RemoveAt(0);
					}
		
					_servers[pair.Key].packets.Add(_servers[pair.Key].server.LastReceivedPacket);
						
					_servers[pair.Key].log.Add(String.Concat(DateTime.UtcNow.ToString(), ".",
					                                         FormatMilliseconds(DateTime.Now.Millisecond)," : ",
					                                         _servers[pair.Key].server.LastReceivedPacket.Address," ",
					                                         DataToString(_servers[pair.Key].server.LastReceivedPacket.Data)));
				}
			}
		}
	}
	
	/// <summary>
	/// Converts a collection of object values to a concatenated string.
	/// </summary>
	/// <param name="data">
	/// A <see cref="List<System.Object>"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	private string DataToString(List<object> data)
	{
		string buffer = "";
		
		for(int i = 0; i < data.Count; i++)
		{
			buffer += data[i].ToString() + " ";
		}
		
		buffer += "\n";
		
		return buffer;
	}
	
	/// <summary>
	/// Formats a milliseconds number to a 000 format. E.g. given 50, it outputs 050. Given 5, it outputs 005
	/// </summary>
	/// <param name="milliseconds">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	private string FormatMilliseconds(int milliseconds)
	{	
		if(milliseconds < 100)
		{
			if(milliseconds < 10)
				return String.Concat("00",milliseconds.ToString());
			
			return String.Concat("0",milliseconds.ToString());
		}
		
		return milliseconds.ToString();
	}
			
	#endregion
}	


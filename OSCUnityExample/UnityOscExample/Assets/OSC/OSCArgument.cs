//
// Author: Job Zwiers
// This is an addition to the OSC package by orge Garcia Martin
// OSCArgument objects represent a single data element received via OSC messages. 
// Bascically each OSCArgument contains either an int, a float, or a string, that can be retrieved
// via properties "intValue", "floatValue", or "stringValue". 
using System;
namespace UnityOSC {
	public class OSCArgument {
		string s;
		int i;
		float f;
		// The int value property.
		public int intValue {
			get {
				return i;
			}
			set {
				i = value;
			}
			
		}
		// the float value property
		public float floatValue {
			get {
				return f;
			}
			set {
				f = value;
			}
			
		}
		// the string value property
		public String stringValue {
			get {
				return s;
			}
			set {
				s = value;
			}
			
		}
	
		// Some redundant setter/getter method, for compatibility with Processing oscP5 interface
		public void setStringValue(string s) { this.s = s; }
		public void setIntValue(int i) { this.i = i; }
		public void setFloatValue(float f) { this.floatValue = f; }
		public string getStringValue() { return s; }
		public int getIntValue() { return i; }
		public float getFloatValue() { return floatValue; } 
	}
}

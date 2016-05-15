/*
  Simple example, based on the examples from the oscP5 library. 
  
  Make sure you import the library. 
  oscP5 website at http://www.sojamo.de/oscP5
*/
 
import oscP5.*;
import netP5.*;

OscP5 oscP5;
NetAddress myRemoteLocation;

void setup() {
  size(400,400);
  frameRate(25);
  
  // start oscP5, listening for incoming messages at port 9000 
  // Make sure Unity sends to port 9000
  oscP5 = new OscP5(this,9000);
  
  /* myRemoteLocation is a NetAddress. a NetAddress takes 2 parameters,
   * an ip address and a port number. myRemoteLocation is used as parameter in
   * oscP5.send() when sending osc packets to another computer, device, 
   * application. usage see below. for testing purposes the listening port
   * and the port of the remote location address are the same, hence you will
   * send messages back to this sketch.
   */
   
  // the adress were we are sending to. 127.0.0.1 is localhost (our own computer). 
  // port 8000 is the port where we send to. Make sure Unity listens to that port. 
  myRemoteLocation = new NetAddress("127.0.0.1",8000);
  
  /* osc plug service
   * osc messages with a specific address pattern can be automatically
   * forwarded to a specific method of an object. in this example 
   * a message with address pattern /test will be forwarded to a method
   * test(). below the method test takes 2 arguments - 2 ints. therefore each
   * message with address pattern /test and typetag ii will be forwarded to
   * the method test(int theA, int theB)
   */
  // not used now
  //oscP5.plug(this,"test","/test");
}

void draw() {
  background(0);  
}

void mouseMoved() {
  /* in the following different ways of creating osc messages are shown by example */
  OscMessage myMessage = new OscMessage("/processing/mouse/");
  
  //myMessage.add(123); /* add an int to the osc message */
  //myMessage.add(12.34); /* add a float to the osc message */
  //myMessage.add("some text"); /* add a string to the osc message */
  
  // mouseX as value between 0.0 - 1.0 
  float mouseXNormalized = mouseX/float(width);
  // mouseY as value betwen 0.0 - 1.0
  float mouseYNormalized = mouseY/float(height);
  
  // send an array of two floats
  myMessage.add(new float[] { mouseXNormalized, mouseYNormalized}); /* add a float array to the osc message */

  myMessage.add("Example 1");
  /* send the message */
  oscP5.send(myMessage, myRemoteLocation); 
  myMessage.print();
}


/* incoming osc message are forwarded to the oscEvent method. */
// also check if the message is plugged (directly uses a call to a function
void oscEvent(OscMessage m) {
  
  m.print();
  
  /*
  if(theOscMessage.isPlugged()==false) {
   // print the address pattern and the typetag of the received OscMessage
    println("### received an osc message.");
    println("### addrpattern\t"+m.addrPattern());
    println("### typetag\t"+m.typetag());
  }
  
  if(m.typetag()== 'i') // message has integer
  { float bla = m.get(i).intValue();    
  }
  else if(m.typetag()== 'f') // message has float
  { float bla = m.get(i).floatValue();
  }
  
  */
}

// incase you want to use the plug functionality
// can be handy
// public void test(int theA, int theB) {
//   println("### plug event method. received a message /test.");
//   println(" 2 ints received: "+theA+", "+theB);  
// }
/*
Face Detector:
- including mirror effect (flip)
- track the position of the biggest face

Install the OpenCV for Processing library (Sketch > Import library):
https://github.com/atduskgreg/opencv-processing

kasperkamperman.com - 27-09-2015
*/

 
import oscP5.*;
import netP5.*;

OscP5 oscP5;
NetAddress myRemoteLocation;

import processing.video.*;
import gab.opencv.*;

Capture cam;

OpenCV camFlip; 

FaceDetector faceDetect;

boolean faceIsOn = false;

void setup() {
  
  size(800,480);
  frameRate(25);
  
  // if you would like to select a specific camera our resolution
  // String[] cameras = Capture.list();
  // println(cameras);
  // cam = new Capture(this, cameras[0]);
  
  cam = new Capture(this, 640, 480);
  cam.start();  
  
  camFlip = new OpenCV( this, cam.width, cam.height); 
  camFlip.useColor();  
  
  faceDetect = new FaceDetector(this, cam.width, cam.height, 4);
  // start oscP5, listening for incoming messages at port 9000 
  // Make sure Unity sends to port 9000
  oscP5 = new OscP5(this,9000);
  
  // the adress were we are sending to. 127.0.0.1 is localhost (our own computer). 
  // port 8000 is the port where we send to. Make sure Unity listens to that port. 
  myRemoteLocation = new NetAddress("127.0.0.1",8000);
  

}

void draw() {
  
  // read the cam frame and detect the faces
  if (cam.available()) {
    cam.read();
    camFlip.loadImage(cam);
    camFlip.flip(OpenCV.HORIZONTAL); 
    
    faceDetect.processImage(camFlip.getOutput());
    
    // we use nf rounds the values to 3 decimals
    if(faceDetect.hasFace) {
      faceIsOn = true;
      
      //print("faceXFactor: "    + nf(faceDetect.faceXFactor, 1, 3) + ", ");
      //print("faceYFactor: "    + nf(faceDetect.faceYFactor, 1, 3) + ", ");
      //print("faceSizeFactor: " + nf(faceDetect.faceSizeFactor, 1, 3)    );
      //println();
      /* in the following different ways of creating osc messages are shown by example */
      OscMessage myMessage = new OscMessage("/processing/face/");
      myMessage.add(new float[] { faceDetect.faceXFactor, faceDetect.faceYFactor}); /* add a float array to the osc message */
    
      myMessage.add("FaceExample");
      /* send the message */
      oscP5.send(myMessage, myRemoteLocation); 
      myMessage.print();
     } else if (faceIsOn == true){
        faceIsOn = false;
        OscMessage myMessage = new OscMessage("/processing/faceOff/");
        myMessage.add("FaceOffforIdleMode");
        /* send the message */
        oscP5.send(myMessage, myRemoteLocation); 
        myMessage.print();     
    }
  }
  
  // show things on the screen
  
  background(0);
  
  faceDetect.displayOpenCVOutput(640, 0);
  
  image(camFlip.getOutput(), 0, 0 ); // show the flipped color image  
  faceDetect.displayDetectionInfo();
  
  showPositionAndSizeSliders();  
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
}
 

void showPositionAndSizeSliders() {
  
   fill(192);
   stroke(0);
   strokeWeight(1);
  
   // draw a triangle as an arrow on the bottom of the screen for the X
   triangle(  (faceDetect.faceXFactor * width)-10, (height - 10), 
              (faceDetect.faceXFactor * width)   , (height - 20), 
              (faceDetect.faceXFactor * width)+10, (height - 10));
               
   // draw a triangle as an arrow on the right of the screen for the Y
   triangle(  width-10, (faceDetect.faceYFactor * height) - 10, 
              width-20, (faceDetect.faceYFactor * height), 
              width-10, (faceDetect.faceYFactor * height) + 10);
               
   // draw an ellipse on the top to represent the size
   ellipse((faceDetect.faceSizeFactor * width), 10, 10,10);
   
}
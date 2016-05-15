/*

One OpenCV object is used to flip the image (so webcam acts like a mirror). 
The other is used to detect Faces on a smaller image (cvDivider) for speed. 

Read more about Face Detection (and Recognition)
http://www.shervinemami.info/faceRecognition.html

Check also the reference:
http://atduskgreg.github.io/opencv-processing/reference/

*/

import java.awt.Rectangle;

class FaceDetector {
  
  OpenCV cv;           // for face detection
  Rectangle[] faces;   // store face rectangles
  
  int divider; 
  
  PImage detectionImg; // in general a smaller image for detection
  
  // normalized values (0.0 - 1.0) to use to control elements
  
  // if there is no Face we gradually (depends on smoothFactor) return to these defaults.
  float faceXFactorDefault    = 0.5;
  float faceYFactorDefault    = 0.5;
  float faceSizeFactorDefault = 0.0;
  
  // in these variables the current face position is stored. 
  float faceXFactor    = faceXFactorDefault;  
  float faceYFactor    = faceYFactorDefault; 
  float faceSizeFactor = faceSizeFactorDefault;
  
  // for smoothing we store the previous value
  float lastFaceXFactor    = faceXFactorDefault;     
  float lastFaceYFactor    = faceYFactorDefault;     
  float lastFaceSizeFactor = faceSizeFactorDefault;  
  
  // 15% of the current value, (100%-15%) of the last
  float smoothFactor = 0.15;        
  
  // boolean that changes based on the fact that we have a face or not
  boolean hasFace = false;

  FaceDetector(PApplet p, float tempWidth, float tempHeight, int tempDivider) {
    
    divider = tempDivider;
    
    cv = new OpenCV(p, int(tempWidth/divider), int(tempHeight/divider));
    
    // Load HAAR cascade to detect a feature. 
    // We can use the constants from the library or you can load some alternative 
    // (see data folder in this sketch) HAAR cascade.
    cv.loadCascade(OpenCV.CASCADE_FRONTALFACE);  // detect faces
    //cv.loadCascade(dataPath("haarcascade_frontalface_alt.xml", true);
    
    faces = cv.detect();
    
    detectionImg = createImage(cv.width, cv.height, RGB);
  }
  
  void processImage(PImage video) {
    
    // copy and scale the video image to the detectionImg
    detectionImg.copy(video, 0, 0, video.width, video.height, 0, 0, cv.width, cv.height);
    
    // load the detectionImg into the cv object for detection
    cv.loadImage(detectionImg);
    
    // detect faces (or other configured cascade elements)
    faces = cv.detect();  
    
    if(faces.length > 0) hasFace = true;
    else                 hasFace = false;
    
    calculateNormalizedValues();
    
  }
  
  void displayOpenCVOutput(int xPos, int yPos) {
        
    image( cv.getOutput(), xPos, yPos);      // show the detection image
    
    // draw rectangle around the faces in the detection image
    noFill();
    strokeWeight(1);
    stroke(0, 255, 0);
    for (int i = 0; i < faces.length; i++) {
      rect(xPos+faces[i].x, yPos+faces[i].y, faces[i].width, faces[i].height);
    } 
    
  }
  
  void displayDetectionInfo() {
    
    noFill();
    strokeWeight(2);
    textSize(32);
    textAlign(LEFT, TOP);
    stroke(255, 0, 0);
    
    for (int i = 0; i < faces.length; i++) {
    
      // scale it up with the divider
      int scaledFaceX    = faces[i].x * divider;
      int scaledFaceY    = faces[i].y * divider;
      // it's always a square so width/height are equal
      int scaledFaceSize = faces[i].width * divider; 
      
      int halfFaceSize  = (scaledFaceSize/2);
      int faceXmiddle   = scaledFaceX + halfFaceSize;
      int faceYmiddle   = scaledFaceY + halfFaceSize;
      
      rect(scaledFaceX, scaledFaceY, scaledFaceSize, scaledFaceSize);  // show rectangle
      ellipse(faceXmiddle, faceYmiddle, 10, 10);                       // draw center
      text(i, scaledFaceX+4, scaledFaceY);                             // show index
    }
    
  }
  
  void calculateNormalizedValues() {
    
    // the biggest face has always highest index.
    // we only would like to use the biggest face (so probaly most in front of the cam).
    // we can do this with faces[faces.length-1] and a check to see if faces.length > 0
    
    if(hasFace) {
        
      Rectangle biggestFace = faces[faces.length-1];
      
      int halfFaceSize  = biggestFace.width/2;
      int faceXmiddle   = biggestFace.x + halfFaceSize;
      int faceYmiddle   = biggestFace.y + halfFaceSize;
      
      // relative position on screen between 0.0 - 1.0 (0 and 100%)
      faceXFactor = map(faceXmiddle, 0 + halfFaceSize, cv.width - halfFaceSize, 0.0, 1.0); 
      faceYFactor = map(faceYmiddle, 0 + halfFaceSize, cv.height - halfFaceSize, 0.0, 1.0); 
      
      // minimal faceSize of 20 is a guess, compare with height, since that is shorter
      // -16 because we can't get the full height
      faceSizeFactor = map(biggestFace.width, 20, cv.height-24, 0.0, 1.0); 
      faceSizeFactor = constrain(faceSizeFactor,0.0,1.0);
      
    }
    else {
      
       faceXFactor    = faceXFactorDefault;  
       faceYFactor    = faceYFactorDefault; 
       faceSizeFactor = faceSizeFactorDefault;
       
    }
    
    // smooth
    faceXFactor = (smoothFactor * faceXFactor) + ((1-smoothFactor) * lastFaceXFactor); 
    lastFaceXFactor = faceXFactor;
    
    faceYFactor = (smoothFactor * faceYFactor) + ((1-smoothFactor) * lastFaceYFactor); 
    lastFaceYFactor = faceYFactor;
    
    faceSizeFactor = (smoothFactor * faceSizeFactor) + ((1-smoothFactor) * lastFaceSizeFactor); 
    lastFaceSizeFactor = faceSizeFactor;
    
  }
  
}
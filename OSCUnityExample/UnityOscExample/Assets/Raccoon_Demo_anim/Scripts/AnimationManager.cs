using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {

	private Animator animator;
	private OSCFaceAnimation OSCFace;

	private int animationState = 3;
	private float oldXPosition;



	void Awake () {
		animator = GetComponent<Animator> ();
		OSCFace = GetComponent<OSCFaceAnimation>();
		oldXPosition = transform.position.x;

	}
	
	// Update is called once per frame
	void Update () {
		float xAbsPosition =  Mathf.Abs(transform.position.x);
		if (OSCFace.faceOff){
			animationState = 3;
		}else{

			if(xAbsPosition > 1){
				animationState = 2;
			}
			else if (xAbsPosition > 0.02f){
				animationState = 1;
			}
			else{
				animationState = 0;
			}

		}

		if(transform.position.x - oldXPosition < - 0.02){
			transform.localScale = new Vector3(-0.1f,0.1f,0.1f);
			oldXPosition = transform.position.x;
		}else if(transform.position.x - oldXPosition > 0.02){
			transform.localScale = new Vector3(0.1f,0.1f,.01f);
			oldXPosition = transform.position.x;
		}

		animator.SetInteger("AnimationState", animationState);
	}
}

// @takashicompany : http://takashicompany.com/
// TouchManager ver1.0.0 2013/11/26

using UnityEngine;
using System.Collections;

public class TouchManagerSample : MonoBehaviour {

	// attack inspector.
	public Camera touchCamera;

	void Awake () {

	}

	void OnEnable () {

		TouchManager.OnTouch += Touch;
		TouchManager.OnPress += Press;
		TouchManager.OnSwipe += Swipe;
		TouchManager.OnTouchEnd += TouchEnd;

		TouchManager.OnLongPress += LongPress;

		TouchManager.OnTap += Tap;
		TouchManager.OnLongTap += LongTap;
		TouchManager.OnSwipeEnd += SwipeEnd;

	}

	private void Touch(Vector2 position){
		Debug.Log("Touch position:" + position);
		Particle(position);
	}

	private void Press(Vector2 position,float pressTime){
		Debug.Log("Press position:" + position + " pressTime:" + pressTime); 
	}

	private void Swipe(Vector2 position,Vector2 startPosition,float swipeTime){
		Debug.Log("Swipe position:" + position + " startPosition:" + startPosition + " swipeTime:" + swipeTime);
	}

	private void TouchEnd(Vector2 position){
		Debug.Log("TouchEnd position:" + position);
	}

	private void LongPress(Vector2 position,float pressTime){
		Debug.Log("LongPress position:" + position + " pressTime:" + pressTime);
	}

	private void Tap(Vector2 position){
		Debug.Log("Tap position:" + position);
	}

	private void LongTap(Vector2 position,float tapTime){
		Debug.Log("LongTap position:" + position + " tapTime:" + tapTime);
	}

	private void SwipeEnd(Vector2 position,Vector2 startPosition,float swipeTime){
		Debug.Log("SwipeEnd position:" + position + " startPosition:" + startPosition + " swipeTime:" + swipeTime);
	}

	private void Particle(Vector2 position){
		ParticleSystem particle = Instantiate(Resources.Load<ParticleSystem>("TouchParticle")) as ParticleSystem;
		Vector3 worldPos = touchCamera.ScreenToWorldPoint(new Vector3(position.x,position.y,0f));
		Vector3 particlePos = new Vector3(worldPos.x,worldPos.y,0f);
		particle.transform.position = particlePos;
		Destroy(particle.gameObject,particle.startLifetime);
	}
}

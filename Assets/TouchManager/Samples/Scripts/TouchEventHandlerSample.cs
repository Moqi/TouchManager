using UnityEngine;
using System.Collections;

public class TouchEventHandlerSample : MonoBehaviour {

	public TouchEventHandler eventHandler;

	void Awake () {
		if(eventHandler == null){
			eventHandler = GetComponent<TouchEventHandler>();
		}
	}

	void OnEnable () {
		eventHandler.OnTouch += Touch;
		eventHandler.OnSwipeEnter += SwipeEnter;
		eventHandler.OnSwipeStay += SwipeStay;
		eventHandler.OnSwipeExit += SwipeExit;
	}

	void OnDisable () {
		eventHandler.OnTouch -= Touch;
		eventHandler.OnSwipeEnter -= SwipeEnter;
		eventHandler.OnSwipeStay -= SwipeStay;
		eventHandler.OnSwipeExit -= SwipeExit;
	}


	private void Touch(GameObject obj){
		Debug.Log("Touch:" + obj.name);
	}

	private void SwipeEnter(GameObject obj){
		Debug.Log("SwipeEnter:" + obj.name);
	}

	private void SwipeStay(GameObject obj){
		Debug.Log("SwipeStay:" + obj.name);
	}

	private void SwipeExit(GameObject obj){
		Debug.LogError("SwipeExit:" + obj.name);
	}
}

// @takashicompany : http://takashicompany.com/
// TouchManager ver1.1.0 2013/11/30

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchEventHandler : MonoBehaviour {

	public delegate void TouchDelegate(GameObject obj);
	public event TouchDelegate OnTouch;

	public Camera touchCamera;

	private Dictionary<GameObject,TouchEventType> _currentEvent;
	private Dictionary<GameObject,TouchEventType> _prevEvent;

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

	void OnDisable () {
		TouchManager.OnTouch -= Touch;
		TouchManager.OnPress -= Press;
		TouchManager.OnSwipe -= Swipe;
		TouchManager.OnTouchEnd -= TouchEnd;
		TouchManager.OnLongPress -= LongPress;
		TouchManager.OnTap -= Tap;
		TouchManager.OnLongTap -= LongTap;
		TouchManager.OnSwipeEnd -= SwipeEnd;
	}

	void Update () {

	}

	void LastUpdate () {
		_prevEvent = _currentEvent;
		_currentEvent = new Dictionary<GameObject, TouchEventType>();
	}


	private void Touch (Vector2 position) {

	}

	private void Press (Vector2 position, float pressTime) {

	}

	private void Swipe (Vector2 position, Vector2 startPosition, float swipeTime) {

	}

	private void TouchEnd (Vector2 position) {

	}

	private void LongPress (Vector2 position, float pressTime) {

	}

	private void Tap (Vector2 position) {

	}

	private void LongTap (Vector2 position, float tapTime) {

	}

	private void SwipeEnd (Vector2 position,Vector2 startPosition,float swipeTime) {
	
	}

	private void ProcessTouchEvent (TouchEventType type, GameObject obj){
		if(obj == null) return;

		if(_currentEvent.ContainsKey(obj)){
			// TODO すでに現在のフレームで別のタッチポイントがオブジェクトを指定した時の処理を書く。
		}else{
			if(_currentEvent == null){
				_currentEvent = new Dictionary<GameObject, TouchEventType>();
			}
			_currentEvent.Add(obj,type);
		}

	}

	private GameObject GetRayCastHit (Vector2 position){

		Ray ray = touchCamera.ScreenPointToRay(position);
		RaycastHit hit;
		RaycastHit2D hit2D;
		GameObject obj3D = null;
		GameObject obj2D = null;

		// 3D
		if(Physics.Raycast(ray,out hit)){
			obj3D= hit.collider.gameObject;
		}

		// 2D
		hit2D = Physics2D.Raycast(ray.origin,ray.direction);
		if(hit2D != null){
			obj2D = hit2D.collider.gameObject;
		}

		if(obj3D != null && obj2D != null){
			float distance3D = Vector3.Distance(touchCamera.transform.position,obj3D.transform.position);
			float distance2D = Vector2.Distance(touchCamera.transform.position,obj2D.transform.position);
			return distance2D < distance3D ? obj3D : obj2D;
		}else if(obj3D != null && obj2D == null){
			return obj3D;
		}else if(obj3D == null && obj2D != null){
			return obj2D;
		}
		return null;
	}
}

internal class TouchEventObject {

	public TouchEventType eventType{get; private set;}
	public GameObject gameObject{get; private set;}

	public TouchEventObject (TouchEventType argType, GameObject argGameObject){
		eventType = argType;
		gameObject = argGameObject;
	}
}

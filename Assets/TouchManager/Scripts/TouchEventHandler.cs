// @takashicompany : http://takashicompany.com/
// TouchManager ver1.1.0 2013/11/30

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TouchEventHandlerType{
	NONE,
	TOUCH,
	SWIPE_ENTER,
	SWIPE_STAY,
	SWIPE_EXIT,
}

public class TouchEventHandler : MonoBehaviour {

	public delegate void TouchDelegate(GameObject obj);
	public event TouchDelegate OnTouch;

	public delegate void SwipeEnterDelegate(GameObject obj, Vector3 touchWorldPosition);
	public event SwipeEnterDelegate OnSwipeEnter;

	public delegate void SwipeStayDelegate(GameObject obj, Vector3 touchWorldPosition);
	public event SwipeStayDelegate OnSwipeStay;

	public delegate void SwipeExitDelegate(GameObject obj);
	public event SwipeExitDelegate OnSwipeExit;

	public event TouchDelegate OnLongPress;

	public event TouchDelegate OnTap;
	public event TouchDelegate OnLongTap;

	public Camera touchCamera;

	private Dictionary<GameObject,TouchEventHandlerType> _currentEventDict;
	private Dictionary<GameObject,TouchEventHandlerType> _prevEventDict;

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

	void LateUpdate () {
		ProcessAfterTouchEvent();
		_prevEventDict = _currentEventDict;
		_currentEventDict = new Dictionary<GameObject, TouchEventHandlerType>();
	}


	private void Touch (Vector2 position) {
		ProcessTouchEvent(TouchEventType.TOUCH,GetRaycastHit(position));
	}

	private void Press (Vector2 position, float pressTime) {
		ProcessTouchEvent(TouchEventType.PRESS,GetRaycastHit(position));
	}

	private void Swipe (Vector2 position, Vector2 startPosition, float swipeTime) {
		ProcessTouchEvent(TouchEventType.SWIPE,GetRaycastHit(position));
	}

	private void TouchEnd (Vector2 position) {
		ProcessTouchEvent(TouchEventType.TOUCH_END,GetRaycastHit(position));
	}

	private void LongPress (Vector2 position, float pressTime) {
		ProcessTouchEvent(TouchEventType.LONG_PRESS,GetRaycastHit(position));
	}

	private void Tap (Vector2 position) {
		ProcessTouchEvent(TouchEventType.TAP,GetRaycastHit(position));
	}

	private void LongTap (Vector2 position, float tapTime) {
		ProcessTouchEvent(TouchEventType.LONG_TAP,GetRaycastHit(position));
	}

	private void SwipeEnd (Vector2 position,Vector2 startPosition,float swipeTime) {
		ProcessTouchEvent(TouchEventType.SWIPE_END,GetRaycastHit(position));
	}

	private void ProcessTouchEvent (TouchEventType touchEventType, TouchedObject touchedObject){
		if(touchedObject == null) return;
		GameObject obj = touchedObject.gameObject;
		TouchEventHandlerType prevHandlerEventType = TouchEventHandlerType.NONE;

		if(_prevEventDict != null && _prevEventDict.ContainsKey(obj)){
			prevHandlerEventType = _prevEventDict[obj];
		}

		if(_currentEventDict != null && _currentEventDict.ContainsKey(obj)){
			// TODO すでに現在のフレームで別のタッチポイントがオブジェクトを指定した時の処理を書く。
		}else{
			if(_currentEventDict == null){
				_currentEventDict = new Dictionary<GameObject, TouchEventHandlerType>();
			}
		}

		switch(touchEventType){
		case TouchEventType.TOUCH:{
			_currentEventDict[obj] = TouchEventHandlerType.TOUCH;
			if(OnTouch != null) OnTouch(obj);
		}break;

		case TouchEventType.SWIPE:{
			if(prevHandlerEventType == TouchEventHandlerType.SWIPE_ENTER || prevHandlerEventType == TouchEventHandlerType.SWIPE_STAY){
				_currentEventDict[obj] = TouchEventHandlerType.SWIPE_STAY;
				if(OnSwipeStay != null) OnSwipeStay(obj,touchedObject.raycastHitPosition);
			}else{
				_currentEventDict[obj] = TouchEventHandlerType.SWIPE_ENTER;
				if(OnSwipeEnter != null) OnSwipeEnter(obj,touchedObject.raycastHitPosition);
			}
		}break;

		}

	}

	private void ProcessAfterTouchEvent () {
		if(_prevEventDict != null){
			foreach(GameObject obj in _prevEventDict.Keys){
				if(!_currentEventDict.ContainsKey(obj)){
					// 現在のフレームで処理をされなかったものに対して処理
					TouchEventHandlerType prevHandlerEventType = _prevEventDict[obj];

					switch(prevHandlerEventType){
					case TouchEventHandlerType.SWIPE_ENTER:{
						if(OnSwipeExit != null) OnSwipeExit(obj);
					}break;

					case TouchEventHandlerType.SWIPE_STAY:{
						if(OnSwipeExit != null) OnSwipeExit(obj);
					}break;

					}

				}
			}
		}
	}

	private TouchedObject GetRaycastHit (Vector2 position){

		Ray ray = touchCamera.ScreenPointToRay(position);
		RaycastHit hit;
		Collider2D hitCollider2D;
	
		TouchedObject hit3D = null;
		TouchedObject hit2D = null;
	
		// 3D
		if(Physics.Raycast(ray,out hit)){
			hit3D = new TouchedObject(hit.collider.gameObject,hit.point);
		}

		// 2D
		Vector3 touchWorldPos = touchCamera.ScreenToWorldPoint(position);
		hitCollider2D = Physics2D.OverlapPoint((Vector2)touchWorldPos);
		if(hitCollider2D != null){
			Vector3 hitPos = new Vector3(touchWorldPos.x,touchWorldPos.y,hitCollider2D.gameObject.transform.position.z);
			hit2D = new TouchedObject(hitCollider2D.gameObject,hitPos);
		}

		if(hit3D != null && hit2D != null){
			float distance3D = Vector3.Distance(touchCamera.transform.position,hit3D.gameObject.transform.position);
			float distance2D = Vector2.Distance(touchCamera.transform.position,hit2D.gameObject.transform.position);
			return distance2D < distance3D ? hit2D : hit3D;
		}else if(hit3D != null && hit2D == null){
			return hit3D;
		}else if(hit3D == null && hit2D != null){
			return hit2D;
		}
		return null;
	}

	private class TouchedObject{
		public GameObject gameObject{get; private set;}
		public Vector3 raycastHitPosition{get; private set;}
		public TouchedObject(GameObject gameObject,Vector3 raycastHitPosition){
			this.gameObject = gameObject;
			this.raycastHitPosition = raycastHitPosition;
		}
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

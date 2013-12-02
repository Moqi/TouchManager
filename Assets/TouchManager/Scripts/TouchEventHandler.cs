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

	public delegate void SwipeDelegate(GameObject obj, Vector3 contactPosition);

	public event SwipeDelegate OnSwipeEnter;
	public event SwipeDelegate OnSwipeStay;
	public event SwipeDelegate OnSwipeExit;

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

	private void ProcessTouchEvent (TouchEventType touchEventType, GameObject obj){
		if(obj == null) return;

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
				if(OnSwipeStay != null) OnSwipeStay(obj);
			}else{
				_currentEventDict[obj] = TouchEventHandlerType.SWIPE_ENTER;
				if(OnSwipeEnter != null) OnSwipeEnter(obj);
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

	private GameObject GetRaycastHit (Vector2 position){

		Ray ray = touchCamera.ScreenPointToRay(position);
		RaycastHit hit;
		//RaycastHit2D hit2D;
		Collider2D collider2D;
		GameObject obj3D = null;
		GameObject obj2D = null;

		// 3D
		if(Physics.Raycast(ray,out hit)){
			obj3D= hit.collider.gameObject;
		}

		// 2D
		collider2D = Physics2D.OverlapPoint((Vector2)touchCamera.ScreenToWorldPoint(position));
		if(collider2D != null){
			obj2D = collider2D.gameObject;
		}
		/* なんでかよくわからないけど、null pointer が返ってくるのでコメントアウト : Physics2D.OverlapPointを使う実装に変更
		hit2D = Physics2D.Raycast(ray.origin,ray.direction);
		if(hit2D != null){
			obj2D = hit2D.collider.gameObject;
		}
		*/

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

// @takashicompany : http://takashicompany.com/
// TouchManager ver1.0.0 2013/11/26

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TakashiCompany.Unity.TouchManager;

internal enum MouseButtonEventType{
	NONE,
	DOWN,	// Input.GetMouseButton
	CLICK,	// Input.GetMouseButtonDown
	UP		// Input.GetMouseButtonUp
}

internal enum TouchEventType{
	NONE,
	TOUCH,
	PRESS,
	TAP,
	SWIPE,
	END
}

/// <summary>
/// デバイスのタッチ入力を管理するクラス。
/// タッチ情報を取得して、デリゲートイベントを発行する。
/// マウス入力をタッチ入力に変換する処理も実装している。
/// </summary>
public class TouchManager : MonoBehaviour {
	
	public delegate void TouchDelegate(Vector2 position);
	/// <summary>
	/// 画面をタッチした時に呼ばれるイベント。
	/// </summary>
	public static event TouchDelegate OnTouch;

	public delegate void TouchPressDelegate(Vector2 position, float pressTime);
	/// <summary>
	/// 画面をタッチし続けた時に呼ばれるイベント。
	/// </summary>
	public static event TouchPressDelegate OnPress;

	public delegate void TouchSwipeDelegate(Vector2 position,Vector2 startPosition,float swipeTime);
	/// <summary>
	/// 画面上で指を移動させた時に呼ばれるイベント。
	/// </summary>
	public static event TouchSwipeDelegate OnSwipe;
	
	public delegate void TouchEndDelegate(Vector2 position);
	/// <summary>
	/// 画面上から指を離した時に呼ばれるイベント。
	/// </summary>
	public static event TouchEndDelegate OnTouchEnd; 


	public delegate void LongPressDelegate(Vector2 position, float pressTime);
	/// <summary>
	/// 指定の時間以上、画面をタッチし続けた時に一度だけ呼ばれるイベント。
	/// </summary>
	public static event LongPressDelegate OnLongPress;

	public delegate void TapDelegate(Vector2 position);
	/// <summary>
	/// 指定の時間内で、画面をタップした際に呼ばれるイベント。
	/// </summary>
	public static event TapDelegate OnTap;

	public delegate void LongTapDelegate(Vector2 position, float tapTime);	
	/// <summary>
	/// 指定の時間より長い時間に画面をタップした際に呼ばれるイベント。
	/// </summary>
	public static event LongTapDelegate OnLongTap;

	public delegate void SwipeEndDelegate(Vector2 position,Vector2 startPosition,float swipeTime);
	/// <summary>
	/// スワイプが終了された時に呼ばれるイベント。
	/// </summary>
	public static event SwipeEndDelegate OnSwipeEnd;


	// Setting Params
	public static float longPressTime = 1f;

	// Singleton Instance
	private static TouchManager _instance;

	// Runtime Values
	private Dictionary<int,List<CustomTouch>> _touchListDict;
	private Dictionary<int,TouchEvent> _prevTouchEventDict;

	private bool _isTouchThisFrame = false;
	private int _mouseClickCount = 0;
	private bool _isPrevMouseInput;
	private Vector2 _prevMousePosition;
	private float _prevMouseTime;



	void Awake () {
		if(_instance == null){
			_instance = this;
		}else{
			Destroy(this);
		}
	}

	void Update () {

		_isTouchThisFrame = false;

		if(0 < Input.touchCount){
			ProcessTouches(TouchesToCustomTouches(Input.touches));
		}

		//マウスイベント
		bool isMouse = false;
		bool isMultiMouse = Input.GetKey(KeyCode.LeftShift);
		MouseButtonEventType mouseEvent = MouseButtonEventType.NONE;


		if(Input.GetMouseButtonDown(0)){
			//特定のマウスの情報を取得する。押された瞬間、trueを返す。
			isMouse = true;
			mouseEvent = MouseButtonEventType.CLICK;
		}else if(Input.GetMouseButton(0)){ 
			//特定のマウスの情報を取得する。押している間、trueを返す。
			isMouse = true;
			mouseEvent = MouseButtonEventType.DOWN;
		}else if(Input.GetMouseButtonUp(0)){
			//特定のマウスの情報を取得する。ボタンから指を話したとき、trueを返す
			isMouse = true;
			mouseEvent = MouseButtonEventType.UP;
		}

		if(isMouse){
			CustomTouch touch = MouseToCustomTouch(Input.mousePosition,mouseEvent);
			CustomTouch[] touches;
			if(isMultiMouse){
				CustomTouch rTouch = MouseToCustomTouch(Input.mousePosition,mouseEvent);
				rTouch.position = new Vector2( Screen.width / 2 + (Screen.width / 2 - touch.position.x) , Screen.height / 2 + (Screen.height / 2 - touch.position.y));
				rTouch.fingerID = touch.fingerID * -1;
				touches = new CustomTouch[]{touch,rTouch};
			}else{
				touches = new CustomTouch[]{touch};
			}

			ProcessTouches(touches);
		}

		if(!_isTouchThisFrame){
			// タッチイベントが無かった場合、前フレームのタッチイベントの保持を解放する。
			_prevTouchEventDict = null;
		}

	}



	/// <summary>
	/// タッチ入力情報を基に、タッチのイベント処理を行う関数。
	/// </summary>
	/// <param name="touches">このフレームのタッチ情報配列</param>
	private void ProcessTouches (CustomTouch[] touches) {

		if(touches == null || touches.Length == 0) return;

		foreach(CustomTouch touch in touches){
			ManageTouch(touch);
		}

		// generate TouchEvent Array.
		TouchEvent[] touchEvents = new TouchEvent[touches.Length];
		for(int i = 0; i < touches.Length; i++){
			touchEvents[i] = GetTouchEvent(touches[i]);
		}

		foreach(TouchEvent touchEvent in touchEvents){
			ProcessSingleTouchEvent(touchEvent);
		}

		_prevTouchEventDict = new Dictionary<int, TouchEvent>();
		foreach(TouchEvent myEvent in touchEvents){
			if(_prevTouchEventDict.ContainsKey(myEvent.touch.fingerID)){
				_prevTouchEventDict[myEvent.touch.fingerID] = myEvent;
			}else{
				_prevTouchEventDict.Add(myEvent.touch.fingerID,myEvent);
			}
		}

		_isTouchThisFrame = true;
	}

	/// <summary>
	/// タッチを管理する関数。
	/// </summary>
	/// <param name="touch">Touch.</param>
	private void ManageTouch (CustomTouch touch){
		List<CustomTouch> list;
		if(_touchListDict != null && _touchListDict.ContainsKey(touch.fingerID)){
			list = _touchListDict[touch.fingerID];
		}else{
			if(_touchListDict == null){
				_touchListDict = new Dictionary<int, List<CustomTouch>>();
			}
			list = new List<CustomTouch>();
		}

		list.Add(touch);

		if(_touchListDict.ContainsKey(touch.fingerID)){
			_touchListDict[touch.fingerID] = list;
		}else{
			 _touchListDict.Add(touch.fingerID,list);
		}

	}

	private TouchEvent GetTouchEvent(CustomTouch touch){
		return new TouchEvent(touch,GetTouchEventType(touch));
	}

	/// <summary>
	/// 過去のタッチ状況から現在のタッチイベントを判定して返す関数。
	/// </summary>
	/// <returns>The touch state.</returns>
	/// <param name="touch">Touch.</param>
	private TouchEventType GetTouchEventType(CustomTouch touch){
		List<CustomTouch> list = _touchListDict[touch.fingerID];
		if(list == null || list.Count == 0) return TouchEventType.NONE;

		if(list.Count == 1){
			return TouchEventType.TOUCH;
		}else if(touch.phase == TouchPhase.Stationary){
			int index = list.FindIndex( m => m.phase == TouchPhase.Moved);
			if(index != -1){
				return TouchEventType.SWIPE;
			}else{
				return TouchEventType.PRESS;
			}
		}else if(touch.phase == TouchPhase.Moved){
			return TouchEventType.SWIPE;
		}else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
			return TouchEventType.END;
		}
		return TouchEventType.NONE;
	}

	/// <summary>
	/// タッチ履歴を基に、現在のフレームのタッチのイベントをコールする。
	/// </summary>
	/// <param name="touchEvent">Touch event.</param>
	private void ProcessSingleTouchEvent (TouchEvent touchEvent){

		List<CustomTouch> touchList = null;

		float touchTime = 0f;
		if(_touchListDict.ContainsKey(touchEvent.touch.fingerID)){
			touchList = _touchListDict[touchEvent.touch.fingerID];
			if(touchList != null) touchTime = touchList[touchList.Count - 1].time - touchList[0].time;
		}

		switch(touchEvent.type){
			case TouchEventType.TOUCH:
			if(OnTouch != null) OnTouch(touchEvent.touch.position);
			break;

			case TouchEventType.PRESS:
			if(longPressTime < touchTime && 2 <= touchList.Count){
				bool b = longPressTime < touchList[touchList.Count - 2].time - touchList[0].time;
				if(!b && OnLongPress != null) OnLongPress(touchEvent.touch.position,touchTime);
			}
			if(OnPress != null) OnPress(touchEvent.touch.position,touchTime);
			break;

			case TouchEventType.SWIPE:
			if(OnSwipe != null) OnSwipe(touchEvent.touch.position,touchList[0].position,touchTime);
			break;

			case TouchEventType.END:

			int moveIndex = touchList.FindIndex(m => m.phase == TouchPhase.Moved);

			if (moveIndex == -1){
				if(longPressTime < touchTime){
					if(OnLongTap != null) OnLongTap(touchEvent.touch.position,touchTime);
				}else{
					if(OnTap != null) OnTap(touchEvent.touch.position);
				}
			}else{
				if(OnSwipeEnd != null) OnSwipeEnd(touchEvent.touch.position,touchList[0].position,touchTime);
			}

			if(_touchListDict.ContainsKey(touchEvent.touch.fingerID)){
				_touchListDict.Remove(touchEvent.touch.fingerID);
			}
			if(OnTouchEnd != null) OnTouchEnd(touchEvent.touch.position);
			break;
		}
	}

	/// <summary>
	/// TouchクラスをCustomTouchクラスに変換する関数。
	/// </summary>
	/// <returns>The to CustomTouch.</returns>
	/// <param name="touch">Touch.</param>
	private CustomTouch TouchToCustomTouch(Touch touch) {
		return new CustomTouch(touch);
	}

	/// <summary>
	/// Touch配列をCustomTouch配列に変換する関数。
	/// </summary>
	/// <returns>The to CustomTouch Array</returns>
	/// <param name="touches">Touches.</param>
	private CustomTouch[] TouchesToCustomTouches(Touch[] touches) {
		CustomTouch[] customTouches = new CustomTouch[touches.Length];
		for(int i = 0; i < touches.Length ; i++){
			customTouches[i] = TouchToCustomTouch(touches[i]);
		}
		return customTouches;
	}

	/// <summary>
	/// マウス入力をCustomTouchクラスに置き換える関数。
	/// </summary>
	/// <returns>CustomTouchクラス</returns>
	/// <param name="mousePosition">マウス座標</param>
	/// <param name="type">入力タイプ</param>
	private CustomTouch MouseToCustomTouch(Vector2 mousePosition,MouseButtonEventType type){

		TouchPhase phase = TouchPhase.Began;

		switch(type){

		case MouseButtonEventType.CLICK:
			phase = TouchPhase.Began;
			break;

		case MouseButtonEventType.DOWN:
			if(_isPrevMouseInput){
				//前のフレームもマウスのボタンを押していた場合。
				if(_prevMousePosition == mousePosition){
					//前のフレームとマウスの位置座標が同じ場合：プレス
					phase = TouchPhase.Stationary;
				}else{
					//前のフレームからマウスの位置座標が移動している場合：ドラッグ
					phase = TouchPhase.Moved;
				}
			}else{
				phase = TouchPhase.Began;
			}
			break;

		case MouseButtonEventType.UP:
			if(_isPrevMouseInput){
				//前のフレームもマウスボタンを押していた場合。
				phase = TouchPhase.Ended;

			}else{
				//普通だったらこの分岐は絶対にしない。
				phase = TouchPhase.Ended;
			}

			break;
		}

		CustomTouch touch = new CustomTouch();
		touch.deltaPosition = mousePosition - _prevMousePosition;
		touch.deltaTime = _isPrevMouseInput ? Time.deltaTime - _prevMouseTime : 0f;
		touch.fingerID =  _mouseClickCount;
		touch.phase = phase;
		touch.position = mousePosition;
		touch.tapCount = 1;
		touch.time = Time.time;

		if(phase == TouchPhase.Ended || phase == TouchPhase.Canceled){
			_isPrevMouseInput = false;
			_mouseClickCount++;
		}else{
			 _isPrevMouseInput = true;
			_prevMousePosition = mousePosition;
			_prevMouseTime = Time.deltaTime;
		}

		return touch;
	}


}

namespace TakashiCompany.Unity.TouchManager {

	/// <summary>
	/// Touchクラスと同じ変数を持つクラス。Touchクラスは変数に代入ができないので、同じクラスを作る。
	/// </summary>
	internal class CustomTouch{
		
		public Vector2 deltaPosition;
		public float deltaTime;
		public int fingerID;
		public TouchPhase phase;
		public Vector2 position;
		public int tapCount;
		public float time;

		public CustomTouch(){}

		public CustomTouch(Touch touch){
			deltaPosition = touch.deltaPosition;
			deltaTime = touch.deltaTime;
			fingerID = touch.fingerId;
			phase = touch.phase;
			position = touch.position;
			tapCount = touch.tapCount;
			time = Time.time;
		}

		public void Log(string str = ""){
			Debug.Log(str +  "CustomTouch fingerID:" + fingerID + " deltaPosition:" + deltaPosition +  " deltaTime:" + deltaTime + " phase:" + phase + " position:" + position + "tapCount:" + tapCount + " time:" + time);
		}
	}


	/// <summary>
	/// 前のフレームと現在のフレームのタッチ情報を基に、画面上での操作を判別するクラス。
	/// </summary>
	internal class TouchEvent{

		public CustomTouch touch;
		public TouchEventType type;

		public TouchEvent(){}

		public TouchEvent (CustomTouch argTouch, TouchEventType argType){
			touch = argTouch;
			type = argType;
		}

		public void Log(string str){
			touch.Log(str + " type:" + type + " ");
		}
	}
}
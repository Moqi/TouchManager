TouchManager 1.0.0 
2013/11/26
@takashicompany : http://takashicompany.com/

現在のフレームの画面タッチ情報を基に、画面のタッチイベントを発行するクラス。
シングルトンクラスとなっており、GameObjectにアタッチすることで動作する。

基本イベント
* TouchManager.OnTouch (Vector2 position) : 画面をタッチした時に呼ばれるイベント。
* TouchManager.OnPress (Vector2 position, float pressTime) : 画面をタッチし続けた時に呼ばれるイベント。
* TouchManager.OnSwipe (Vector2 position, Vector2 startPosition, float swipeTime) : 画面上で指を移動させた時に呼ばれるイベント。
* TouchManager.OnTouchEnd (Vector2 position) : 画面上から指を離した時に呼ばれるイベント。

結果イベント（アクション終了時に呼び出されるイベント）
* TouchManager.OnLongPress (Vector2 position, float pressTime) : 指定の時間以上、画面をタッチし続けた時に一度だけ呼ばれるイベント。
* TouchManager.OnTap (Vector2 position) : 指定の時間内に画面をタップした時に呼ばれるイベント。
* TouchManager.OnLongTap (Vector2 position, float tapTime) : 指定の時間より長い時間画面をタップした際に呼ばれるイベント。
* TouchManager.OnSwipeEnd (Vector2 position, Vector2 startPosition, float swipeTime) : スワイプが終了した際に呼ばれるイベント。
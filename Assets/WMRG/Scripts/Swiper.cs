using UnityEngine;
using System.Collections;

public class Swiper : MonoBehaviour {
	[HideInInspector]
	public float startTime;
	[HideInInspector]
	public Vector2 startPos;
    public Vector3 lastTouchPos;
    //[HideInInspector]
    public bool couldBeHSwipe, couldBeVSwipe, twofingersMode, couldBe1touch, canBeDoubleTap;
	
	public bool swipeLeft,swipeRight,swipeUp,swipeDown, oneTouch, pinchIn, pinchOut, swiping, doubleTap;	
	
	public float comfortZone = 100;
	public float minSwipeDist = 1f;
	public float maxSwipeTime = 0.5f;
    public float doubleTapTimer;
    public static Swiper instance;
    public bool log;

	
    void Start()
    {
        instance = this;
    }

	void Update() {
	
	    swipeLeft = false;
	    swipeRight = false;
	    swipeUp = false;
	    swipeDown = false;
	    oneTouch = false;
	    pinchIn = false;
	    pinchOut = false;
	    swiping = false;
        doubleTap = false;

        if (GameController.data.uiTouched || GameController.data.letterDragging)
            return;

        if (Input.touchCount ==1 && !twofingersMode) {
            Touch touch = Input.touches[0];
            switch (touch.phase) {
                case TouchPhase.Began:
                    couldBeHSwipe = true;
                    couldBeVSwipe = true;
                    couldBe1touch = true;
                    startPos = touch.position;
                    startTime = Time.time;
                    break;
                case TouchPhase.Moved:
				    if (Mathf.Abs(touch.position.y - startPos.y) > comfortZone) {
					    couldBeHSwipe = false;
				    }
				    if (Mathf.Abs(touch.position.x - startPos.x) > comfortZone) {
					    couldBeVSwipe = false;
				    }
				
				    if (Vector3.Distance (touch.position, startPos) > 10){
					    swiping = true;
					    couldBe1touch = false;
				    }
                    break;
                case TouchPhase.Ended:
                    float swipeTime = Time.time - startTime;
                    float swipeDist = (touch.position - startPos).magnitude;
                    if (couldBeHSwipe && (swipeTime < maxSwipeTime) && (swipeDist > minSwipeDist)) {
                        float swipeDirection = Mathf.Sign(touch.position.x - startPos.x);
					    if(swipeDirection == 1){
						    swipeRight=true;
                            if (log)
                                Debug.Log("swipeRight");
                        } else {
						    swipeLeft=true;
                            if (log)
                                Debug.Log("swipeLeft");
					    }
				    } else if (couldBeVSwipe && (swipeTime < maxSwipeTime) && (swipeDist > minSwipeDist)) {
					    float swipeDirection = Mathf.Sign(touch.position.y - startPos.y);
					    if(swipeDirection == 1){
						    swipeUp=true;
                            if (log)
                                Debug.Log("swipeUp");
                        } else {
						    swipeDown=true;
                            if (log)
                                Debug.Log("swipeDown");
                        }
				    } else if (canBeDoubleTap){
					   doubleTap = true;
                            if (log)
                                Debug.Log("doubleTap");
                    } else {
                        oneTouch =true;
                        if (log)
                            Debug.Log("oneTouch");
                        couldBe1touch = false;
                        canBeDoubleTap = true;
                    }
                    lastTouchPos = touch.position;
                    break;
            }
        } else if(Input.touchCount == 2){
		    twofingersMode = true;
		    Touch touchZero = Input.GetTouch(0);
		    Touch touchOne = Input.GetTouch(1);
		    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
		    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
		    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
		    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

		    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
		    // Debug.Log(deltaMagnitudeDiff);
		    if(deltaMagnitudeDiff > 5) {
			    pinchIn = true;
                if (log)
                    Debug.Log("pinchIn");
            }
		    if(deltaMagnitudeDiff < -5) {
			    pinchOut = true;
                if (log)
                    Debug.Log("pinchOut");
            }
	    }
	
	    if(twofingersMode){
		    if(Input.touchCount == 0)
			    twofingersMode = false;
	    }

        if (canBeDoubleTap && doubleTapTimer < 0.5f)
        {
            doubleTapTimer += Time.deltaTime;

        } else
        {
            canBeDoubleTap = false;
            doubleTapTimer = 0;
        }
    }
}

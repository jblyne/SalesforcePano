using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Used to detect swipe interactions from the user.

public class TouchController : MonoBehaviour {

	public enum Swipe {
		None = 0,
		Positive = 1,
		Negative = 2
	}

	private Vector2 startPos = Vector2.zero;
	private float swipeStartTime = 0.0f;
	private float minSwipeDist = 10.0f;
	private float maxSwipeTime = 5.0f;
	private float xSwipeSpeed = 0.0f;
	private float ySwipeSpeed = 0.0f;
	private Vector2 prevFramePos = new Vector2( -1.0f, -1.0f );
	private bool began;
	private bool xSwipe;

	private Swipe m_xSwipe = Swipe.None;
	private Swipe m_ySwipe = Swipe.None;

	public Vector2 SwipeSpeed {
		get { return new Vector2( xSwipeSpeed, ySwipeSpeed ); }
	}

	public Swipe[] SwipeDirection {
		get { return new Swipe[] { m_xSwipe, m_ySwipe }; }
	}

	public bool Swiping {
		get {
			if ( m_xSwipe == Swipe.None && m_ySwipe == Swipe.None ) {
				return false;
			}
			else {
				return true;
			}
		}
	}

	void Update() {
		m_xSwipe = m_ySwipe = Swipe.None;
		CheckSwipes();
	}

	private void CheckSwipes() {
		if ( Input.touchCount > 0 ) {
			Touch touch = Input.touches[0];
			switch ( touch.phase ) { //Screen has been touched, this could be a swipe.
				case TouchPhase.Began:
					m_xSwipe = m_ySwipe = Swipe.None;
					xSwipeSpeed = ySwipeSpeed = 0.0f;
					startPos = touch.position;
					swipeStartTime = Time.time;
					prevFramePos = new Vector2( -1.0f, -1.0f );
					began = true;
					break;

				case TouchPhase.Moved:
					if ( prevFramePos != new Vector2( -1.0f, -1.0f ) ) {
						float xSwipeValue = Mathf.Sign( touch.position.x - startPos.x );
						float xPrevSwipeValue = Mathf.Sign( touch.position.x - prevFramePos.x );

						float ySwipeValue = Mathf.Sign( touch.position.y - startPos.y );
						float yprevSwipeValue = Mathf.Sign( touch.position.y - prevFramePos.y );

						if ( xSwipeValue != xPrevSwipeValue || ySwipeValue != yprevSwipeValue ) {
							startPos = prevFramePos;
							swipeStartTime = Time.time;
							m_xSwipe = m_ySwipe = Swipe.None;
						}
					}

					float swipeTime = Time.time - swipeStartTime;
					float xSwipeDist = ( new Vector3( touch.position.x, 0, 0 ) - new Vector3( startPos.x, 0, 0 ) ).magnitude;
					float ySwipeDist = ( new Vector3( 0, touch.position.y, 0 ) - new Vector3( 0, startPos.y, 0 ) ).magnitude;

					if ( began ) {
						if ( xSwipeDist > ySwipeDist ) {
							xSwipe = true;
						}
						else {
							xSwipe = false;
						}
					}

					if ( xSwipe ) {
						if ( ( swipeTime < maxSwipeTime ) && ( xSwipeDist > minSwipeDist ) ) {
							float swipeValue = Mathf.Sign( touch.position.x - startPos.x );
							xSwipeSpeed = PixelsToPercentage( xSwipeDist, true ) / swipeTime;

							if ( swipeValue > 0 ) {
								m_xSwipe = Swipe.Positive;
							}
							else if ( swipeValue < 0 ) {
								m_xSwipe = Swipe.Negative;
							}
						}
						else {
							xSwipeSpeed = 0.0f;
						}
					}
					else {
						
						if ( ( swipeTime < maxSwipeTime ) && ( ySwipeDist > minSwipeDist ) ) {
							float swipeValue = Mathf.Sign( touch.position.y - startPos.y );
							ySwipeSpeed = PixelsToPercentage( ySwipeDist, false ) / swipeTime;

							if ( swipeValue > 0 ) {
								m_ySwipe = Swipe.Positive;
							}
							else {
								m_ySwipe = Swipe.Negative;
							}
						}
						else {
							ySwipeSpeed = 0.0f;
						}
					}
					prevFramePos = touch.position;
					break;

				case TouchPhase.Ended:
					prevFramePos = new Vector2( -1.0f, -1.0f );
					m_xSwipe = m_ySwipe = Swipe.None;
					ySwipeSpeed = xSwipeSpeed = 0.0f;
					break;
			}
		}
	}

	private float PixelsToPercentage( float _pix, bool _xAxis ) {
		float pixels;
		if ( _xAxis ) {
			pixels = Screen.width;
		}
		else {
			pixels = Screen.height;
		}

		return (_pix / pixels) * 100.0f;
	}
}
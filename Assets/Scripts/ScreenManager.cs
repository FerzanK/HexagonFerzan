using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TapEvent : UnityEvent<Vector3>
{
}

public class ScreenManager : MonoBehaviour
{
    public UnityEvent OnRotationCW = new UnityEvent();
    public UnityEvent OnRotationCC = new UnityEvent();
    public TapEvent OnTap = new TapEvent();
    private float touchStartTime = 0.0f;
    private HexGrid grid;
    private Vector2 touchStart;
    private Vector2 touchEnd;
    public float validTouchDuration = 1.2f;
    public float maximumValidTapDistance = 1.5f;
    public float tapRegisterDuration = 0.35f;

    void Awake()
    {
        grid = GameObject.Find("Grid").GetComponent<HexGrid>();
    }

    void Update()
    {
        ProcessTouch();
        //ProcessMouseControls();
    }

    Vector3 ToWorldPosition(Vector2 pos)
    {
        var worldPos = Camera.main.ScreenToWorldPoint(pos);
        worldPos.z = 0.0f;
        return worldPos;
    }

    void CalculateSwipeDirection()
    {
        if (grid.currentSelectionPoint == null) return;
        var v1 = touchStart;
        var v2 = touchEnd;

        float xDist = Mathf.Abs(v2.x - v1.x);
        float yDist = Mathf.Abs(v2.y - v1.y);
        var screenCoordinates = Camera.main.WorldToScreenPoint(grid.currentSelectionPoint.position);

        if (xDist > yDist)
        {
            if (screenCoordinates.y > v1.y)  // lower side of selection point
            {
                if (v1.x > v2.x) // to left swipe
                {
                    OnRotationCW.Invoke();

                }
                else
                {
                    OnRotationCC.Invoke();
                }
            }
            else // upper side of selection point
            {
                if (v1.x > v2.x) // // to right swipe
                {
                    OnRotationCC.Invoke();
                }
                else
                {
                    OnRotationCW.Invoke();
                }
            }
        }
        else
        {
            if (screenCoordinates.x > v1.x) // left side
            {
                if (v1.y > v2.y) // swipe down
                {
                    OnRotationCC.Invoke();
                } 
                else OnRotationCW.Invoke();

            }
            else // right side
            {
                if (v1.y > v2.y) OnRotationCW.Invoke();
                else OnRotationCC.Invoke();
            }
        }
    }

    void ProcessMouseControls()
    {
        if (Input.GetButtonDown("Fire1")) OnRotationCW.Invoke();
        if (Input.GetButtonDown("Fire2")) OnRotationCC.Invoke();
        if (Input.GetButtonDown("Fire3"))
        {
            var pos = ToWorldPosition(Input.mousePosition);
            OnTap.Invoke(pos);
        }
    }

    void ProcessTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.realtimeSinceStartup;
                    touchStart = touch.position;
                    touchEnd = touch.position;
                    break;

                case TouchPhase.Moved:
                    touchEnd = touch.position;
                    break;

                case TouchPhase.Ended:
                    var touchDuration = Time.realtimeSinceStartup - touchStartTime;
                    var dist = Vector2.Distance(touchEnd, touchStart);
                    if (touchDuration < tapRegisterDuration || 
                        maximumValidTapDistance > dist)
                    {
                        var pos = ToWorldPosition(touch.position);
                        OnTap.Invoke(pos);
                        break;

                    }
                    if (validTouchDuration > touchDuration)
                    {
                        CalculateSwipeDirection();
                    }
                    break;
            }
        }
    }


}

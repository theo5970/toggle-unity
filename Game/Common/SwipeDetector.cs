using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toggle.Game.Common
{
    public class SwipeDetector : MonoBehaviour
    {
        public enum Direction { Left, Right };
        public event System.Action<Direction> swipeDetected;

        public float angleThreshold = 30;
        public float lengthThreshold = 3;

        private Vector2 finger0EndPos;
        private Vector2 finger1EndPos;
        private Vector2 averageDirection;
        private int endedTouchFlag = 0;

        void Update()
        {
            if (Input.touchCount >= 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.fingerId == 0)
                {
                    if (touch.phase == TouchPhase.Moved) averageDirection += touch.deltaPosition * touch.deltaTime;
                    if (touch.phase == TouchPhase.Ended)
                    {
                        endedTouchFlag |= 0b01;
                        finger0EndPos = touch.position;
                    }
                }
                else
                {
                    if (touch.phase == TouchPhase.Moved) averageDirection += touch.deltaPosition * touch.deltaTime;
                    if (touch.phase == TouchPhase.Ended)
                    {
                        endedTouchFlag |= 0b10;
                        finger1EndPos = touch.position;
                    }
                }

                if (Input.touchCount >= 2)
                {
                    Touch touch2 = Input.GetTouch(1);
                    if (touch2.phase == TouchPhase.Moved) averageDirection += touch2.deltaPosition * touch.deltaTime;
                    if (touch2.phase == TouchPhase.Ended)
                    {
                        endedTouchFlag |= 0b10;
                        finger0EndPos = touch.position;
                        finger1EndPos = touch2.position;
                    }
                }
            }
            if (endedTouchFlag == 0b11)
            {
                float angleRight = Vector2.Angle(averageDirection.normalized, Vector2.left);
                float angleLeft = Vector2.Angle(averageDirection.normalized, Vector2.right);

                float relativeDistance = (finger1EndPos - finger0EndPos).magnitude / Screen.width;
                if (averageDirection.magnitude > lengthThreshold && relativeDistance < 0.35f)
                {
                    if (angleRight < angleThreshold)
                    {
                        swipeDetected?.Invoke(Direction.Right);
                    }
                    else if (angleLeft < angleThreshold)
                    {
                        swipeDetected?.Invoke(Direction.Left);
                    }
                }

                averageDirection = Vector2.zero;
                endedTouchFlag = 0;

                finger0EndPos = Vector2.zero;
                finger1EndPos = Vector2.zero;
            }
        }
    }
}

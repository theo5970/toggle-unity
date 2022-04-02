using UnityEngine;
using UnityEngine.EventSystems;

namespace Toggle.LevelEditor
{
    public class DragableUI : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public bool resetOnEnable;
        public bool lockX;
        public bool lockY;

        private bool isStarted = false;
        private Vector3 startPos;

        private Vector3 delta;

    
        public void OnBeginDrag(PointerEventData eventData)
        {
            delta = transform.position - Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 oldPos = transform.position;
            Vector3 newPos = Input.mousePosition + delta;
            if (lockX)
            {
                newPos.x = oldPos.x;
            }
            if (lockY)
            {
                newPos.y = oldPos.y;
            }
            transform.position = newPos;
        }

        private void Awake()
        {
            if (!isStarted)
            {
                isStarted = true;
                startPos = transform.position;
            }
        }

        private void OnEnable()
        {
            if (resetOnEnable)
            {
                transform.position = startPos;
            }
        }

    }
}

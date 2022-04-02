using UnityEngine;
using UnityEngine.EventSystems;

namespace Toggle.Game.Common
{
    public static class TouchUtils
    {
        public static bool IsPointerOverGameObject()
        {

#if UNITY_EDITOR || (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX)
            //check mouse
            return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
            //check touch
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    int id = touch.fingerId;
                    if (EventSystem.current.IsPointerOverGameObject(id))
                    {
                        return true;
                    }
                }
            }

            return false;
#endif
        }
    }
}

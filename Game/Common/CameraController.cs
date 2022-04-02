using Toggle.Core;
using UnityEngine;

namespace Toggle.Game.Common
{
    public class CameraController : MonoBehaviour
    {
        [Range(0.0f, 3.0f)] public float scaleFactor1 = 2.0f;

        [Range(0.0f, 3.0f)] public float scaleFactor2 = 2.0f;
        private Camera mainCamera;
        private GameMap map;

        private Vector3 startPos;


        private void Start()
        {
            map = GameMap.Instance;
            mainCamera = GetComponent<Camera>();

            startPos = transform.position;
        }

        void LateUpdate()
        {
            ButtonGrid grid = map.grid;
            if (grid.width == 0 && grid.height == 0)
            {
                return;
            }

            float screenRatio = (float) Screen.width / Screen.height;
            float targetRatio = (float) grid.width / grid.height;
            if (screenRatio > targetRatio)
            {
                mainCamera.orthographicSize = grid.height / scaleFactor1;
            }
            else
            {
                float differenceInSize = (targetRatio / screenRatio);
                mainCamera.orthographicSize = (grid.height / scaleFactor1) * differenceInSize;
            }


            mainCamera.orthographicSize *= screenRatio < 1 ? 1.2f : 1.5f;


            float ratioOfRatio = targetRatio / screenRatio;
            ratioOfRatio = Mathf.Clamp(ratioOfRatio, scaleFactor1, scaleFactor2);
            mainCamera.orthographicSize *= 1.666667f / ratioOfRatio;
        }
    }
}
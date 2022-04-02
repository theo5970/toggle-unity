using UnityEngine;

namespace Toggle.Game.Common
{
    public class DontDestroyOnLoadObject : MonoBehaviour
    {
        public static DontDestroyOnLoadObject Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}

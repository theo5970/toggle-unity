using UnityEngine;
using UnityEngine.SceneManagement;

namespace Toggle.Game.Common
{
    public class LoadSceneButton : MonoBehaviour
    {
        public string sceneName;
        private UnityEngine.UI.Button button;

        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<UnityEngine.UI.Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
    }
}

using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

namespace Toggle.Game.Common
{
    public class SceneStack : MonoBehaviour
    {
        private static Stack<string> sceneNameStack = new Stack<string>();

        void Awake()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneNameStack.Push(scene.name);
        }

        public static void BackToPreviousScene()
        {
            if (sceneNameStack.Count > 1)
            {
                sceneNameStack.Pop();
                string sceneNameToBack = sceneNameStack.Pop();
                SceneManager.LoadScene(sceneNameToBack);
            }
        }
    }
}
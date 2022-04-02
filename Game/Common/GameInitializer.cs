using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Toggle.Game.Common
{
    public class GameInitializer : MonoBehaviour
    {
        public CanvasGroup blackScreen;
        private DataManager dataManager;

        private void Awake()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR

            Application.targetFrameRate = 30;

#else
            Application.targetFrameRate = -1;
#endif

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                Time.timeScale = 1;
            };
        }

        IEnumerator Start()
        {
            dataManager = DataManager.Instance;
            var saveData = dataManager.saveData;

            blackScreen.gameObject.SetActive(true);

            if (Locale.supportedLanguages.Contains(saveData.language))
            {
                yield return Locale.Load(saveData.language);
            }
            else
            {
                yield return Locale.Load(SystemLanguage.English);
            }

            float t = 0;
            while (t < 1)
            {
                blackScreen.alpha = Mathf.Lerp(1, 0, t);
                t += Time.deltaTime / 0.5f;
                yield return null;
            }
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }
    }
}
using System.Collections;
using Toggle.Game.Common;
using Toggle.Game.Mode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Toggle.Game.MainMenu
{
    public struct RecentLevelView
    {
        public bool isEnable;
        public string packName;
        public int page;
    }

    public class MainMenuScene : MonoBehaviour
    {
        [Header("Menu Objects")]
        public GameObject firstMenu;
        public GameObject packSelectMenu;
        public GameObject levelSelectMenu;
        public GameObject settingsMenu;

        [Header("MainMenu Buttons")]
        public Button challengeButton;
        public Button editorButton;
        public Button onlineButton;
        public Button qrPlayButton;
        public Button rankingButton;
        public Button quitButton;

        private static RecentLevelView recentLevelView = new RecentLevelView();
        public static void SetRecentLevelView(string packName, int page)
        {
            recentLevelView.isEnable = true;
            recentLevelView.packName = packName;
            recentLevelView.page = page;
        }

        public static void SetEnableRecentLevelView(bool isEnable)
        {
            recentLevelView.isEnable = isEnable;
        }

        // Use this for initialization
        void Start()
        {
            firstMenu.SetActive(true);
            packSelectMenu.SetActive(false);
            levelSelectMenu.SetActive(false);
            settingsMenu.SetActive(false);
            
            challengeButton.onClick.AddListener(() =>
            {
                GameManager.SetGameMode(typeof(ChallengeMode));
                SceneManager.LoadScene("PlayScene");
            });

            onlineButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LevelSearchScene");
            });

            qrPlayButton.onClick.AddListener(() =>
            {
                GameManager.SetGameMode(typeof(QRScanMode));
                SceneManager.LoadScene("PlayScene");
            });
            editorButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("EditorSelectScene");
            });


            quitButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });

            LoadRecentLevelView();
        }
        
        private void LoadRecentLevelView()
        {
            if (!recentLevelView.isEnable) return;

            PanelStack.Instance.Push(packSelectMenu);
            levelSelectMenu.SetActive(true);

            PackSelectView packSelectView = packSelectMenu.GetComponent<PackSelectView>();
            packSelectView.SwitchToLevelSelect(recentLevelView.packName);

            LevelSelectGrid selectGrid = packSelectView.selectGrid;
            selectGrid.LoadPage(recentLevelView.page);

            recentLevelView.isEnable = false;
        }

#if UNITY_EDITOR  
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.G))
            {
                GameManager.SetGameMode(typeof(GeneratorMode));
                SceneManager.LoadScene("PlayScene");
            }
        }
#endif

    }
}

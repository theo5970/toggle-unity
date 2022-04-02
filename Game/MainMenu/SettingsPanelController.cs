using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Toggle.Client;
using Toggle.Game.Common;
using Toggle.LevelEditor;
using UnityEngine;
using UnityEngine.UI;


using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Toggle.Game.UI;

namespace Toggle.Game.MainMenu
{
    public class SettingsPanelController : MonoBehaviour
    { 

        [Header("General")]
        public DialogPanel dialog;
        public GameObject loadingObject;
        public RadioBox soundRadio;
        public RadioBox autoCloudRadio;
        public Button languageButton;
        public GameObject languagePanel;
        public Button achievementButton;
        public Button leaderboardButton;
        public Button discordButton;

        private DataManager dataManager;
        private GameSave saveData;

        // Start is called before the first frame update
        void Start()
        {
            dataManager = DataManager.Instance;
            saveData = dataManager.saveData;

            soundRadio.state = saveData.soundPlay;
            soundRadio.onStateChanged += SoundRadioStateChanged;

            autoCloudRadio.state = saveData.isAutoSave;
            autoCloudRadio.onStateChanged += AutoCloudStateChanged;

            languageButton.onClick.AddListener(() => languagePanel.SetActive(true));

#if UNITY_ANDROID
            achievementButton.onClick.AddListener(() => PlayGamesPlatform.Instance.ShowAchievementsUI());
            leaderboardButton.onClick.AddListener(() => PlayGamesPlatform.Instance.ShowLeaderboardUI());
#endif

            discordButton.onClick.AddListener(() => Application.OpenURL("https://discord.com/invite/zyrKetSBba"));
            if (Locale.isLoadedAny)
            {
                UpdateLanguageButton();
            }
            Locale.onLoad += UpdateLanguageButton;
            
        }

        private void OnDestroy()
        {
            Locale.onLoad -= UpdateLanguageButton;
        }

        private void UpdateLanguageButton()
        {
            languageButton.SetText(string.Format("{0}: {1}", Locale.Get("settings.language"), Locale.languageNameDic[Locale.currentLanguage]));
        }

        private void SoundRadioStateChanged(bool newState)
        {
            saveData.soundPlay = newState;
            AudioListener.volume = newState ? 1 : 0;
            dataManager.Save();
        }

        private void AutoCloudStateChanged(bool newState)
        {
            saveData.isAutoSave = newState;
            dataManager.Save();
        }

    }
}

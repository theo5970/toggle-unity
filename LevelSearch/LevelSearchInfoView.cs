using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Toggle.Client;
using UnityEngine.SceneManagement;
using Toggle.Game.Common;

namespace Toggle.LevelSearch
{
    public class LevelSearchInfoView : MonoBehaviour
    {
        [HideInInspector]
        public int levelId;

        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelCreatorText;
        public TextMeshProUGUI downloadCountText;
        public TextMeshProUGUI minimumClicksText;
        public TextMeshProUGUI likeCountText;
        public TextMeshProUGUI levelIdText;
        public ThumbUpButton thumbUpButton;

        public Button playButton;
        private OnlineLevel levelInfo;
        private string levelCode;

        private LevelDownloadCache downloadCache = LevelDownloadCache.Instance;
        void Start()
        {
            thumbUpButton.onClick += OnThumbButtonClicked;
            playButton.onClick.AddListener(() =>
            {
                OnlineLevelMode.levelCode = levelCode;
                GameManager.SetGameMode(typeof(OnlineLevelMode));
                SceneManager.LoadScene("PlayScene");
            });
        }

        private void OnDestroy()
        {
            thumbUpButton.onClick -= OnThumbButtonClicked;
        }

        private async void OnThumbButtonClicked()
        {
            levelInfo.likeCount += thumbUpButton.state ? 1 : -1;
            if (levelInfo.likeCount < 0) levelInfo.likeCount = 0;

            likeCountText.text = string.Format("{0:#,0}", levelInfo.likeCount);

            if (thumbUpButton.state)
            {
                await WebConnection.Instance.LikeLevel(levelId);
            }
            else
            {
                await WebConnection.Instance.CancelLikeLevel(levelId);
            }


            if (downloadCache.TryGet(levelId, out SavedOnlineLevel savedOnlineLevel))
            {
                savedOnlineLevel.isLiked = thumbUpButton.state;
                downloadCache.Save();
            }
        }

        public void Apply(OnlineLevel onlineLevel)
        {
            levelInfo = onlineLevel;

            levelNameText.text = onlineLevel.title;
            levelCreatorText.text = "by " + onlineLevel.creator;
            downloadCountText.text = string.Format("{0:#,0}", onlineLevel.downloadCount);
            likeCountText.text = string.Format("{0:#,0}", onlineLevel.likeCount);
            levelIdText.text = "ID : " + onlineLevel.id;
            minimumClicksText.text = onlineLevel.minimumClicks.ToString();

            thumbUpButton.state = false;

            levelId = onlineLevel.id;

            bool shouldDownloadLevel = false;
            if (downloadCache.TryGet(levelId, out SavedOnlineLevel savedOnlineLevel))
            {
                thumbUpButton.state = savedOnlineLevel.isLiked;
                levelCode = savedOnlineLevel.levelCode;

                Debug.Log(onlineLevel.updatedDatetime + " > " + savedOnlineLevel.info.updatedDatetime);

                if (onlineLevel.updatedDatetime > savedOnlineLevel.info.updatedDatetime)
                {
                    shouldDownloadLevel = true;
                    savedOnlineLevel.info.updatedDatetime = onlineLevel.updatedDatetime;
                    downloadCache.Save();
                }
            }
            else
            {
                shouldDownloadLevel = true;
            }

            if (shouldDownloadLevel) DownloadLevelAsync();
        }

        private async void DownloadLevelAsync()
        {
            levelCode = await WebConnection.Instance.DownloadLevel(levelId);

            if (downloadCache.TryGet(levelId, out SavedOnlineLevel savedOnlineLevel))
            {
                savedOnlineLevel.levelCode = levelCode;
            }
            else
            {
                downloadCache.Add(levelId, new SavedOnlineLevel
                {
                    info = levelInfo,
                    levelCode = levelCode
                });
            }
            downloadCache.Save();

            levelInfo.downloadCount++;
            downloadCountText.text = string.Format("{0:#,0}", levelInfo.downloadCount);
        }

    }
}
using GooglePlayGames.BasicApi.SavedGame;
using System.Collections;
using System.Collections.Generic;
using Toggle.Game.Common;
using Toggle.Game.Data;
using Toggle.Game.GPGS;
using Toggle.Game.Mode;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Toggle.Game.Achievement
{
    public class AchievementMonitor : Singleton<AchievementMonitor>
    {
        private AchievementManager achievementManager;
        private GameMode gameMode;
        private GameSave saveData;

        private Dictionary<string, LevelPackSave> packSaves;
        private WaitForSeconds waitOneSeconds = new WaitForSeconds(1);

        private CloudSaveManager cloudSaveManager;

        IEnumerator Start()
        {
            cloudSaveManager = CloudSaveManager.Instance;
            cloudSaveManager.onLoad += OnCloudSaveLoad;

            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
            achievementManager = AchievementManager.Instance;

            saveData = DataManager.Instance.saveData;
            packSaves = saveData.packSaves;

            yield return achievementManager.WaitForInit();
            UpdateAchievements();
        }

        private void OnDestroy()
        {
            if (cloudSaveManager != null) cloudSaveManager.onLoad -= OnCloudSaveLoad;
        }

        private void OnCloudSaveLoad(SavedGameRequestStatus status)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                UpdateAchievements();
            }
        }

        public void UpdateAchievements()
        {
            CheckLevels();
            CheckButtonClicks();
        }


        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "PlayScene")
            {
                StartCoroutine(WaitOneFrameAndCall(() =>
                {
                    var gameManager = GameManager.Instance;
                    gameMode = gameManager.currentMode;
                    gameMode.onLevelCleared += CheckLevels;
                    gameMode.onButtonClick += CheckButtonClicks;
                }));
            }
        }
        
        private IEnumerator WaitOneFrameAndCall(System.Action callback)
        {
            yield return null;

            callback();
        }
        private void SceneUnloaded(Scene scene)
        {
            if (scene.name == "PlayScene")
            {
                gameMode.onLevelCleared -= CheckLevels;
                gameMode.onButtonClick -= CheckButtonClicks;
            }
        }

        private int CountClearsOnAllPacks()
        {
            int result = 0;
            foreach (var packSave in packSaves.Values)
            {
                result += packSave.CountClearedLevels();
            }
            return result;
        }

        private int CountPerfectClearsOnAllPacks()
        {
            int result = 0;
            foreach (var packSave in packSaves.Values)
            {
                result += packSave.CountPerfectLevels();
            }
            return result;
        }

        private void CheckLevels()
        {
            #region 모든 팩에서 클리어한 레벨 수
            int clearedLevels = CountClearsOnAllPacks();

            if (clearedLevels >= 500)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_500, OnAchievementUpload);
            }
            if (clearedLevels >= 250)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_250, OnAchievementUpload);
            }
            if (clearedLevels >= 200)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_200, OnAchievementUpload);
            }
            if (clearedLevels >= 100)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_100, OnAchievementUpload);
            }
            if (clearedLevels >= 50)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_50, OnAchievementUpload);
            }
            if (clearedLevels >= 25)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_25, OnAchievementUpload);
            }
            if (clearedLevels >= 10)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_10, OnAchievementUpload);
            }
            #endregion

            #region 모든 팩에서 완벽하게 클리어한 레벨 수
            int perfectLevels = CountPerfectClearsOnAllPacks();
            if (perfectLevels >= 500)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_500_perfect, OnAchievementUpload);
            }
            if (perfectLevels >= 250)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_250_perfect, OnAchievementUpload);
            }
            if (perfectLevels >= 100)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_100_perfect, OnAchievementUpload);
            }
            if (perfectLevels >= 50)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_50_perfect, OnAchievementUpload);
            }
            if (perfectLevels >= 25)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_25_perfect, OnAchievementUpload);
            }
            if (perfectLevels >= 10)
            {
                achievementManager.UnlockAchievement(GPGSIds.achievement_cleared_10_perfect, OnAchievementUpload);
            }
            #endregion
        }

        private void OnAchievementUpload(bool success)
        {
            Debug.Log("업적 업로드: " + success);
        }
        private void CheckButtonClicks()
        {
        }

        public void ReportBug()
        {
            achievementManager.UnlockAchievement(GPGSIds.achievement_bug, OnAchievementUpload);
        }


    }
}

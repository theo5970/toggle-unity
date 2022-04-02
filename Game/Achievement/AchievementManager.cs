using System.Collections;
using System.Collections.Generic;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Toggle.Game.Achievement
{
    public class AchievementManager : Singleton<AchievementManager>
    {

        private Dictionary<string, bool> completedAchievements = new Dictionary<string, bool>();
        private static WaitForSeconds waitOneSeconds = new WaitForSeconds(1);

        public bool isAchievementsLoaded { get; private set; }



        private IEnumerator Start()
        {
            while (!Social.localUser.authenticated)
            {
                yield return waitOneSeconds;
            }

            Social.LoadAchievements(achievementList =>
            {
                foreach (IAchievement achievement in achievementList)
                {
                    if (achievement.completed)
                    {
                        completedAchievements.Add(achievement.id, true);

                    }
                }
                isAchievementsLoaded = true;
            });

        }



        public IEnumerator WaitForInit()
        {
            while (!isAchievementsLoaded)
            {
                yield return waitOneSeconds;
            }
        }

        public void UnlockAchievement(string achievementId, System.Action<bool> callback)
        {
            if (completedAchievements.ContainsKey(achievementId))
            {
                callback?.Invoke(false);
            }
            else
            {
                Social.ReportProgress(achievementId, 100.0, callback);
                completedAchievements.Add(achievementId, true);
            }
        }
    }
}
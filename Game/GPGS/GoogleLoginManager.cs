using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Toggle.Client;
using UnityEngine.SocialPlatforms;
using Toggle.Game.Common;

namespace Toggle.Game.GPGS
{
    public class GoogleLoginManager : MonoBehaviour
    {
#if UNITY_ANDROID
        void Awake()
        {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();


        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);

#if UNITY_EDITOR
            LoginTestUser();
#else
            Debug.Log("IsAuthenticated: " + PlayGamesPlatform.Instance.IsAuthenticated());
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
                {
                    Debug.Log("Google Play Login Result: " + result);

                    if (result == SignInStatus.Success)
                    {
                        Debug.Log("User ID : " + Social.localUser.id);
                        Debug.Log("User Display Name : " + Social.localUser.userName);
                    }

                    ILocalUser localUser = Social.localUser;
                    if (!string.IsNullOrEmpty(localUser.id))
                    {
                        SignIn(localUser.id, localUser.userName);
                    }
                });
            }
#endif
#endif
        }

#if UNITY_EDITOR
        private void LoginTestUser()
        {
            SignIn("a_1234567890", "UnityEditor");
        }
#endif

        private async void SignIn(string id, string userName)
        {
            DataManager dataManager = DataManager.Instance;

            if (string.IsNullOrEmpty(dataManager.saveData.sessionId))
            {
                AuthResult authResult = await WebConnection.Instance.SignIn(id, userName);
                if (authResult.isSuccess)
                {
                    dataManager.saveData.sessionId = WebConnection.Instance.sessionId;
                    dataManager.Save();
                }
                Debug.Log("AuthResult: " + authResult.isSuccess);
            }
        }
    }
}
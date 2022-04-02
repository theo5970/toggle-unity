using System.Collections;
using System.Collections.Generic;
using TMPro;
using Toggle.LevelEditor;
using UnityEngine;
using UnityEngine.UI;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Toggle.Game;
using Toggle.Game.GPGS;
using GooglePlayGames.BasicApi.SavedGame;

public class GoogleSettingsPanelController : MonoBehaviour
{
#if UNITY_ANDROID
    public DialogPanel dialog;
    public Button loginButton;
    public Button loadButton;
    public Button saveButton;
    public GameObject loadingObject;
    public GameObject cloudPanel;
    public TextMeshProUGUI lastBackupText;

    private CloudSaveManager cloudSaveManager;
    private WaitForSeconds waitForOneSeconds = new WaitForSeconds(1.0f);
    void Start()
    {
        cloudPanel.SetActive(Social.localUser.authenticated);
        loginButton.onClick.AddListener(OnGoogleLoginButtonClicked);

        loadButton.onClick.AddListener(OnLoadButtonClicked);
        saveButton.onClick.AddListener(OnSaveButtonClicked);

        Locale.onLoad += UpdateBackupText;
    }

    private void UpdateBackupText()
    {
        if (cloudSaveManager.GetLastSaveDateTime().Ticks == 0)
        {
            lastBackupText.text = string.Format("{0}: N/A", Locale.Get("settings.lastsave"));
        }
        else
        {
            lastBackupText.text = string.Format("{0}: {1}", Locale.Get("settings.lastsave"), cloudSaveManager.GetLastSaveDateTime().ToString());
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateLoginInfoTextCoroutine());

        cloudSaveManager = CloudSaveManager.Instance;
        cloudSaveManager.onLoad += OnCloudSaveManagerLoad;
        cloudSaveManager.onSave += OnCloudSaveManagerSave;
        cloudSaveManager.onError += OnCloudSaveManagerError;
    }

    private void OnDisable()
    {
        cloudSaveManager.onLoad -= OnCloudSaveManagerLoad;
        cloudSaveManager.onSave -= OnCloudSaveManagerSave;
        cloudSaveManager.onError -= OnCloudSaveManagerError;

        Locale.onLoad -= UpdateBackupText;
    }


    private void OnCloudSaveManagerLoad(SavedGameRequestStatus status)
    {
        loadingObject.SetActive(false);
        if (status == SavedGameRequestStatus.Success)
        {
            dialog.Show(Locale.Get("messages.successcloudload"), null, Locale.Get("default.ok"));
        }
        else
        {
            dialog.Show(Locale.Get("messages.failcloudload") + status, null, Locale.Get("default.ok"));
        }
        UpdateBackupText();
    }
    private void OnCloudSaveManagerSave(SavedGameRequestStatus status)
    {
        loadingObject.SetActive(false);
        if (status == SavedGameRequestStatus.Success)
        {
            dialog.Show(Locale.Get("messages.successcloudsave"), null, Locale.Get("default.ok"));
        }
        else
        {
            dialog.Show(Locale.Get("messages.failcloudsave") + status, null, Locale.Get("default.ok"));
        }
        UpdateBackupText();
    }
    private void OnCloudSaveManagerError(CloudSaveError error)
    {
        loadingObject.SetActive(false);
        dialog.Show(Locale.Get("default.error") + ": " + error, null, Locale.Get("default.ok"));
    }

    private IEnumerator UpdateLoginInfoTextCoroutine()
    {
        yield return null;

        while (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            yield return waitForOneSeconds;
        }
        cloudPanel.SetActive(true);
        loginButton.SetTranslationKey("default.logout");

        UpdateBackupText();
    }

    private void OnLoadButtonClicked()
    {
        dialog.Show(Locale.Get("clouddialog.load"), i =>
        {
            if (i == 0)
            {
                loadingObject.SetActive(true);
                cloudSaveManager.LoadFromCloud();
            }
        }, Locale.Get("default.ok"), Locale.Get("default.cancel"));
    }

    private void OnSaveButtonClicked()
    {
        dialog.Show(Locale.Get("clouddialog.save"), i =>
        {
            if (i == 0)
            {
                loadingObject.SetActive(true);
                cloudSaveManager.SaveToCloud();
            }
        }, Locale.Get("default.ok"), Locale.Get("default.cancel"));
    }

    private void OnGoogleLoginButtonClicked()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.SignOut();
            cloudPanel.SetActive(false);

            dialog.Show(Locale.Get("messages.signout"), null, Locale.Get("default.ok"));
            loginButton.SetTranslationKey("default.login");
        }
        else
        {
            loadingObject.SetActive(true);
            PlayGamesPlatform.Instance.Authenticate((result, message) =>
            {
                loginButton.interactable = false;
                if (result)
                {
                    dialog.Show(Locale.Get("messages.successgooglelogin"), null, Locale.Get("default.ok"));
                    cloudPanel.SetActive(true);
                }
                else
                {
                    dialog.Show(Locale.Get("messages.failgooglelogin") + "\n" + message, null, Locale.Get("default.ok"));
                }
                loginButton.SetTranslationKey(result ? "default.logout" : "default.login");
                loadingObject.SetActive(false);

                loginButton.interactable = true;
            });
        }
    }
#endif
}

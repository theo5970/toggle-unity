using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Toggle.Game.Common;
using Toggle.LevelEditor;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.UI;

public class HintManager : Singleton<HintManager>
{
#if TEST_BUILD
    /*
     * 테스트 광고 UnitID
     */
    private static readonly string adUnitId = Global.testAdUnitId;
#else

    /*
     * 실제 광고 UnitID
     */
    private static readonly string adUnitId = Global.realAdUnitId;
#endif

    public GameObject rootObject;

    public Button viewAdsButton;
    public Button cancelButton;

    public DialogPanel dialog;

    public TextMeshProUGUI hintCounterText;

    private RewardedAd rewardedAd;
    private DataManager dataManager;

    private System.Action<bool> tempCallback;

    void Start()
    {
        viewAdsButton.onClick.AddListener(() => ShowAds());
        cancelButton.onClick.AddListener(() => rootObject.SetActive(false));

        dataManager = DataManager.Instance;

        UpdateHintCounterText();
    }

    private void OnEnable()
    {
        viewAdsButton.interactable = true;
        cancelButton.interactable = true;
    }

    private void UpdateHintCounterText()
    {
        hintCounterText.text = string.Format("Hint ({0})", dataManager.saveData.hintCount);
    }
    public void UseHint(System.Action<bool> callback)
    {
        var saveData = dataManager.saveData;
        if (saveData.hintCount < 0 || saveData.hintCount > 3) return;

        if (saveData.hintCount > 0)
        {
            callback(true);
            saveData.hintCount--;

            UpdateHintCounterText();
            dataManager.Save();
        }
        else
        {
            rootObject.SetActive(true);
            tempCallback = callback;
        }
    }

    private void ShowAds()
    {
        rewardedAd = new RewardedAd(adUnitId);
        rewardedAd.OnAdLoaded += RewardedAd_OnAdLoaded;
        rewardedAd.OnAdOpening += RewardedAd_OnAdOpening;
        rewardedAd.OnUserEarnedReward += RewardedAd_OnUserEarnedReward;
        rewardedAd.OnAdFailedToLoad += RewardedAd_OnAdFailedToLoad;
        rewardedAd.OnAdFailedToShow += RewardedAd_OnAdFailedToShow;
        rewardedAd.OnAdClosed += RewardedAd_OnAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(request);

        viewAdsButton.interactable = false;
        cancelButton.interactable = false;
    }

    private void RewardedAd_OnAdClosed(object sender, System.EventArgs e)
    {
        viewAdsButton.interactable = true;
        cancelButton.interactable = true;
    }

    private void RewardedAd_OnAdLoaded(object sender, System.EventArgs e)
    {
        rewardedAd.Show();
    }

    private void RewardedAd_OnAdFailedToShow(object sender, AdErrorEventArgs e)
    {
        Debug.LogError("Ad Error: " + e.AdError.ToString());
        StartCoroutine(ShowFailDialogCoroutine(e.AdError.GetMessage()));
    }

    private void RewardedAd_OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        Debug.LogError("Ad Error: " + e.LoadAdError.ToString());
        StartCoroutine(ShowFailDialogCoroutine(e.LoadAdError.GetMessage()));
    }

    private IEnumerator ShowFailDialogCoroutine(string message)
    {
        yield return new WaitForEndOfFrame();

        dialog.Show(Locale.Get("hintdialog.messages.fail") + "\n" + message, null, Locale.Get("default.ok"));
        viewAdsButton.interactable = true;
        cancelButton.interactable = true;
    }
    private void RewardedAd_OnUserEarnedReward(object sender, Reward e)
    {
        viewAdsButton.interactable = true;
        cancelButton.interactable = true;

        dataManager.saveData.hintCount = Mathf.Clamp(dataManager.saveData.hintCount + 3, 0, 3);
        dataManager.Save();

        rewardedAd.OnAdLoaded -= RewardedAd_OnAdLoaded;
        rewardedAd.OnAdOpening -= RewardedAd_OnAdOpening;
        rewardedAd.OnUserEarnedReward -= RewardedAd_OnUserEarnedReward;
        rewardedAd.OnAdFailedToLoad -= RewardedAd_OnAdFailedToLoad;
        rewardedAd.OnAdFailedToShow -= RewardedAd_OnAdFailedToShow;
        rewardedAd.OnAdClosed -= RewardedAd_OnAdClosed;

        UpdateHintCounterText();
        rootObject.SetActive(false);

        tempCallback?.Invoke(false);
    }

    private void RewardedAd_OnAdOpening(object sender, System.EventArgs e)
    {
        viewAdsButton.interactable = false;
        cancelButton.interactable = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            rootObject.SetActive(false);
        }
    }
}

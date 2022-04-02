using System.Collections;
using QRPlay;
using Toggle.Core;
using Toggle.Core.Function;
using Toggle.Game.Common;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.Game.Mode
{
    public class QRScanMode : GameMode
    {
        public QRCodeLevelFetcher qrFetcher;

        private Button nextButton;
        private Button restartButton;
        private Button rescanButton;

        public override void Prepare()
        {
            base.Prepare();

            nextButton = gameManager.clearDialog.nextButton;
            nextButton.onClick.AddListener(OnNextButtonClicked);

            map.onLevelLoaded.AddListener(OnLevelLoaded);

            GameObject scanPrefab = Resources.Load<GameObject>("Prefabs/QRCodeScan");
            GameObject scanObject = Instantiate(scanPrefab);

            qrFetcher = scanObject.GetComponent<QRCodeLevelFetcher>();
            qrFetcher.onScanComplete += QRFetcherScanComplete;

            StartCoroutine(StartScanAsync());

            gameManager.isInputFreeze = true;

            rescanButton = gameManager.rescanButton;
            rescanButton.gameObject.SetActive(true);
            rescanButton.onClick.AddListener(OnRescanButtonClicked);

            gameManager.modeText.text = "커스텀 플레이";
            gameManager.infoButton.gameObject.SetActive(true);
            gameManager.infoButton.onClick.AddListener(OnInfoButtonClicked);
        }

        private ToggleLevelReader.Result readerResult;
        private void QRFetcherScanComplete(ToggleLevelReader.Result readerResult)
        {
            isSolved = false;
            gameTimer.ResetTimer();
            gameTimer.UpdateUI();

            gameManager.ResumeGame();

            if (readerResult.minimumClickCount.HasValue)
            {
                int minimumClickCount = readerResult.minimumClickCount.Value;
                gameManager.minimumClickCountText.text = minimumClickCount.ToString();
            }

            int latteFlag = 0;
            for (int i = 0; i < readerResult.buttons.Length; i++)
            {
                FunctionSubType subType = readerResult.buttons[i];
                if (subType == FunctionSubType.KOA)
                {
                    latteFlag |= 0x01;
                }
                if (subType == FunctionSubType.KOB)
                {
                    latteFlag |= 0x02;
                }
            }

            if (latteFlag == 3)
            {
                GameManager.SetGameMode(typeof(LatteIsHorseMode));
                gameManager.PrepareSelectedMode();
            }

            this.readerResult = readerResult;
        }

        public override void OnBackToMainMenu()
        {
            qrFetcher.decodeController.StopWork();
        }

        private IEnumerator StartScanAsync()
        {
            yield return null;

            gameManager.PauseGame(false);
            gameTimer.StopTimer();
            qrFetcher.StartScan();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            map.onLevelLoaded.RemoveListener(OnLevelLoaded);
            qrFetcher.onScanComplete -= QRFetcherScanComplete;
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        private void OnLevelLoaded()
        {
            isSolved = false;
            clickCount = 0;

            gameManager.isInputFreeze = false;
            gameManager.ResetCommandManager();
        }

        private void OnNextButtonClicked()
        {
            qrFetcher.StartScan();
            gameManager.isInputFreeze = true;

            gameTimer.ResetTimer();
            gameTimer.UpdateUI();
        }

        private void OnRescanButtonClicked()
        {
            StartCoroutine(StartScanAsync());
        }

        private void OnInfoButtonClicked()
        {
            string creatorText = string.Format("{0}: {1}", Locale.Get("qrmode.creator"), readerResult.creator == null ? Locale.Get("qrmode.creatorunknown") : readerResult.creator);

            gameManager.infoDialog.Show(creatorText, null, Locale.Get("default.ok"));
        }


        protected override void OnButtonClicked()
        {
            if (!gameTimer.isWorking)
            {
                gameTimer.StartTimer();
            }

        }

        protected override void OnClearMap()
        {
            gameTimer.StopTimer();
            gameManager.isInputFreeze = true;

            string message;
            if (readerResult.creator != null)
            {
                message = $"축하합니다!\n{readerResult.creator}님이 제작하신 레벨을 {clickCount}번 클릭 만에 클리어했습니다.";
            }
            else
            {
                message = $"축하합니다!\n누군가가 제작한 레벨을 {clickCount}번 클릭 만에 클리어했습니다.";
            }

            int starCount = ScoreCalculator.CalculateStarCount(ScoreCalculator.Calculate(clickCount, minimumClickCount, 20));
            gameManager.clearDialog.Show(starCount, message, "다시 스캔하기");
        }
    }
}
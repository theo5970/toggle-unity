using System.Collections;
using TMPro;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.Game.Common
{
    public class GameTimer : Singleton<GameTimer>
    {
        public enum Mode
        {
            Stopwatch, CountDown
        }
        private GameManager gameManager;

        private IEnumerator counterCoroutine;
        public SafeInt totalSeconds { get; private set; }

        public Mode mode = Mode.Stopwatch;
        public System.Action onCountdownEnd;
        public TextMeshProUGUI timerText;

        public Gradient progressGradient;
        public Image progressBarImage;

        public bool isWorking
        {
            get;
            private set;
        }

        public int countdownTime
        {
            get => _countdownTime;
            set
            {
                _countdownTime = value;
                ResetTimer();
            }
        }

        private int _countdownTime;

        private void Awake()
        {
            isWorking = false;
        }

        public void StartTimer()
        {
            if (counterCoroutine != null)
            {
                StopCoroutine(counterCoroutine);
            }


            counterCoroutine = InternalCoroutine();
            gameManager = GameManager.Instance;
            StartCoroutine(counterCoroutine);

            isWorking = true;
        }

        public void IncreaseTime(int secondsToAdd)
        {
            totalSeconds += secondsToAdd;
            UpdateUI();
        }

        public void SetTime(int time)
        {
            totalSeconds = time;
            UpdateUI();
        }

        public void StopTimer()
        {
            if (isWorking)
            {
                StopCoroutine(counterCoroutine);
                isWorking = false;
            }
        }

        public void ResetTimer()
        {
            totalSeconds = (mode == Mode.Stopwatch) ? 0 : countdownTime;
            UpdateUI();
        }

        public void UpdateUI()
        {
            timerText.text = CommonUtils.FormatTime(totalSeconds);
            if (mode == Mode.CountDown)
            {
                float t = ((float)totalSeconds / countdownTime);
                // float t2 = 0.5f * (Mathf.Cos(Mathf.PI * (t + 1)) + 1);
                progressBarImage.fillAmount = t;
                progressBarImage.color = progressGradient.Evaluate(t);
            }
        }

        private WaitForSeconds oneSecondWait = new WaitForSeconds(1);
        private IEnumerator InternalCoroutine()
        {
            while (totalSeconds >= 0)
            {
                yield return oneSecondWait;
                if (!gameManager.isGamePaused)
                {
                    if (mode == Mode.Stopwatch)
                        totalSeconds++;
                    else
                        totalSeconds--;
                }

                UpdateUI();

                if (mode == Mode.CountDown && totalSeconds == 0)
                {
                    onCountdownEnd?.Invoke();
                    break;
                }
            }

            isWorking = false;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Toggle.Client;
using Toggle.Core;
using Toggle.Core.Solver;
using Toggle.Game.Common;
using Toggle.Game.Data;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Toggle.Game.Mode
{
    public class ClassicMode : GameMode
    {
        private static LevelPackSave currentPackSave;
        private static int currentLevelIndex;

        private Button hintButton;
        private Button nextButton;
        private TextMeshProUGUI hintButtonText;

        private SequenceReverse reverse;
        private ToggleClassicLevel currentLevel;

        private BitArray firstStates;
        private BitArray emptyStates;

        private List<BaseButton> solverInputs = new List<BaseButton>();
        private List<BaseButton> hintSolutions = new List<BaseButton>();

        private bool isPressedHintCoordinate = true;

        private float startTime;
        private bool isFirstSinceLevelLoaded = true;
        private ClassicPlayRecord playRecord;

        private SwipeDetector swipeDetector;

        public static void SetLevel(LevelPackSave packSave, int newIndex)
        {
            currentPackSave = packSave;
            currentLevelIndex = newIndex;
        }

        public override void Prepare()
        {
            base.Prepare();
            reverse = new SequenceReverse();

            LoadLevel(currentLevelIndex);

            swipeDetector = gameManager.GetComponent<SwipeDetector>();
            swipeDetector.swipeDetected += OnSwipeDetected;
            nextButton = gameManager.clearDialog.nextButton;
            nextButton.onClick.AddListener(OnNextButtonClicked);

            commandManager.onUndo += OnCommandUndo;
            commandManager.onRedo += OnCommandRedo;

            hintButton = gameManager.hintButton;
            hintButton.gameObject.SetActive(true);
            hintButton.onClick.AddListener(OnHintButtonClicked);
            hintButtonText = hintButton.GetComponentInChildren<TextMeshProUGUI>();
        }



        private void OnCommandUndo(Command command)
        {
            playRecord.undoCount++;
        }

        private void OnCommandRedo(Command command)
        {
            playRecord.redoCount++;
        }

        protected override void OnGameRestart()
        {
            base.OnGameRestart();

            playRecord.restartCount++;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            nextButton.onClick.RemoveListener(OnNextButtonClicked);
            hintButton.onClick.RemoveListener(OnHintButtonClicked);
        }

        private int previousTotalButtons = 0;

        private void LoadLevel(int levelIndex)
        {
            ResetPlayRecord();
            playSequence.Clear();
            isSolved = false;

            isFirstSinceLevelLoaded = true;

            currentLevel = LevelManager.Instance.GetLevel(currentPackSave.packName, levelIndex);
            map.LoadLevel(currentLevel.data);

            // 세이브 파일에 '시도 했음(1)'로 저장하기.
            LevelTryState levelState = currentPackSave.GetLevelState(currentLevelIndex);
            if (levelState == LevelTryState.NotTried)
            {
                currentPackSave.SetLevelState(currentLevelIndex, LevelTryState.Tried);
                dataManager.Save();
            }

            if (map.grid.totalButtons == previousTotalButtons)
            {
                firstStates.SetAll(false);
                emptyStates.SetAll(false);
            }
            else
            {
                firstStates = new BitArray(map.grid.totalButtons);
                emptyStates = new BitArray(map.grid.totalButtons);
            }

            previousTotalButtons = map.grid.totalButtons;
            map.grid.BackupStates(firstStates);

            gameManager.ResetCommandManager();

            clickCount = 0;
            ProcessHint();
            playRecord.minClickCount = minimumClickCount = hintSolutions.Count;

            gameManager.modeText.text = Locale.Get("classicmode.toptitle").Replace("%d", (currentLevelIndex + 1).ToString());
            gameManager.minimumClickCountText.text = minimumClickCount.ToString();
        }

        private void ResetPlayRecord()
        {
            playRecord = new ClassicPlayRecord
            {
                packName = currentPackSave.packName,
                levelIndex = currentLevelIndex
            };
        }

        private void OnHintButtonClicked()
        {
            if (!isSolved)
            {
                if (isPressedHintCoordinate)
                {
                    HintManager.Instance.UseHint(DoHint);
                    isPressedHintCoordinate = false;
                }
            }
        }

        private void DoHint(bool isSuccess)
        {
            if (isSuccess)
            {
                playRecord.hintCount++;

                ProcessHint();
                ShowHint();
            }
            else
            {
                isPressedHintCoordinate = true;
            }
        }

        private void ProcessHint()
        {
            compressor.Clear();
            var generateOrders = currentLevel.generateOrders;

            for (int i = 0; i < generateOrders.Count; i++)
            {
                compressor.Add(map.grid[generateOrders[i]]);
            }
            compressor.AddRange(playSequence);
            compressor.Compress(solverInputs, emptyStates);

            if (solverInputs.Count == 0)
            {
                hintSolutions.Clear();
                return;
            }

            // Debug.Log(solverInputs.Count);

            reverse.Clear();
            reverse.AddRange(solverInputs);
            reverse.Solve(hintSolutions);

            // Debug.Log("Solutions (Before Compress): " + hintSolutions.Count);

            compressor.Clear();
            compressor.AddRange(hintSolutions);
            compressor.Compress(hintSolutions, firstStates);

            // Debug.Log("Solutions (After Compress): " + hintSolutions.Count);


            /*StringBuilder sb = new StringBuilder();
            sb.Append("해결 순서: ");
            for (int i = 0; i < hintSolutions.Count; i++)
            {
                sb.Append(hintSolutions[i].coordinate + ", ");
            }
            Debug.Log(sb.ToString());*/
        }

        private void ShowHint()
        {
            GameObject hintMark = gameManager.hintMark;

            Vector3 firstHintPos = hintSolutions[0].coordinate.ToVector3();
            hintMark.SetActive(true);
            hintMark.transform.localPosition = firstHintPos - map.center;
        }

        private void OnNextButtonClicked()
        {
            gameTimer.ResetTimer();

            currentLevelIndex++;

            // 모든 맵을 클리어 했을 경우
            if (currentLevelIndex >= LevelManager.Instance.GetLevelCount(currentPackSave.packName))
            {
                SceneManager.LoadScene("MainScene");
                return;
            }

            playSequence.Clear();
            gameTimer.UpdateUI();

            LoadLevel(currentLevelIndex);
        }

        protected override void OnButtonClicked()
        {
            playRecord.totalClickCount++;
            playRecord.clickCount++;

            gameManager.hintMark.SetActive(false);
            isPressedHintCoordinate = true;

            if (!gameTimer.isWorking)
            {
                gameTimer.StartTimer();
                if (isFirstSinceLevelLoaded)
                {
                    startTime = Time.time;
                    isFirstSinceLevelLoaded = false;
                }
            }
        }

        protected override void OnClearMap()
        {
            gameTimer.StopTimer();

            // 세이브 파일에 '해결함'으로 저장하기
            LevelTryState levelState = currentPackSave.GetLevelState(currentLevelIndex);
            if (levelState != LevelTryState.Clear)
            {
                currentPackSave.SetLevelState(currentLevelIndex, LevelTryState.Clear);
            }

            if (clickCount < minimumClickCount)
            {
                // 최소 클릭수보다 적으면 버그 업적달성! ㅋㅋㅋㅋㅋㅋㅋ
                Achievement.AchievementMonitor.Instance.ReportBug();
            }

            int score = ScoreCalculator.Calculate(clickCount, minimumClickCount, 15);
            int oldStarCount = currentPackSave.GetStarCount(currentLevelIndex);
            int newStarCount = ScoreCalculator.CalculateStarCount(score);

            // 기존 별 개수와 비교하여 별 개수가 늘어났으면 세이브 파일에 반영하기
            if (newStarCount > oldStarCount)
            {
                currentPackSave.SetStarCount(currentLevelIndex, newStarCount);
            }
            dataManager.Save();

            // 일시정지 코루틴 시작
            gameManager.hintMark.SetActive(false);
            gameManager.clearDialog.Show(newStarCount, Locale.Get("classicmode.clearmessage").Replace("%d", clickCount.ToString()), Locale.Get("classicmode.nextlevel"));


            SendPlayDataToServer();
        }


        // 서버에 플레이 정보 보내기
        private async void SendPlayDataToServer()
        {
            playRecord.clearTime = Time.time - startTime;

            ClassicPlayRecord recordForServer = playRecord;
            ResetPlayRecord();
            var connection = WebConnection.Instance;
            await connection.SendClassicPlayData(recordForServer);
        }

        private IEnumerator WaitPauseCoroutine()
        {
            yield return null;
            gameManager.isInputFreeze = true;
        }

        private void OnSwipeDetected(SwipeDetector.Direction direction)
        {
            if (direction == SwipeDetector.Direction.Left)
            {
                currentLevelIndex--;
                if (currentLevelIndex < 0)
                    currentLevelIndex = currentPackSave.states.Count - 1;
            }
            else
            {
                currentLevelIndex++;
                if (currentLevelIndex > currentPackSave.states.Count - 1)
                    currentLevelIndex = 0;
            }
            LoadLevel(currentLevelIndex);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnSwipeDetected(SwipeDetector.Direction.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnSwipeDetected(SwipeDetector.Direction.Right);
            }
        }
#endif
    }
}
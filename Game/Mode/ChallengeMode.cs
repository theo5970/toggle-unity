using System.Collections;
using System.Collections.Generic;
using System.Text;
using Toggle.Client;
using Toggle.Core;
using Toggle.Core.Generator;
using Toggle.Core.Solver;
using Toggle.Game.Common;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Toggle.Game.Mode
{
    public class ChallengeMode : GameMode
    {
        private static readonly FunctionTypeFlags[] availableCombinations =
        {
            FunctionTypeFlags.OneArrowLinear,
            FunctionTypeFlags.TwoArrowLinear,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.TwoArrowLinear,
            FunctionTypeFlags.OneArrowDiagonal,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear,
            FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear |FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow,
            FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal | FunctionTypeFlags.FourArrow
        };

        private SafeInt clearCount = 0;
        private SafeInt totalScore = 0;

        private LevelGenerator generator = new LevelGenerator();
        private int requireClickCount = 1;
        private int difficulty = 0;
        private int scoreCombo = 250;

        private float lastGameTime = 0;


        public override void Prepare()
        {
            base.Prepare();

            gameManager.modeText.text = Locale.Get("challengemode.toptitle");
            gameManager.progressBarObject.SetActive(true);
            gameManager.challengeUI.SetActive(true);
            gameManager.clearDialog.canClose = false;

            InitGame();

            var nextButton = gameManager.clearDialog.nextButton;
            nextButton.onClick.AddListener(NextButtonClick);
        }

        private void NextButtonClick()
        {
            SceneManager.LoadScene("MainScene");
        }

        private void InitGame()
        {
            gameTimer.mode = GameTimer.Mode.CountDown;
            gameTimer.countdownTime = 90; // 1분 30초
            gameTimer.onCountdownEnd += CountdownEnd;
            gameTimer.UpdateUI();

            GenerateMap();
        }


        private void CountdownEnd()
        {
            gameManager.PauseGame(false);
            gameManager.isInputFreeze = true;

            string scoreThousand = string.Format("{0:#,0}", (int)totalScore);
            gameManager.restartButton.interactable = false;
            gameManager.clearDialog.Show(0, Locale.Get("challengemode.clearmessage").Replace("%d", scoreThousand), Locale.Get("challengemode.backtomenu"));

            // 구글 플레이에 점수 기록
            Social.ReportScore(totalScore, GPGSIds.leaderboard_timeattack, (bool success) =>
            {
                // if (success) Debug.Log("타임어택 점수 기록에 성공함.");
            });
        }



        private void GenerateMap()
        {
            isSolved = false;

            int combCount = availableCombinations.Length;
            int combInterval = combCount / 4;
            int combStartIndex = difficulty * combInterval;
            int combEndIndex = Mathf.Min(combCount - 1, combStartIndex + combInterval - 1);

            int combIndex = Random.Range(combStartIndex, combEndIndex + 1);

            var generatorOptions = new LevelGenerator.Options
            {
                buttonCount = Mathf.FloorToInt(requireClickCount * Random.Range(2.0f, 3.0f)),
                clickCount = requireClickCount,
                shouldPlaceEssentialType = true,
                shouldClickEssentialType = false,
                mapSize = new Vector2Int(5, 5),
                fourArrowPlaceLimit = 2,
                typeFlags = availableCombinations[combIndex]
            };

            // 빈 맵 만들기
            map.grid.Resize(generatorOptions.mapSize);
            map.GenerateEmptyMap();
            map.RefreshButtons();

            generator.grid = map.grid;
            generator.options = generatorOptions;

            var generateResult = generator.Generate();
            map.RefreshButtonsOutside();
            gameManager.BackupFirstStates();

            minimumClickCount = GetMinimumClickCount(generateResult.clickOrders);
            gameManager.minimumClickCountText.text = minimumClickCount.ToString();
            clickCount = 0;
        }

        private SequenceReverse solver = new SequenceReverse();
        private List<BaseButton> tempSolverOutput = new List<BaseButton>();

        private int GetMinimumClickCount(List<Vector2Int> clickOrders)
        {
            BitArray firstStates = new BitArray(map.grid.totalButtons);
            map.grid.BackupStates(firstStates);

            solver.Clear();
            for (int i = 0; i < clickOrders.Count; i++)
            {
                Vector2Int coord = clickOrders[i];
                solver.Add(map.grid[coord]);
            }

            solver.Solve(tempSolverOutput);

            compressor.Clear();
            compressor.AddRange(tempSolverOutput);
            compressor.Compress(tempSolverOutput, firstStates);

            return tempSolverOutput.Count;
        }
#if UNITY_EDITOR
        private void Update()
        {
            // Skip
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(SolveSimulation());
            }
        }

        private IEnumerator SolveSimulation()
        {
            for (int i = 0; i <= 100; i++)
            {
                for (int k = 0; k < tempSolverOutput.Count; k++)
                {
                    var buttonView = map.GetButtonView(tempSolverOutput[k].coordinate);
                    buttonView.OnClicked();

                    yield return new WaitForSeconds(0.25f);
                }
                yield return null;
            }

        }
#endif

        protected override void OnButtonClicked()
        {
            if (!gameTimer.isWorking)
            {
                gameTimer.StartTimer();
            }
        }

        protected override void OnClearMap()
        {
            float accuracy = ScoreCalculator.Calculate(clickCount, minimumClickCount, 20) / 100.0f;
            float clearTime = Time.time - lastGameTime;

            int timeToIncrease = Mathf.FloorToInt(Mathf.Clamp(clearTime * 2.0f, 1, 15));

            if (gameTimer.totalSeconds <= 75)
            {
                gameTimer.IncreaseTime(timeToIncrease);
                if (gameTimer.totalSeconds > 90)
                {
                    gameTimer.SetTime(90);
                }
            }

            lastGameTime = Time.time;

            int timeScore = Mathf.FloorToInt(Mathf.Clamp(2.0f / clearTime, 0.1f, 4) * 2500);
            int clickScore = (1 + (requireClickCount / 2)) * 1250;
            totalScore += Mathf.FloorToInt(timeScore * accuracy) + (clickScore + scoreCombo);

            gameManager.scoreText.text = string.Format("{0:#,0}", (int)totalScore);

            clearCount++;
            if (clearCount % 4 == 0)
            {
                requireClickCount = Mathf.Min(7, requireClickCount + 1);
                scoreCombo += 250;

                if (clearCount % 16 == 0)
                {
                    difficulty++;
                    requireClickCount -= 3;

                    if (difficulty > 3)
                    {
                        difficulty = 0;
                        requireClickCount = 1;
                    }
                }
            }
      
            gameManager.ResetCommandManager();
            GenerateMap();
        }
    }
}
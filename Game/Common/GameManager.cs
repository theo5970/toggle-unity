using System;
using System.Collections;
using TMPro;
using Toggle.Game.Mode;
using Toggle.Utils;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Toggle.LevelEditor;

namespace Toggle.Game.Common
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("General")] public bool inEditor = false;

        [Header("UI")]
        public TextMeshProUGUI modeText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI clickCountText;
        public TextMeshProUGUI minimumClickCountText;
        public Button restartButton;
        public Button undoButton;
        public Button redoButton;

        public LevelClearDialog clearDialog;

        public Button infoButton;
        public DialogPanel infoDialog;
        public GameObject challengeUI;
        public GameObject progressBarObject;


        [Header("Pause Menu")] public GameObject pauseMenu;
        public Button pauseButton;
        public Button continueButton;
        public Button rescanButton;
        public Button mainMenuButton;

        [Header("Hints")] public Button hintButton;
        public GameObject hintMark;

        private GameMap map;
        private BitArray firstStates;

        private GameTimer gameTimer;
        private DataManager dataManager;
        private CommandManager commandManager;

        public bool isInputFreeze;
        public bool isGamePaused { get; private set; }
        public event Action onGameRestart;

        private static Type currentModeType;
        public GameMode currentMode { get; set; }

        public static void SetGameMode(Type newGameModeType)
        {
            currentModeType = newGameModeType;
        }

        private void Awake()
        {
            firstStates = new BitArray(10000);
        }

        private void Start()
        {
            map = GameMap.Instance;
            commandManager = CommandManager.Instance;
            dataManager = DataManager.Instance;
            gameTimer = GameTimer.Instance;

            map.onLevelLoaded.AddListener(() => map.grid.BackupStates(firstStates));

            if (restartButton)
                restartButton.onClick.AddListener(RestartLevel);

            if (undoButton)
            {
                undoButton.onClick.AddListener(() =>
                {
                    commandManager.Undo();
                    CheckHistoryIsEmpty();
                });
            }

            if (redoButton)
            {
                redoButton.onClick.AddListener(() =>
                {
                    commandManager.Redo();
                    CheckHistoryIsEmpty();
                });
            }

            commandManager.onCommandRegister += OnCommandRegister;
            pauseMenu.SetActive(false);
            pauseButton.onClick.AddListener(() => PauseGame());
            continueButton.onClick.AddListener(ResumeGame);

            mainMenuButton.onClick.AddListener(() =>
            {
                isGamePaused = false;
                currentMode.OnBackToMainMenu();
                SceneStack.BackToPreviousScene();
            });



            if (dataManager.saveData.playCount == 0)
            {
                // 이 때 튜토리얼을 해야할 거 같다.
                currentMode = gameObject.AddComponent<TutorialMode>();
                currentMode.Prepare();

            }
            else
            {
                PrepareSelectedMode();
            }
        }

        public void PrepareSelectedMode()
        {
            if (currentMode != null)
            {
                Destroy(currentMode);
            }

            currentMode = gameObject.AddComponent(currentModeType) as GameMode;
            if (currentMode != null) currentMode.Prepare();

        }

        public void PauseGame(bool showPauseMenu = true)
        {
            Time.timeScale = 0;
            isGamePaused = true;
            pauseMenu.SetActive(showPauseMenu);
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            StartCoroutine(WaitOneFrameAndResumeGame());
        }

        private IEnumerator WaitOneFrameAndResumeGame()
        {
            yield return null;

            isGamePaused = false;
            pauseMenu.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isGamePaused) PauseGame();
            }
        }

        private void OnCommandRegister(Command command)
        {
            if (command is ButtonClickCommand)
                CheckHistoryIsEmpty();
        }

        private void CheckHistoryIsEmpty()
        {
            undoButton.interactable = !commandManager.IsUndoEmpty;
            redoButton.interactable = !commandManager.IsRedoEmpty;
        }

        public void BackupFirstStates()
        {
            map.grid.BackupStates(firstStates);
        }

        private void RestartLevel()
        {
            map.grid.RestoreStates(firstStates);
            map.RefreshButtonsOutside();

            ResetCommandManager();
            onGameRestart?.Invoke();
        }

        public void ResetCommandManager()
        {
            commandManager.ClearHistory();
            CheckHistoryIsEmpty();
        }
    }
}
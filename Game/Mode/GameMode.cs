using System;
using System.Collections.Generic;
using Toggle.Core;
using Toggle.Core.Solver;
using Toggle.Game.Common;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.Mode
{
    public abstract class GameMode : MonoBehaviour
    {
        protected GameManager gameManager;
        protected GameMap map;
        protected CommandManager commandManager;
        protected DataManager dataManager;
        protected GameTimer gameTimer;

        protected List<BaseButton> playSequence = new List<BaseButton>(1024);
        protected ClickActionCompressor compressor = new ClickActionCompressor();

        protected int clickCount
        {
            get => _clickCount;
            set
            {
                _clickCount = value;
                gameManager.clickCountText.text = _clickCount.ToString();
            }
        }

        private SafeInt _clickCount;

        protected SafeInt minimumClickCount;

        public bool isSolved { get; protected set; }

        public event Action onButtonClick;
        public event Action onLevelCleared;
    
        public virtual void Prepare()
        {
            gameManager = GameManager.Instance;
            gameTimer = GameTimer.Instance;
            map = GameMap.Instance;
            commandManager = CommandManager.Instance;
            dataManager = DataManager.Instance;

            gameManager.onGameRestart += OnGameRestart;
            gameManager.hintButton.gameObject.SetActive(false);
            gameManager.infoButton.gameObject.SetActive(false);
            gameManager.progressBarObject.SetActive(false);
            gameManager.challengeUI.SetActive(false);

            commandManager.onUndo += OnCommandUndo;
            commandManager.onRedo += OnCommandRedo;
            commandManager.onCommandRegister += OnCommandRegister;
        
            gameManager.continueButton.onClick.AddListener(OnContinueButtonClick);
            gameManager.restartButton.onClick.AddListener(OnRestartButtonClick);

            dataManager.saveData.playCount++;
            dataManager.Save();
        }

        public virtual void OnBackToMainMenu() { }
        private void OnContinueButtonClick()
        {
            if (!isSolved)
            {
                gameTimer.StartTimer();
            }
        }

        private void OnRestartButtonClick()
        {
            isSolved = false;
            gameManager.isInputFreeze = false;
            gameManager.clearDialog.Hide();
        }

        protected virtual void OnGameRestart()
        {
            playSequence.Clear();
            dataManager.saveData.playCount++;
            dataManager.Save();

            clickCount = 0;
            isSolved = false;
        }

        protected virtual void OnDestroy()
        {
            gameManager.onGameRestart -= OnGameRestart;
        
            commandManager.onUndo -= OnCommandUndo;
            commandManager.onRedo -= OnCommandRedo;
            commandManager.onCommandRegister -= OnCommandRegister;
        }

        protected virtual void OnCommandRegister(Command command)
        {
            if (command is ButtonClickCommand buttonClickCommand)
            {
                dataManager.saveData.clickCount++;
                playSequence.Add(buttonClickCommand.button);
                clickCount++;

                OnButtonClicked();
                onButtonClick?.Invoke();

                if (!isSolved && map.grid.GetActiveCount() == 0)
                {
                    isSolved = true;
                    OnClearMap();
                    onLevelCleared?.Invoke();
                }
            }
        }

        protected virtual void OnButtonClicked() { }
        protected virtual void OnClearMap() { }

        private void OnCommandRedo(Command command)
        {
            if (command is ButtonClickCommand buttonClickCommand)
            {
                playSequence.Add(buttonClickCommand.button);
                clickCount++;
            }
        }

        private void OnCommandUndo(Command command)
        {
            if (command is ButtonClickCommand buttonClickCommand)
            {
                if (playSequence.Count > 0)
                {
                    int endIndex = playSequence.Count - 1;
                    var lastButton = playSequence[endIndex];
                    if (buttonClickCommand.button == lastButton)
                    {
                        playSequence.RemoveAt(endIndex);
                    }
                }
                clickCount--;
            }
        }
    
    }
}
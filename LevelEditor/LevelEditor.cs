using System.Collections;
using System.Collections.Generic;
using TMPro;
using Toggle.Core;
using Toggle.Core.Function;
using Toggle.Core.Solver;
using Toggle.Game.Achievement;
using Toggle.Game.Common;
using Toggle.Game.UI;
using Toggle.LevelEditor.Palette;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Toggle.LevelEditor
{
    public class LevelEditor : Singleton<LevelEditor>
    {
        private EditorDataManager dataManager;
        private static EditorToggleLevel currentToggleLevel;


        public static void SetCurrentLevel(EditorToggleLevel data)
        {
            currentToggleLevel = data;
        }

        [Header("UI")] public Button clearAllButton;
        public Button resetStateButton;
        
        public Button saveButton;
        public Button exitButton;

        public EditorTypePalette typePalette;

        public TextMeshProUGUI pathText;
        public DialogPanel questionDialog;

        [Header("Editor Mode")] public RadioBoxGroup editorModeRadioGroup;

        private ClickActionCompressor compressor;
        private SequenceReverse sequenceReverse;

        private BitArray emptyStates;
        private GameMap map;
        private ButtonGrid grid;
        private GameManager gameManager;
        private CommandManager commandManager;

        private bool isLevelChanged = false;


        public LevelEditorMode editorMode { get; private set; } = LevelEditorMode.Click;

        void Start()
        {
            map = GameMap.Instance;
            grid = map.grid;
            gameManager = GameManager.Instance;
            commandManager = CommandManager.Instance;
            dataManager = EditorDataManager.Instance;

            emptyStates = new BitArray(grid.totalButtons);
            emptyStates.SetAll(false);

            compressor = new ClickActionCompressor();
            sequenceReverse = new SequenceReverse();

            resetStateButton.onClick.AddListener(ResetStateButtonClicked);
            clearAllButton.onClick.AddListener(ClearAllButtonClicked);

            saveButton.onClick.AddListener(SaveCurrentLevel);
            exitButton.onClick.AddListener(TryExit);

            UpdateTitleText();
            LoadCurrentLevel();

            AchievementManager.Instance.UnlockAchievement(GPGSIds.achievement_newcustomlevel, null);
        }

        private void ClearAllButtonClicked()
        {
            questionDialog.Show(Locale.Get("leveleditor.removeall.dialog"), index =>
            {
                questionDialog.Close();
                switch (index)
                {
                    case 0:
                        ClearAll();
                        break;
                }
            }, Locale.Get("default.ok"), Locale.Get("default.cancel"));
        }

        private void ClearAll()
        {
            grid.SetAllState(false);
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    map.grid[x, y].functionSubType = FunctionSubType.NOP;
                }
            }

            tempCompressorOrders.Clear();
            map.RefreshButtonsOutside();
        }

        private void ResetStateButtonClicked()
        {
            questionDialog.Show(Locale.Get("leveleditor.resetstate.dialog"), index =>
            {
                questionDialog.Close();
                switch (index)
                {
                    case 0:
                        grid.SetAllState(false);
                        tempCompressorOrders.Clear();
                        break;
                }
            }, Locale.Get("default.ok"), Locale.Get("default.cancel"));
        }

        private void Update()
        {
            switch (editorModeRadioGroup.selectedIndex)
            {
                case 0:
                    editorMode = LevelEditorMode.SetFunction;
                    break;
                case 1:
                    editorMode = LevelEditorMode.Click;
                    break;
                case 2:
                    editorMode = LevelEditorMode.Remove;
                    break;
            }
        }

        public void TryExit()
        {
            if (!isLevelChanged)
            {
                SceneManager.LoadScene("EditorSelectScene");
                return;
            }

            questionDialog.Show(Locale.Get("leveleditor.savedialog"), index =>
            {
                questionDialog.Close();
                switch (index)
                {
                    case 0:
                        SaveCurrentLevel();
                        break;
                    case 1:
                        break;
                    case 2:
                        return;
                }

                SceneManager.LoadScene("EditorSelectScene");
            }, Locale.Get("leveleditor.savedialog.save"), Locale.Get("leveleditor.savedialog.dontsave"), Locale.Get("default.cancel"));
        }

        public void UpdateTitleText()
        {
            if (currentToggleLevel != null)
            {
                pathText.text = currentToggleLevel.title + (isLevelChanged ? " [*]" : string.Empty);
            }
        }

        public void NotifyChanged()
        {
            if (!isLevelChanged)
            {
                isLevelChanged = true;
                UpdateTitleText();
            }
        }

        public void LoadCurrentLevel()
        {
            map.LoadLevel(currentToggleLevel.data);
            isLevelChanged = false;

            emptyStates = new BitArray(grid.totalButtons);
            emptyStates.SetAll(false);

            tempCompressorOrders.Clear();
            for (int i = 0; i < currentToggleLevel.generateOrders.Count; i++)
            {
                tempCompressorOrders.Add(grid[currentToggleLevel.generateOrders[i]]);
            }
            compressor.Clear();
            compressor.AddRange(tempCompressorOrders);

            UpdateTitleText();
        }

        public void SaveCurrentLevel()
        {
            if (isLevelChanged)
            {
                currentToggleLevel.isVerified = false;
                currentToggleLevel.minimumClickCount = int.MaxValue;
            }

            ApplyToData();

            currentToggleLevel.editTimeStamp = CommonUtils.GetCurrentTimeStamp();
            currentToggleLevel.generateOrders.Clear();
            for (int i = 0; i < tempCompressorOrders.Count; i++)
            {
                currentToggleLevel.generateOrders.Add(tempCompressorOrders[i].coordinate);
            }

            dataManager.SaveAll();

            isLevelChanged = false;
            UpdateTitleText();
        }

        // EditorLevelData에 적용
        public void ApplyToData()
        {
            grid.CopyToLevel(currentToggleLevel.data);

            isLevelChanged = false;
            UpdateTitleText();
        }

        // 맵 내의 버튼이 클릭됬을 때
        public void OnMapButtonClicked(BaseButtonView view)
        {
            switch (editorMode)
            {
                case LevelEditorMode.Click:
                    SolveAnyButtonClick(view);
                    break;
                case LevelEditorMode.SetFunction:
                    SolveSetFunctionType(view, ConvertMap.IntToType(typePalette.selectedType));
                    break;
                case LevelEditorMode.Remove:
                    SolveSetFunctionType(view, FunctionSubType.NOP);
                    break;
            }
        }

        private void SolveSetFunctionType(BaseButtonView view, FunctionSubType subType)
        {
            FunctionSubType targetSubType = subType;
            if (view.button.functionSubType == targetSubType) return;

            // 회전 버튼을 가장자리에 설치하지 못하게 막는다.
            bool isRotateAndPlaceAtBoundary = false;
            if (targetSubType == FunctionSubType.RC || targetSubType == FunctionSubType.RCC)
            {
                Vector2Int coord = view.button.coordinate;

                if ((coord.x == 0 || coord.x == map.grid.width - 1) 
                    || (coord.y == 0 || coord.y == map.grid.height - 1))
                {
                    isRotateAndPlaceAtBoundary = true;
                }
            }
            if (isRotateAndPlaceAtBoundary)
            {
                questionDialog.Show(Locale.Get("leveleditor.messages.rotateedge"), null, Locale.Get("default.ok"));
                return;
            }

            // map.SetFunction(button.gameObject, targetType);
            var command = new SetFunctionCommand(map, view, targetSubType);
            commandManager.Register(command);
            command.Execute();

            RemoveButtonFromGenerateOrders(view.button);
            ReconstructStates();


            NotifyChanged();
        }

        private List<BaseButton> tempCompressorOrders = new List<BaseButton>();

        private void SolveAnyButtonClick(BaseButtonView view)
        {
            if (view.button.functionSubType == FunctionSubType.NOP) return;

            compressor.Clear();
            compressor.AddRange(tempCompressorOrders);
            compressor.Add(view.button);
            compressor.Compress(tempCompressorOrders, emptyStates);

            NotifyChanged();
        }


        private void RemoveButtonFromGenerateOrders(BaseButton button)
        {
            for (int i = 0; i < tempCompressorOrders.Count; i++)
            {
                var otherButton = tempCompressorOrders[i];
                if (otherButton.coordinate == button.coordinate)
                {
                    tempCompressorOrders.RemoveAt(i--);
                }
            }
        }

        private void ReconstructStates()
        {
            grid.SetAllState(false);

            for (int i = 0; i < tempCompressorOrders.Count; i++)
            {
                var button = tempCompressorOrders[i];
                button.Action();
            }
        }
    }
}
using System.Collections;
using System.Text;
using Toggle.Game.Common;
using Toggle.LevelEditor;
using Toggle.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Toggle.Game.Mode
{
    public class CustomPlayMode : GameMode
    {
        private static EditorToggleLevel currentToggleLevel;

        public static void SetCurrentLevel(EditorToggleLevel data)
        {
            currentToggleLevel = data;
        }

        private EditorDataManager editorDataManager;
    
        private UnityEngine.UI.Button nextButton;
        private UnityEngine.UI.Button continueButton;
    

        private BitArray firstStates;
        private int oldMinimumClickCount = int.MaxValue;

        public override void Prepare()
        {
            base.Prepare();
            editorDataManager = EditorDataManager.Instance;

            nextButton = gameManager.clearDialog.nextButton;
            continueButton = gameManager.continueButton;
            map.LoadLevel(currentToggleLevel.data);
            firstStates = new BitArray(map.grid.width * map.grid.height);
            map.grid.BackupStates(firstStates);

            if (currentToggleLevel.isVerified)
            {
                oldMinimumClickCount = currentToggleLevel.minimumClickCount;

                if (oldMinimumClickCount == int.MaxValue)
                {
                    gameManager.minimumClickCountText.text = "∞";
                }
                else
                {
                    gameManager.minimumClickCountText.text = oldMinimumClickCount.ToString();
                }
            }
            nextButton.onClick.AddListener(OnNextButtonClicked);

            gameManager.modeText.text = Locale.Get("customplaymode.toptitle");

        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                StartCoroutine(TestSolveCoroutine());
            }

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
            currentToggleLevel.isVerified = true;

            SaveSolveSequence();

            gameTimer.StopTimer();
            editorDataManager.SaveAll();

            // 일시정지 코루틴 시작
            StartCoroutine(WaitPauseCoroutine());
            gameManager.clearDialog.Show(3, Locale.Get("customplaymode.clearmessage").Replace("%d", clickCount.ToString()), Locale.Get("customplaymode.backtomenu"));
        }

        private void SaveSolveSequence()
        {
            compressor.Clear();
            compressor.AddRange(playSequence);
            playSequence.Clear();
            compressor.Compress(playSequence, firstStates);

            if (clickCount < oldMinimumClickCount)
            {
                gameManager.minimumClickCountText.text = $"{clickCount}*";
                currentToggleLevel.minimumClickCount = clickCount;
            }
        

            var orders = currentToggleLevel.orders;
            orders.Clear();
            for (int i = 0; i < playSequence.Count; i++)
            {
                orders.Add(playSequence[i].coordinate);
            }
        }
    
        private void OnNextButtonClicked()
        {
            SceneStack.BackToPreviousScene();
        }
    

        private IEnumerator WaitPauseCoroutine()
        {
            yield return null;
            gameManager.isInputFreeze = true;
        }

        private IEnumerator TestSolveCoroutine()
        {
            var solveOrders = currentToggleLevel.orders;
            for (int i = 0; i < solveOrders.Count; i++)
            {
                Vector2Int coord = solveOrders[i];
                map.grid[coord].Action();
            
                yield return new WaitForSeconds(0.5f);
            }
        }

    }
}
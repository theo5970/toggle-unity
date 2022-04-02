using System.Collections;
using UnityEngine;

using Toggle.Game.Common;
using Toggle.Game.Tutorial;
using Toggle.Core;

namespace Toggle.Game.Mode
{
    public class TutorialMode : GameMode
    {
        private ToggleLevel level0 = new ToggleLevel(ToggleLevelReader.FromBase64("Y2RkZWJlZoAARgZ0wMJQwcDAChIHAA=="));
        private ToggleLevel level1 = new ToggleLevel(ToggleLevelReader.FromBase64("Y2RkZWJlZgADRgYEgAqxnFBiYWAFsQA="));
        private ToggleLevel level2 = new ToggleLevel(ToggleLevelReader.FromBase64("Y2RkZWJlZgADRmYgg5GBiYGBhYUJIsTS0MTEwApiAQA="));

        private TutorialGuideUI guideUI;
        private GameObject tutorialCanvasObj;
        public override void Prepare()
        {
            base.Prepare();
            gameManager.modeText.text = Locale.Get("tutorial.title");


            tutorialCanvasObj = Instantiate(Resources.Load<GameObject>("Prefabs/TutorialCanvas"));
            guideUI = tutorialCanvasObj.GetComponent<TutorialGuideUI>();

            gameManager.isInputFreeze = true;
            gameManager.undoButton.interactable = false;
            gameManager.redoButton.interactable = false;
            gameManager.restartButton.interactable = false;


            StartCoroutine(TutorialRoutine());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }


        private IEnumerator WaitForAnyTouch()
        {
            bool isInputDetected;
            do
            {
                isInputDetected = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
                yield return null;
            } while (!isInputDetected || TouchUtils.IsPointerOverGameObject());
        }

        private IEnumerator WaitForSolve()
        {
            while (map.grid.GetActiveCount() > 0)
            {
                yield return null;
            }
        }

        private WaitForSeconds waitForSomeTouchDelay = new WaitForSeconds(0.15f);
        private IEnumerator TutorialRoutine()
        {
            map.grid.Resize(5, 5);
            map.GenerateEmptyMap();
            guideUI.SetText(Locale.Get("tutorial.1"));
            yield return WaitForAnyTouch();


            guideUI.SetText(Locale.Get("tutorial.2"));
            yield return WaitForAnyTouch();

            map.LoadLevel(level0);
            guideUI.SetText(Locale.Get("tutorial.3"));
            yield return waitForSomeTouchDelay;
            gameManager.isInputFreeze = false;
            yield return WaitForSolve();

            gameManager.isInputFreeze = true;
            gameManager.ResetCommandManager();
            guideUI.SetText(Locale.Get("tutorial.4"));
            yield return WaitForAnyTouch();

            guideUI.SetText(Locale.Get("tutorial.5"));
            map.LoadLevel(level1);
            yield return WaitForAnyTouch();
            guideUI.SetText(Locale.Get("tutorial.6"));
            clickCount = 0;
            yield return waitForSomeTouchDelay;
            gameManager.isInputFreeze = false;
            yield return WaitForSolve();

            guideUI.SetText(Locale.Get("tutorial.7"));
            gameManager.isInputFreeze = true;
            gameManager.ResetCommandManager();
            yield return WaitForAnyTouch();

            guideUI.SetText(Locale.Get("tutorial.8"));
            yield return WaitForAnyTouch();

            guideUI.SetText(Locale.Get("tutorial.9"));
            map.LoadLevel(level2);
            clickCount = 0;
            yield return waitForSomeTouchDelay;
            gameManager.restartButton.interactable = true;
            gameManager.isInputFreeze = false;
            yield return WaitForSolve();

            guideUI.SetText(Locale.Get("tutorial.10"));
            yield return new WaitForSeconds(2.0f);

            gameManager.ResetCommandManager();
            gameManager.isInputFreeze = false;
            gameManager.PrepareSelectedMode();
            Destroy(tutorialCanvasObj);
        }
    }
}
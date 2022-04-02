using System;
using System.Collections;
using QRcode.Scripts;
using Toggle.Core;
using Toggle.Game.Common;
using Toggle.LevelEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QRPlay
{
    public class QRCodeLevelFetcher : MonoBehaviour
    {
        public delegate void ScanCompleteHandler(ToggleLevelReader.Result readerResult);

        private GameMap gameMap;
        private bool isAnyLevelLoaded = false;

        public GameObject scanScreenObject;

        public GameObject cameraCanvas;
        public QRCodeDecodeController decodeController;
        public Button cameraButton;
        public Button importClipboardButton;

        public event ScanCompleteHandler onScanComplete;

        public DialogPanel dialog;

        private PanelStack panelStack;

        

        private void Awake()
        {
            scanScreenObject.SetActive(false);
            panelStack = GetComponent<PanelStack>();
            panelStack.onBackToScene += OnBackToMainScene;
        }

        private void OnBackToMainScene()
        {
            decodeController.StopWork();
        }

        private void OnDestroy()
        {
            panelStack.onBackToScene -= OnBackToMainScene;
        }

        void Start()
        {
            gameMap = GameMap.Instance;
            decodeController.onQRScanFinished.AddListener(ApplyLevelCodeToGame);

            importClipboardButton.onClick.AddListener(ImportFromClipboard);
            cameraButton.onClick.AddListener(OnCameraButtonClicked);

        }


           
        


        private void Update()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
                {
                    ApplyLevelCodeToGame(UniClipboard.UniClipboard.GetText());
                }
            }
        }

        private void ImportFromClipboard()
        {
            ApplyLevelCodeToGame(UniClipboard.UniClipboard.GetText());
        }

        public void StartScan()
        {
            scanScreenObject.SetActive(true);
        }

        private void OnCameraButtonClicked()
        {
            decodeController.StartWork();
            panelStack.Push(cameraCanvas);
        }

        private void ApplyLevelCodeToGame(string text)
        {
            try
            {
                var readerResult = ToggleLevelReader.FromBase64(text);
                gameMap.LoadLevel(new ToggleLevel(readerResult));

                onScanComplete?.Invoke(readerResult);
                isAnyLevelLoaded = true;
                scanScreenObject.SetActive(false);
            }
            catch (Exception ex)
            {
                dialog.Show(Locale.Get("qrmode.invalid"), null, Locale.Get("default.ok"));
            }
            decodeController.Reset();
        }
    }
}
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QRcode.Scripts;
using System;
using TMPro;
using Toggle.Client;
using Toggle.Core;
using Toggle.Core.Function;
using Toggle.Core.Generator;
using Toggle.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.LevelEditor
{
    public class EditorLevelShareView : MonoBehaviour
    {
        public RawImage qrcodeImage;
        public TextMeshProUGUI infoText;
        public QRCodeEncodeController encodeController;
        public Button copyCodeButton;
        public Button uploadButton;
        public DialogPanel dialog;

        private Texture2D qrTexture;
        private bool isFirstTime = true;

        private static readonly Color32 verifiedBgColor = new Color32(177, 255, 181, 255);
        private static readonly Color32 notVerifiedBgColor = new Color32(255, 177, 174, 255);

        private string currentEncodedText;
        private WebConnection webConnection;

        private EditorToggleLevel currentEditorLevel;

        public void Show(EditorToggleLevel editorLevel)
        {
            currentEditorLevel = editorLevel;

            if (qrTexture == null)
            {
                qrTexture = new Texture2D(512, 512);
            }

            Image copyCodeButtonBackground = copyCodeButton.GetComponent<Image>();
            copyCodeButtonBackground.color = Color.white;
            qrcodeImage.color = currentEditorLevel.isVerified ? verifiedBgColor : notVerifiedBgColor;

            encodeController.e_QRCodeWidth = qrTexture.width;
            encodeController.e_QRCodeHeight = qrTexture.height;
            encodeController.eCodeFormat = QRCodeEncodeController.CodeMode.QR_CODE;

            using (ToggleLevelWriter writer = new ToggleLevelWriter())
            {
                writer.Write(currentEditorLevel.data);
                writer.Write(ToggleLevelBinaryProperties.MinimumClick, currentEditorLevel.minimumClickCount);

                webConnection = WebConnection.Instance;
                if (Social.localUser.authenticated)
                {
                    writer.Write(ToggleLevelBinaryProperties.Creator, Social.localUser.userName);
                }
                currentEncodedText = writer.ToBase64();
            }

            uploadButton.interactable = currentEditorLevel.isVerified;

            if (currentEditorLevel.isUploaded)
            {
                uploadButton.SetTranslationKey("leveleditor.share.updatelevel");
            }


            if (isFirstTime)
            {
                encodeController.onQREncodeFinished.AddListener(texture =>
                {
                    qrcodeImage.texture = texture;
                });

                copyCodeButton.onClick.AddListener(() =>
                {
                    UniClipboard.UniClipboard.SetText(currentEncodedText);
                    copyCodeButtonBackground.color = verifiedBgColor;
                });

                uploadButton.onClick.AddListener(() =>
                {
                    if (currentEditorLevel.isUploaded)
                    {
                        UploadToServer(currentEditorLevel, WebConnection.Instance.UpdateLevel);
                    }
                    else
                    {
                        UploadToServer(currentEditorLevel, WebConnection.Instance.UploadLevel);
                    }
                });

                isFirstTime = false;
            }

            if (currentEditorLevel.isUploaded)
            {
                infoText.text = string.Format("{0}\n<size=24>{1} x {2}\n\nID: {3}</size>", currentEditorLevel.title, currentEditorLevel.data.width, currentEditorLevel.data.height, currentEditorLevel.uploadId);
            }
            else
            {
                infoText.text = string.Format("{0}\n<size=24>{1} x {2}</size>", currentEditorLevel.title, currentEditorLevel.data.width, currentEditorLevel.data.height);
            }
            encodeController.Encode(currentEncodedText);
        }


        private async void UploadToServer(EditorToggleLevel editorLevel, Func<EditorToggleLevel, string, UniTask<UploadResult>> uploadFunc)
        {
            UploadResult result = await uploadFunc(editorLevel, currentEncodedText);

            if (result.isSuccess)
            {
                dialog.Show(Locale.Get("leveleditor.share.messages.success"), null, Locale.Get("default.ok"));

                editorLevel.isUploaded = true;
                if (result.uploadType == UploadResult.Type.Upload)
                {
                    editorLevel.uploadId = result.levelId;
                }

                uploadButton.SetTranslationKey("leveleditor.share.updatelevel");
                EditorDataManager.Instance.SaveAll();
            }
            else
            {
                dialog.Show(Locale.Get("leveleditor.share.messages.fail") + "\n" + result.failMessage, null, Locale.Get("default.ok"));
            }
        }
    }
}
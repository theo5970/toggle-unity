using System;
using System.Collections.Generic;
using Toggle.Game.Common;
using Toggle.Game.Mode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Toggle.LevelEditor
{
    public class EditorLevelSelectView : MonoBehaviour
    {
        private EditorToggleLevel selectedToggleLevel;

        public PanelStack panelStack;
        public Action<EditorToggleLevel> onLevelSelected;
        public EditorLevelSelectButton selectedButton;

        public Transform listTransform;
        public GameObject listViewObject;
        public GameObject infoPanel;
        public GameObject sharePanel;
        public GameObject buttonPrefab;

        [Header("Action Buttons")]
        public Button playButton;
        public Button editButton;
        public Button shareButton;
        public Button deleteButton;

        [Header("Share")] public EditorLevelShareView shareView;
    
        [Header("Other")]
        public DialogPanel dialog;

        private List<EditorLevelSelectButton> buttonList;
        private EditorDataManager dataManager;

        // Use this for initialization
        void Start()
        {
            buttonList = new List<EditorLevelSelectButton>();

            dataManager = EditorDataManager.Instance;
            dataManager.newLevelEvent.AddListener(CreateNewLevelButton);
            GenerateButtons();

            playButton.onClick.AddListener(() =>
            {
                CustomPlayMode.SetCurrentLevel(selectedToggleLevel);
                GameManager.SetGameMode(typeof(CustomPlayMode));
                SceneManager.LoadScene("PlayScene");
            });

            editButton.onClick.AddListener(() =>
            {
                LevelEditor.SetCurrentLevel(selectedToggleLevel);
                SceneManager.LoadScene("EditorScene");
            });
        
            shareButton.onClick.AddListener(() =>
            {
                panelStack.Push(sharePanel);
                shareView.Show(selectedToggleLevel);
            });
        
            deleteButton.onClick.AddListener(() =>
            {
                dialog.Show(Locale.Get("leveleditor.deletedialog.title"), (int index) =>
                {
                    dialog.Close();
                    switch (index)
                    {
                        case 0: // Yes
                            break;

                        case 1: // Cancel
                            return;
                    }

                    dataManager.DeleteLevel(selectedToggleLevel);
                    Destroy(selectedButton.gameObject);
                    Deselect();

                }, Locale.Get("default.ok"), Locale.Get("default.cancel"));
            });
        }

        private void GenerateButtons()
        {
            int count = dataManager.GetLevelCount();
            for (int i = 0; i < count; i++)
            {
                EditorToggleLevel toggleLevel = dataManager.GetLevelData(i);
                GameObject newObj = Instantiate(buttonPrefab);
                newObj.transform.SetParent(listTransform, false);

                var button = newObj.GetComponent<EditorLevelSelectButton>();
                button.Apply(toggleLevel, this);
                buttonList.Add(button);
            }
        }

        public void CreateNewLevelButton(EditorToggleLevel newToggleLevel)
        {
            GameObject newObj = Instantiate(buttonPrefab);
            newObj.transform.SetParent(listTransform, false);

            // 최신 레벨은 최상단 바로 아래쪽에 위치하도록 (맨 위에는 레벨 생성버튼 있음)
            newObj.transform.SetSiblingIndex(1);

            var button = newObj.GetComponent<EditorLevelSelectButton>();
            button.Apply(newToggleLevel, this);
            buttonList.Add(button);

            Select(button);
        }

        public void Select(EditorLevelSelectButton button)
        {
            selectedButton = button;
            selectedToggleLevel = button.ToggleLevel;

            panelStack.Push(infoPanel);
        
            onLevelSelected?.Invoke(button.ToggleLevel);
        }

        public void Deselect()
        {
            selectedToggleLevel = null;
            selectedButton = null;

            panelStack.Pop();
        }
    }
}

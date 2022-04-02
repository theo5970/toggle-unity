using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Toggle.Client;

namespace Toggle.LevelSearch
{
    public class LevelSearchResultButton : MonoBehaviour
    {
        public OnlineLevel levelData { get; private set; }
        public HorizontalLayoutGroup horizontalLayoutGroup;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI creatorText;
        public TextMeshProUGUI downloadCountText;
        public TextMeshProUGUI likeCountText;


        public Button playButton;

        [HideInInspector]
        public LevelSearchResultView view;

        public void Apply(LevelSearchResultView view, OnlineLevel levelData)
        {
            this.view = view;
            this.levelData = levelData;

            RefreshUI();

            LayoutRebuilder.ForceRebuildLayoutImmediate(likeCountText.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(downloadCountText.GetComponent<RectTransform>());

            horizontalLayoutGroup.CalculateLayoutInputHorizontal();
            horizontalLayoutGroup.CalculateLayoutInputVertical();
            horizontalLayoutGroup.SetLayoutHorizontal();
            horizontalLayoutGroup.SetLayoutVertical();
        }


        private void OnEnable()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (levelData != null)
            {
                titleText.text = levelData.title;
                creatorText.text = levelData.creator;
                downloadCountText.text = string.Format("{0:#,0}", levelData.downloadCount);
                likeCountText.text = string.Format("{0:#,0}", levelData.likeCount);
            }
        }

        private void Awake()
        {
            playButton.onClick.AddListener(() =>
            {
                view.ShowInfoPanel(this);
            });
        }
    }
}
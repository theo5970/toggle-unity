using System.Collections;
using System.Collections.Generic;
using Toggle.Client;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.LevelSearch
{
    public class LevelSearchResultView : MonoBehaviour
    {
        public Button previousPageButton;
        public Button nextPageButton;
        public Button refreshButton;

        public GameObject buttonPrefab;

        private static LevelSearchOptions options = new LevelSearchOptions();

        private int totalPages;

        public Transform listRoot;


        public PanelStack panelStack;
        public GameObject infoPanel;

        private void Start()
        {
            previousPageButton.interactable = false;
            nextPageButton.interactable = true;

            previousPageButton.onClick.AddListener(() =>
            {
                options.page = Mathf.Clamp(options.page - 1, 1, totalPages);
                UpdatePageButtonStates();
                StartSearchAsync();
            });

            nextPageButton.onClick.AddListener(() =>
            {
                options.page = Mathf.Clamp(options.page + 1, 1, totalPages);
                UpdatePageButtonStates();
                StartSearchAsync();
            });

            refreshButton.onClick.AddListener(() =>
            {
                StartSearchAsync();
            });
        }

        private void UpdatePageButtonStates()
        {
            previousPageButton.interactable = options.page > 1;
            nextPageButton.interactable = options.page < totalPages;
        }

        public void SearchWithFilter(LevelSearchOptions.Filter filter)
        {
            options.page = 1;
            options.filter = filter;
            options.keyword = string.Empty;

            StartSearchAsync();
        }

        public void SearchWithKeyword(string keyword)
        {
            options.page = 1;
            options.filter = LevelSearchOptions.Filter.Keyword;
            options.keyword = keyword;

            StartSearchAsync();
        }

        private async void StartSearchAsync()
        {
            SearchResult searchResult = await WebConnection.Instance.SearchLevels(options);
            totalPages = searchResult.totalPages;

            foreach (Transform tr in listRoot)
            {
                Destroy(tr.gameObject);
            }

            if (searchResult.levels != null)
            {
                foreach (var onlineLevel in searchResult.levels)
                {
                    GameObject newObj = Instantiate(buttonPrefab, listRoot);

                    var resultButton = newObj.GetComponent<LevelSearchResultButton>();
                    resultButton.Apply(this, onlineLevel);
                }
            }

            UpdatePageButtonStates();
        }


        public void ShowInfoPanel(LevelSearchResultButton button)
        {
            panelStack.Push(infoPanel);
            infoPanel.GetComponent<LevelSearchInfoView>().Apply(button.levelData);
        }
    }
}
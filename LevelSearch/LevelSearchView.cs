using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Toggle.LevelSearch
{
    public class LevelSearchView : MonoBehaviour
    {
        private PanelStack panelStack;

        public Button mostRecentButton;
        public Button mostDownloadedButton;
        public Button mostLikedButton;
        public Button leastClearedButton;
        public TMP_InputField keywordField;

        public Button searchButton;
        public GameObject mainPanel;
        public GameObject listPanel;

        private LevelSearchResultView resultView;


        // Start is called before the first frame update
        void Start()
        {
            LevelDownloadCache.Instance.Load();

            resultView = listPanel.GetComponent<LevelSearchResultView>();

            panelStack = GetComponent<PanelStack>();
            panelStack.Push(mainPanel);


            mostRecentButton.onClick.AddListener(() => SearchWithFilter(LevelSearchOptions.Filter.MostRecent));
            mostDownloadedButton.onClick.AddListener(() => SearchWithFilter(LevelSearchOptions.Filter.MostDownloaded));
            mostLikedButton.onClick.AddListener(() => SearchWithFilter(LevelSearchOptions.Filter.MostLiked));
            leastClearedButton.onClick.AddListener(() => SearchWithFilter(LevelSearchOptions.Filter.FewCleared));

            searchButton.onClick.AddListener(() =>
            {
                SearchWithKeyword(keywordField.text);
            });
        }

        public void SearchWithFilter(LevelSearchOptions.Filter filter)
        {
            panelStack.Push(listPanel);
            resultView.SearchWithFilter(filter);
        }

        public void SearchWithKeyword(string keyword)
        {
            panelStack.Push(listPanel);
            resultView.SearchWithKeyword(keyword);
        }
    }
}

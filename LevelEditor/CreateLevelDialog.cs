using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.LevelEditor
{
    public class CreateLevelDialog : MonoBehaviour
    {
        public GameObject blackPanel;
        private EditorDataManager editorDataManager;

        [Header("Sliders")] public Slider widthSlider;
        public Slider heightSlider;

        [Header("Buttons")] public Button okButton;
        public Button cancelButton;

        [Header("Fields")] public TMP_InputField titleField;


        private static readonly char[] allowedSpecialCharacters = {'.', '!', '-', '_', '?', '+', ' '};

        private bool CheckIsValidTitle(string title)
        {
            int count = title.Length;
            for (int i = 0; i < count; i++)
            {
                char ch = title[i];

                if (char.IsControl(ch) || char.IsSeparator(ch) || char.IsSymbol(ch))
                {
                    if (!allowedSpecialCharacters.Contains(ch)) return false;
                }
            }

            return true;
        }

        private void Awake()
        {
            okButton.onClick.AddListener(() =>
            {
                int columns = Mathf.FloorToInt(widthSlider.value);
                int rows = Mathf.FloorToInt(heightSlider.value);

                editorDataManager.CreateEmptyLevel(titleField.text, columns, rows);
                gameObject.SetActive(false);
            });

            cancelButton.onClick.AddListener(() => { gameObject.SetActive(false); });

            titleField.onEndEdit.AddListener((text) =>
            {
                bool cond1 = (text.Length >= 1) && (text[0] != ' ');
                bool cond2 = CheckIsValidTitle(text.Trim());

                okButton.interactable = cond1 && cond2;
            });
        }

        // Use this for initialization
        void Start()
        {
            editorDataManager = EditorDataManager.Instance;
        }

        private void OnEnable()
        {
            blackPanel.SetActive(true);
        }

        private void OnDisable()
        {
            blackPanel.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Toggle.LevelEditor
{
    public class DialogPanel : MonoBehaviour
    {
        public GameObject blackPanel;
        public TextMeshProUGUI titleText;
        public List<UnityEngine.UI.Button> buttons;

        void Start()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                int k = i;
                buttons[i].onClick.AddListener(() =>
                {
                    OnButtonClicked(k);
                });
            }
        }

        private System.Action<int> currentEvent;
        public void Show(string title, System.Action<int> clickedEvent, params string[] buttonMessages)
        {
            gameObject.SetActive(true);
            titleText.text = title;
            if (blackPanel != null) blackPanel.SetActive(true);

            int textCount = buttonMessages.Length;
            for (int i = 0; i < textCount; i++)
            {
                buttons[i].gameObject.SetActive(true);

                TextMeshProUGUI btnText = buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                btnText.text = buttonMessages[i];
            }
            for (int i = textCount; i < buttons.Count; i++)
            {
                buttons[i].gameObject.SetActive(false);
            }

            currentEvent = clickedEvent;
        }

        public void Close()
        {
            currentEvent = null;
            gameObject.SetActive(false);
            if (blackPanel != null) blackPanel.SetActive(false);
        }

        private void OnButtonClicked(int index)
        {
            if (currentEvent != null)
            {
                currentEvent.Invoke(index);
            }
            Close();
        }
    }
}

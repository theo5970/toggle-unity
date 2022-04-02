using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Toggle.Game.UI
{
    public class RadioBox : MonoBehaviour, IPointerClickHandler
    {
        public RadioBoxSkin skin;

        public bool state
        {
            get => _state;
            set
            {
                _state = value;
                onStateChanged?.Invoke(_state);
                UpdateNewValue(_state);
            }
        }

        public event System.Action<bool> onStateChanged;

        private bool _state;
    
        private int id;
        private RadioBoxGroup group;

        private Image background;
        private List<Image> imageList;
        private List<TextMeshProUGUI> tmproList;


        // Start is called before the first frame update
        void Awake()
        {
            background = GetComponent<Image>();

            imageList = new List<Image>();
            tmproList = new List<TextMeshProUGUI>();

            foreach (Image image in transform.GetComponentsInChildren<Image>())
            {
                if (image.transform.parent == transform)
                {
                    imageList.Add(image);
                }
            };
            foreach (TextMeshProUGUI tmpro in transform.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (tmpro.transform.parent == transform)
                {
                    tmproList.Add(tmpro);
                }
            };

            state = false;
        }

        public void SetGroup(RadioBoxGroup _group, int childId)
        {
            group = _group;
            id = childId;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            state = !state;

            if (group != null)
            {
                group.SetSelectedIndex(id);
            }
        }

        private void UpdateNewValue(bool newState)
        {
            Color backgroundColor = newState ? skin.highlightBackgroundColor : skin.normalBackgroundColor;
            Color foregroundColor = newState ? skin.highlightForegroundColor : skin.normalForegroundColor;

            background.color = backgroundColor;
            for (int i = 0; i < imageList.Count; i++)
            {
                imageList[i].color = foregroundColor;
            }

            for (int i = 0; i < tmproList.Count; i++)
            {
                tmproList[i].color = foregroundColor;
            }
        }
    }
}
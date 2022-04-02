using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
namespace Toggle.Game.MainMenu
{
    public class LanguageButton : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector]
        public int id;

        [HideInInspector]
        public LanguageSettingsPanel parent;
        public void OnPointerClick(PointerEventData eventData)
        {
            parent.OnChildClicked(id);
        }

        private static readonly Color colorSelected = new Color32(252, 144, 3, 255);
        private static readonly Color colorNormal = new Color32(143, 143, 143, 255);
        public bool isSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                textUI.color = _isSelected ? colorSelected : colorNormal;
            }
        }
        private bool _isSelected;

        private TextMeshProUGUI textUI;
        void Awake()
        {
            textUI = GetComponent<TextMeshProUGUI>();
            isSelected = false;
        }

        public void SetText(string text)
        {
            textUI.text = text;
        }
    }
}
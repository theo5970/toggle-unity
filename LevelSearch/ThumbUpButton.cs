using System.Collections;
using System.Collections.Generic;
using Toggle.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.LevelSearch
{
    public class ThumbUpButton : MonoBehaviour
    {
        private Button button;
        public bool state
        {
            get => _state;
            set
            {
                _state = value;

                if (_state)
                {
                    button.SetText("\ue817");
                    button.targetGraphic.color = new Color32(10, 144, 247, 255);
                }
                else
                {
                    button.SetText("\ue9f3");
                    button.targetGraphic.color = new Color32(168, 168, 168, 255);
                }
            }
        }
        private bool _state;

        public event System.Action onClick;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                state = !state;
                onClick?.Invoke();
            });
        }
    }
}
using System.Collections;
using UnityEngine;
using TMPro;

namespace Toggle.Game.UI
{
    public class RadioBoxText : MonoBehaviour
    {
        private RadioBox radioBox;
        private LocaleTMPUI localeTMPUI;
        void Awake()
        {
            radioBox = transform.parent.GetComponent<RadioBox>();
            radioBox.onStateChanged += OnStateChanged;

            localeTMPUI = GetComponent<LocaleTMPUI>();

            Locale.onLoad += OnLocaleLoad;
        }

        private void OnLocaleLoad()
        {
            localeTMPUI.SetTranslationKey(radioBox.state ? "radio.on" : "radio.off");
        }

        private void OnStateChanged(bool newState)
        {
            localeTMPUI.SetTranslationKey(newState ? "radio.on" : "radio.off");
        }

        private void OnDestroy()
        {
            radioBox.onStateChanged -= OnStateChanged;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Toggle.Game.Common;
using TMPro;

namespace Toggle.Game.Tutorial
{
    public class TutorialGuideUI : MonoBehaviour
    {
        public TextMeshProUGUI guideText;
        private Camera cam;
        private RectTransform rectTransform;
        private GameMap gameMap;

        void Start()
        {
            cam = Camera.main;
            gameMap = GameMap.Instance;

        }

        public void SetText(string text)
        {
            guideText.text = text;
        }
    }
}
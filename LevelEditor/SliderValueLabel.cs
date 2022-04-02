using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.LevelEditor
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SliderValueLabel : MonoBehaviour
    {
        public Slider slider;
        private TextMeshProUGUI text;
        void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
            slider.onValueChanged.AddListener((float newValue) =>
            {
                text.text = Mathf.FloorToInt(newValue).ToString();
            });
        }
    }
}

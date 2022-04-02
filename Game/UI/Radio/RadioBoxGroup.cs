using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toggle.Game.UI
{
    public class RadioBoxGroup : MonoBehaviour
    {
        public int selectedIndex { get; private set; } = 0;
        public int defaultIndex = 0;
        private List<RadioBox> radioBoxes;

        public RadioBox this[int index] => radioBoxes[index];

        private void Awake()
        {
            selectedIndex = defaultIndex;
        }

        private IEnumerator Start()
        {
            int childCount = transform.childCount;
            radioBoxes = new List<RadioBox>();

            int counter = 0;
            for (int i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out RadioBox radioBox))
                {
                    radioBox.SetGroup(this, counter++);
                    radioBoxes.Add(radioBox);
                }
            }

            yield return null;
            SetSelectedIndex(selectedIndex);
        }

        public void SetSelectedIndex(int index)
        {
            selectedIndex = index;

            for (int i = 0; i < radioBoxes.Count; i++)
            {
                radioBoxes[i].state = (i == selectedIndex);
            }
        }
    }
}
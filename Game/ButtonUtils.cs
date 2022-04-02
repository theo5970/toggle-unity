using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.Game
{
    public static class ButtonUtils
    {
        public static void SetText(this Button button, string text)
        {
            if (button.transform.childCount == 0) return;

            if (button.transform.GetChild(0).TryGetComponent(out TextMeshProUGUI textUI))
            {
                textUI.text = text;
            }
            else
            {
                if (button.transform.GetChild(1).TryGetComponent(out TextMeshProUGUI textUI2))
                {
                    textUI2.text = text;
                }
            }
        }


        public static void SetTranslationKey(this Button button, string translationKey)
        {
            if (button.transform.childCount == 0) return;

            if (button.transform.GetChild(0).TryGetComponent(out LocaleTMPUI localeTMPUI))
            {
                localeTMPUI.SetTranslationKey(translationKey);
            }
            else
            {
                if (button.transform.GetChild(1).TryGetComponent(out LocaleTMPUI localeTMPUI2))
                {
                    localeTMPUI2.SetTranslationKey(translationKey);
                }
            }
        }
    }
}

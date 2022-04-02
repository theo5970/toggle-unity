using System.Collections;
using System.Collections.Generic;
using Toggle.Game.Common;
using UnityEngine;

namespace Toggle.Game.MainMenu
{
    public class LanguageSettingsPanel : MonoBehaviour
    {
        public Transform root;



        private static readonly SystemLanguage[] languageOrders = { SystemLanguage.Korean, SystemLanguage.English };
        private List<LanguageButton> buttons = new List<LanguageButton>();

        void Start()
        {
            for (int i = 0; i < root.childCount; i++)
            {
                LanguageButton button = root.GetChild(i).GetComponent<LanguageButton>();
                button.parent = this;
                button.id = i;
                button.SetText(Locale.languageNameDic[languageOrders[i]]);
                buttons.Add(button);
            }
        }

        public void OnChildClicked(int id)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].isSelected = (i == id);
            }

            SystemLanguage targetLanguage = languageOrders[id];
            StartCoroutine(Locale.Load(targetLanguage));

            DataManager.Instance.saveData.language = targetLanguage;
            DataManager.Instance.Save();
        }


    }
}

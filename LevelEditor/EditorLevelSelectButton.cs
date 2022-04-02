using System;
using System.Collections;
using TMPro;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.LevelEditor
{
    public class EditorLevelSelectButton : MonoBehaviour
    {
        public EditorLevelSelectView view;

        public EditorToggleLevel ToggleLevel { get; private set; }

        private TextMeshProUGUI titleText;
        private TextMeshProUGUI sizeText;
        private TextMeshProUGUI verifiedText;
        private TextMeshProUGUI dateText;
        private UnityEngine.UI.Button viewButton;

        private DateTime editTime;

        // Use this for initialization
        void Awake()
        {
            titleText = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
            sizeText = transform.Find("SizeText").GetComponent<TextMeshProUGUI>();
            verifiedText = transform.Find("VerifiedText").GetComponent<TextMeshProUGUI>();
            dateText = transform.Find("DateText").GetComponent<TextMeshProUGUI>();
            viewButton = transform.Find("ViewButton").GetComponent<UnityEngine.UI.Button>();

            viewButton.onClick.AddListener(() => { view.Select(this); });
        }

        public void Apply(EditorToggleLevel toggleLevel, EditorLevelSelectView view)
        {
            this.view = view;
            ToggleLevel = toggleLevel;

            titleText.text = toggleLevel.title;
            sizeText.text = $"{toggleLevel.data.width}x{toggleLevel.data.height}";
            verifiedText.text = toggleLevel.isVerified ? Locale.Get("leveleditor.verified") : Locale.Get("leveleditor.notverified");
            dateText.text = "?";

            editTime = CommonUtils.TimeStampToDateTime(toggleLevel.editTimeStamp);
            StartCoroutine(UpdateTextTextRealtime());
        }

        private static readonly WaitForSeconds delayWait = new WaitForSeconds(15);

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(UpdateTextTextRealtime());
        }

        private IEnumerator UpdateTextTextRealtime()
        {
            while (true)
            {
                var now = DateTime.Now;
                var timeDiff = now - editTime;

                if (timeDiff.Days > 0)
                {
                    int monthSpan = now.MonthDifference(editTime);
                    if (monthSpan > 0)
                    {
                        int yearSpan = monthSpan / 12;
                        if (yearSpan > 0)
                        {
                            dateText.text = yearSpan + Locale.Get("timespan.years");
                        }
                        else
                        {
                            dateText.text = monthSpan + Locale.Get("timespan.months");
                        }
                    }
                    else
                    {
                        dateText.text = timeDiff.Days + Locale.Get("timespan.days");
                    }
                }
                else if (timeDiff.Hours > 0)
                {
                    dateText.text = timeDiff.Hours + Locale.Get("timespan.hours");
                }
                else if (timeDiff.Minutes > 0)
                {
                    dateText.text = timeDiff.Minutes + Locale.Get("timespan.minutes");
                }
                else
                {
                    dateText.text = Locale.Get("timespan.justbefore");
                }

                yield return delayWait;
            }
        }
    }
}
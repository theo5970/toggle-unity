using TMPro;
using UnityEngine;

namespace Toggle.LevelEditor
{
    public class EditorLevelInfoView : MonoBehaviour
    {
        public EditorLevelSelectView mainView;

        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelSizeText;
        public TextMeshProUGUI levelVerifiedText;

        private void Awake()
        {
            mainView.onLevelSelected += OnLevelSelected;
        }

        private void OnLevelSelected(EditorToggleLevel level)
        {
            levelNameText.text = level.title;
            levelSizeText.text = string.Format("{0}: {1} x {2}",
                Locale.Get("leveleditor.info.size"), level.data.width, level.data.height);

            if (level.minimumClickCount != int.MaxValue)
            {
                if (level.isVerified)
                {
                    levelVerifiedText.text = string.Format("{0} ({1}: {2})", Locale.Get("leveleditor.verified"), Locale.Get("leveleditor.minimumclickcount"), level.minimumClickCount);
                }
                else
                {
                    levelVerifiedText.text = Locale.Get("leveleditor.notverified");
                }
            }
            else
            {
                levelVerifiedText.text = (level.isVerified ? Locale.Get("leveleditor.verified") : Locale.Get("leveleditor.notverified"));
            }
        }
    }
}
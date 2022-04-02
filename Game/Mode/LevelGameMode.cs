using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using Toggle.Core;

namespace Toggle.Game.Mode
{
    public class LevelGameMode : GameMode
    {

        public static ToggleLevel levelToLoad;
        private LevelClearDialog clearDialog;

        public override void Prepare()
        {
            base.Prepare();
            map.LoadLevel(levelToLoad);

            gameManager.modeText.text = "???";

            clearDialog = gameManager.clearDialog;
            clearDialog.nextButton.onClick.AddListener(() => SceneManager.LoadScene("MainScene"));
        }

        protected override void OnClearMap()
        {
            clearDialog.Show(3, 
                Locale.Get("latteishorse"), 
                Locale.Get("challengemode.backtomenu"));

        }
    }
}
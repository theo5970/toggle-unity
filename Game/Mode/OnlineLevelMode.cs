using System.Collections;
using System.Collections.Generic;
using Toggle.Core;
using Toggle.Game.Common;
using Toggle.Game.Mode;
using UnityEngine;

public class OnlineLevelMode : GameMode
{
    public static string levelCode;

    public override void Prepare()
    {
        base.Prepare();

        map.LoadLevel(new ToggleLevel(ToggleLevelReader.FromBase64(levelCode)));

        gameManager.modeText.text = "온라인 레벨";
        gameManager.clearDialog.nextButton.onClick.AddListener(() =>
        {
            SceneStack.BackToPreviousScene();
        });
    }

    protected override void OnClearMap()
    {
        gameManager.clearDialog.Show(3, "축하합니다!\n레벨을 클리어하셨습니다!", "돌아가기");

    }
}

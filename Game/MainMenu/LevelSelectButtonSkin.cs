using Toggle.Game.Data;
using UnityEngine;

namespace Toggle.Game.MainMenu
{
    [CreateAssetMenu(fileName = "New Level Select Button Skin", menuName = "Toggle/Create Level Select Button Skin",
        order = 0)]
    public class LevelSelectButtonSkin : ScriptableObject
    {
        public Color colorNotCleared;
        public Color colorCleared;
        public Color colorPerfect;

        public Color GetColorByTryState(LevelTryState tryState, int starCount)
        {
            switch (tryState)
            {
                case LevelTryState.Clear:
                    if (starCount == 3)
                    {
                        return colorPerfect;
                    }
                    else
                    {
                        return colorCleared;
                    }
                default:
                    return colorNotCleared;
            }
        }
    }
}
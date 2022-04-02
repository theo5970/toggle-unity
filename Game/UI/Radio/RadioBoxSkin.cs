using UnityEngine;

namespace Toggle.Game.UI
{
    [CreateAssetMenu(fileName = "New RadioBox Skin", menuName = "Toggle/Create RadioBox Skin", order = 0)]
    public class RadioBoxSkin : ScriptableObject
    {
        public Color normalBackgroundColor;
        public Color normalForegroundColor;

        public Color highlightBackgroundColor;
        public Color highlightForegroundColor;
    }
}
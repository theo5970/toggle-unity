using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Toggle.Game.Common
{
    [CreateAssetMenu(fileName ="New Button Skin", menuName = "Create Button Skin", order = 0)]

    public class ButtonSkin : ScriptableObject
    {
        [Header("Normal Colors")]
        public Color normalFunctionColor;
        public Color normalDefaultColor;
        public Color normalHoverColor;

        [Header("Highlight Colors")]
        public Color highlightFunctionColor; 
        public Color highlightDefaultColor;
        public Color highlightHoverColor;

        [Header("Sprites")]
        public SpriteAtlas atlas;

        private Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public Sprite GetSprite(string spriteTag)
        {
            Sprite result;
            if (cache.TryGetValue(spriteTag, out result))
            {
                return result;
            }
            else
            {
                result = atlas.GetSprite(spriteTag);
                if (result != null)
                {
                    cache.Add(spriteTag, result);
                }
                return result;
            }
        }
    }
}
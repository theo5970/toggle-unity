using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class AroundEightDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(AroundEightFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);

            SetSpriteAndRotation(view, view.skin.GetSprite("EightArrow"), Quaternion.identity);
        }
    }
}
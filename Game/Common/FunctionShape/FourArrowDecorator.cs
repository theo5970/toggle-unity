using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class FourArrowDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(FourArrowFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);

            SetSpriteAndRotation(view, view.skin.GetSprite("FourArrow"), Quaternion.identity);
        }
    }
}
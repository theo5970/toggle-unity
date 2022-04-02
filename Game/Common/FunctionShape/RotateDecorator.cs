using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class RotateDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(RotateFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);

            bool isClockwise = (view.button.functionSubType == FunctionSubType.RC);
            var funcShape = view.functionShape;
            funcShape.flipX = !isClockwise;

            SetSpriteAndRotation(view, view.skin.GetSprite("ClockwiseRotate"), Quaternion.identity);
        }
    }
}
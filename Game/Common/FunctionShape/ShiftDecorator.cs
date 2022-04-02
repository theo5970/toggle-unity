using System;
using UnityEngine;
using Toggle.Core.Function;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class ShiftDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(ShiftFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);
            
            var funcShape = view.functionShape;
            funcShape.flipX = view.button.functionSubType != FunctionSubType.SHR;
            funcShape.flipY = false;

            SetSpriteAndRotation(view, view.skin.GetSprite("Shift"), Quaternion.identity);
        }
    }
}
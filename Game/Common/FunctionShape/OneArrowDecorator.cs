using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class OneArrowDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(OneArrowFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            float angle = 0;
            switch (view.button.functionSubType)
            {
                case FunctionSubType.L:
                    angle = 180;
                    break;
                case FunctionSubType.R:
                    angle = 0;
                    break;
                case FunctionSubType.U:
                    angle = 90;
                    break;
                case FunctionSubType.D:
                    angle = 270;
                    break;
                case FunctionSubType.DLU:
                    angle = 135;
                    break;
                case FunctionSubType.DRU:
                    angle = 45;
                    break;
                case FunctionSubType.DLD:
                    angle = 225;
                    break;
                case FunctionSubType.DRD:
                    angle = 315;
                    break;
            }

            SetSpriteAndRotation(view, view.skin.GetSprite("OneArrow"), Quaternion.Euler(0, 0, angle));
        }
    }
}
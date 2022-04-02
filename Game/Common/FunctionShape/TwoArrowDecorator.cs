using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class TwoArrowDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(TwoArrowFunction);
        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);
            
            float angle = 0;
            switch (view.button.functionSubType)
            {
                case FunctionSubType.BH:
                    angle = 0;
                    break;
                case FunctionSubType.BV:
                    angle = 90;
                    break;
                case FunctionSubType.DBLDRU:
                    angle = 45;
                    break;
                case FunctionSubType.DBLURD:
                    angle = -45;
                    break;
            }

            SetSpriteAndRotation(view, view.skin.GetSprite("TwoArrow"), Quaternion.Euler(0, 0, angle));
        }
    }
}
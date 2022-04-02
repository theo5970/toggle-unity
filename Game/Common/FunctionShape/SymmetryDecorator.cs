using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class SymmetryDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(SymmetryFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);
            
            float angle = 0;
            switch (view.button.functionSubType)
            {
                case FunctionSubType.SYH:
                    angle = 0;
                    break;
                case FunctionSubType.SYV:
                    angle = 90;
                    break;
            }

            SetSpriteAndRotation(view, view.skin.GetSprite("Symmetry"), Quaternion.Euler(0, 0, angle));
        }
    }
}
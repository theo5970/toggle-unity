using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class NothingDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(NothingFunction);

        protected override bool resetFlip => false;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);

            SetSpriteAndRotation(view, null, Quaternion.identity);
        }
    }
}
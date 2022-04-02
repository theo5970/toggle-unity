using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public class LatteIsHorseDecorator : FunctionShapeDecorator
    {
        public override Type functionType => typeof(LatteIsHorseFunction);

        protected override bool resetFlip => true;

        public override void UpdateOutside(BaseButtonView view)
        {
            base.UpdateOutside(view);

            Sprite sprite = null;
            switch (view.button.functionSubType)
            {
                case FunctionSubType.KOA:
                    sprite = view.skin.GetSprite("A");
                    break;
                case FunctionSubType.KOB:
                    sprite = view.skin.GetSprite("B");
                    break;
            }

            SetSpriteAndRotation(view, sprite, Quaternion.identity);
        }
    }
}

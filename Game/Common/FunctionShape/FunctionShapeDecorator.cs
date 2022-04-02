using System;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Game.Common.FunctionShape
{
    [Preserve]
    public abstract class FunctionShapeDecorator
    {
        public abstract Type functionType { get; }

        protected abstract bool resetFlip { get; }

        public virtual void UpdateOutside(BaseButtonView view)
        {
            if (resetFlip)
            {
                view.functionShape.flipX = false;
                view.functionShape.flipY = false;
            }
        }
        
        public void SetSpriteAndRotation(BaseButtonView view, Sprite sprite, Quaternion rotation)
        {
            view.StartCoroutine(view.StartFuncShapeAnimation(sprite, rotation));
        }
    }
}
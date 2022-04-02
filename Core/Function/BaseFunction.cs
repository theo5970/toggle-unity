using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Utils;

namespace Toggle.Core.Function
{
    [Preserve]
    public abstract class BaseFunction
    {
        public abstract FunctionSubType[] GetSupportedTypes();

        /// <summary>
        /// 클릭 가능여부
        /// </summary>
        public virtual bool IsClickable { get; } = true;


        /// <summary>
        /// 버튼을 클릭했을 때 호출
        /// </summary>
        /// <param name="button">버튼</param>
        protected abstract void OnClickedInternal(BaseButton button);

        private object lockObject = new object();

        public void OnClicked(BaseButton button)
        {
            lock (lockObject)
            {
                OnClickedInternal(button);
            }
        }
    }
}
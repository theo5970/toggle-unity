using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 아무 역할도 없는 버튼
    /// </summary>
    [Preserve]
    public class NothingFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes = {FunctionSubType.NOP};
        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        protected override void OnClickedInternal(BaseButton button)
        {
            
        }
    }
}
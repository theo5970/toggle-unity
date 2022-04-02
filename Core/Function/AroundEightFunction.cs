using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    [Preserve]
    public class AroundEightFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes = {FunctionSubType.AROUNDEIGHTSPACE};
        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        protected override void OnClickedInternal(BaseButton button)
        {
            ButtonGrid grid = button.grid;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int coord = button.coordinate + new Vector2Int(dx, dy);
                    if (grid.CheckRange(coord)) grid[coord].Toggle();
                }
            }
        }
    }
}
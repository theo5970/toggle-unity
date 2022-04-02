using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 쉬프트 버튼 (왼쪽, 오른쪽 방향)
    /// </summary>
    [Preserve]
    public class ShiftFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes =
            {FunctionSubType.SHL, FunctionSubType.SHR, FunctionSubType.SHU, FunctionSubType.SHD};

        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        private readonly List<bool> oldStates = new List<bool>();
        private readonly List<bool> newStates = new List<bool>();

        protected override void OnClickedInternal(BaseButton button)
        {
            ButtonGrid grid = button.grid;
            oldStates.Clear();
            newStates.Clear();

            for (int i = 0; i < grid.width; i++)
            {
                oldStates.Add(false);
                newStates.Add(false);
            }

            for (int i = 0; i < grid.width; i++)
            {
                Vector2Int otherCoord = new Vector2Int(i, button.coordinate.y);

                if (grid.TryGetAt(otherCoord, out BaseButton otherButton))
                {
                    oldStates[i] = otherButton.isOn;
                }
            }

            if (button.functionSubType == FunctionSubType.SHR)
            {
                newStates[0] = oldStates[grid.width - 1];
                for (int i = 1; i < grid.width; i++)
                {
                    newStates[i] = oldStates[i - 1];
                }
            }
            else
            {
                newStates[grid.width - 1] = oldStates[0];
                for (int i = 0; i < grid.width - 1; i++)
                {
                    newStates[i] = oldStates[i + 1];
                }
            }

            for (int i = 0; i < grid.width; i++)
            {
                Vector2Int otherCoord = new Vector2Int(i, button.coordinate.y);
                if (grid.TryGetAt(otherCoord, out BaseButton otherButton))
                {
                    otherButton.isOn = newStates[i];

                    int affectOrder = i;
                    if (button.functionSubType == FunctionSubType.SHL) affectOrder = grid.width - 1 - i;
                }
            }
        }

    }
}
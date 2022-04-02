using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 회전 버튼 (시계 방향, 반시계 방향)
    /// </summary>
    [Preserve]
    public class RotateFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes = { FunctionSubType.RC, FunctionSubType.RCC };
        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        private static readonly Vector2Int[] directions =
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1)
        };


        private readonly bool[] cachedOldStates = new bool[8];
        private readonly bool[] cachedNewStates = new bool[8];

        protected override void OnClickedInternal(BaseButton button)
        {
            ButtonGrid grid = button.grid;

            FunctionSubType subType = button.functionSubType;
            button.Toggle();

            // 주변 8칸의 상태 저장하기
            for (int i = 0; i < 8; i++)
            {
                Vector2Int otherCoord = button.coordinate + directions[i];

                if (grid.TryGetAt(otherCoord, out BaseButton otherButton))
                {
                    cachedOldStates[i] = otherButton.isOn;
                }
                else
                {
                    cachedOldStates[i] = false;
                }
            }

            if (subType == FunctionSubType.RC) // 시계 방향으로 배열 회전
            {
                cachedNewStates[0] = cachedOldStates[7];
                for (int i = 1; i < 8; i++)
                {
                    cachedNewStates[i] = cachedOldStates[i - 1];
                }
            }
            else if (subType == FunctionSubType.RCC) // 시계 반대방향으로 배열 회전
            {
                cachedNewStates[7] = cachedOldStates[0];
                for (int i = 0; i < 7; i++)
                {
                    cachedNewStates[i] = cachedOldStates[i + 1];
                }
            }

            // 회전된 배열을 맵에 적용
            for (int i = 0; i < 8; i++)
            {
                Vector2Int otherCoord = button.coordinate + directions[i];
                if (grid.TryGetAt(otherCoord, out BaseButton otherButton))
                {
                    bool newState = cachedNewStates[i];
                    if (otherButton.isOn != newState)
                    {
                        otherButton.Toggle();
                    }
                }
            }
        }


    }
}
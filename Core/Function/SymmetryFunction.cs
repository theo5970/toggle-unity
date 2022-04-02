using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 대칭 버튼 (가로 대칭, 세로 대칭)
    /// </summary>
    [Preserve]
    public class SymmetryFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes = { FunctionSubType.SYH, FunctionSubType.SYV };
        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        private bool[,] tempStates = new bool[2, 16];

        private BaseButton button;
        private ButtonGrid grid;

        protected override void OnClickedInternal(BaseButton button)
        {
            this.button = button;
            this.grid = button.grid;
            button.Toggle();

            switch (button.functionSubType)
            {
                case FunctionSubType.SYH:
                    DoHorizontal();
                    break;

                case FunctionSubType.SYV:
                    DoVertical();
                    break;
            }
        }

        /// <summary>
        /// 가로 방향 대칭토글
        /// </summary>
        private void DoHorizontal()
        {
            int buttonX = button.coordinate.x;
            int x1 = buttonX - 1;
            int x2 = buttonX + 1;

            // 1차 : 양 옆의 버튼 상태 백업하기
            if (grid.CheckRange(x1, 0))
            {
                for (int y = 0; y < grid.height; y++)
                {
                    tempStates[0, y] = grid[x1, y].isOn;
                }
            }
            else
            {
                for (int i = 0; i < 16; i++) tempStates[0, i] = false;
            }

            if (grid.CheckRange(x2, 0))
            {
                for (int y = 0; y < grid.height; y++)
                {
                    tempStates[1, y] = grid[x2, y].isOn;
                }
            }
            else
            {
                for (int i = 0; i < 16; i++) tempStates[1, i] = false;
            }

            // 2차 : 적용하기
            if (grid.CheckRange(x1, 0))
            {
                // x2 열 => x1 열 교체
                for (int y = 0; y < grid.height; y++)
                {
                    grid[x1, y].isOn = tempStates[1, y];
                }
            }

            if (grid.CheckRange(x2, 0))
            {
                // x1 열 => x2 열 교체
                for (int y = 0; y < grid.height; y++)
                {
                    grid[x2, y].isOn = tempStates[0, y];
                }
            }
        }

        /// <summary>
        /// 세로 방향 대칭토글
        /// </summary>
        private void DoVertical()
        {
            int buttonY = button.coordinate.y;
            int y1 = buttonY - 1;
            int y2 = buttonY + 1;

            // 1차 : 위, 아래의 버튼 상태 백업하기
            if (grid.CheckRange(0, y1))
            {
                for (int x = 0; x < grid.width; x++)
                {
                    tempStates[0, x] = grid[x, y1].isOn;
                }
            }
            else
            {
                for (int i = 0; i < 16; i++) tempStates[0, i] = false;
            }

            if (grid.CheckRange(0, y2))
            {
                for (int x = 0; x < grid.width; x++)
                {
                    tempStates[1, x] = grid[x, y2].isOn;
                }
            }
            else
            {
                for (int i = 0; i < 16; i++) tempStates[1, i] = false;
            }

            // 2차 : 적용하기
            if (grid.CheckRange(0, y1))
            {
                // y2 열 => y1 열 교체
                for (int x = 0; x < grid.width; x++)
                {
                    grid[x, y1].isOn = tempStates[1, x];
                }
            }

            if (grid.CheckRange(0, y2))
            {
                // x1 열 => x2 열 교체
                for (int x = 0; x < grid.width; x++)
                {
                    grid[x, y2].isOn = tempStates[0, x];
                }
            }
        }
    }
}
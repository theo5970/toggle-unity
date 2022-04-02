using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 한 방향 화살표 버튼 (직선, 대각선)
    /// </summary>
    [Preserve]
    public class OneArrowFunction : BaseFunction
    {
        private BaseButton button;
        private ButtonGrid grid;

        private readonly FunctionSubType[] supportedTypes =
        {
            FunctionSubType.L, FunctionSubType.D, FunctionSubType.R, FunctionSubType.U, FunctionSubType.DLU,
            FunctionSubType.DLD, FunctionSubType.DRU, FunctionSubType.DRD
        };

        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        protected override void OnClickedInternal(BaseButton button)
        {
            this.button = button;
            this.grid = button.grid;

            switch (button.functionSubType)
            {
                case FunctionSubType.L:
                    DoLeft();
                    break;
                case FunctionSubType.R:
                    DoRight();
                    break;
                case FunctionSubType.U:
                    DoUp();
                    break;
                case FunctionSubType.D:
                    DoDown();
                    break;
                case FunctionSubType.DLU: // Left-Up
                    DoDiagonal(-1, 1);
                    break;
                case FunctionSubType.DRU: // Right-Up
                    DoDiagonal(1, 1);
                    break;
                case FunctionSubType.DLD: // Left-Down
                    DoDiagonal(-1, -1);
                    break;
                case FunctionSubType.DRD: // Right-Down
                    DoDiagonal(1, -1);
                    break;
            }

        }


        /// <summary>
        /// 왼쪽 방향 토글
        /// </summary>
        private void DoLeft()
        {
            int y = button.coordinate.y;
            for (int x = button.coordinate.x; x >= 0; x--)
            {
                if (grid.TryGetAt(x, y, out BaseButton btn))
                {
                    btn.Toggle();
                }
            }
        }

        /// <summary>
        /// 오른쪽 방향 토글
        /// </summary>
        private void DoRight()
        {
            int y = button.coordinate.y;
            for (int x = button.coordinate.x; x < grid.width; x++)
            {
                if (grid.TryGetAt(x, y, out BaseButton btn))
                {
                    btn.Toggle();
                }
            }
        }

        /// <summary>
        /// 위쪽 방향 토글
        /// </summary>
        private void DoUp()
        {
            int x = button.coordinate.x;
            for (int y = button.coordinate.y; y < grid.height; y++)
            {
                if (grid.TryGetAt(x, y, out BaseButton btn))
                {
                    btn.Toggle();
                }
            }
        }

        /// <summary>
        /// 아래쪽 방향 토글
        /// </summary>
        private void DoDown()
        {
            int x = button.coordinate.x;
            for (int y = button.coordinate.y; y >= 0; y--)
            {
                if (grid.TryGetAt(x, y, out BaseButton btn))
                {
                    btn.Toggle();
                }
            }
        }

        /// <summary>
        /// 대각선 방향 토글
        /// </summary>
        /// <param name="dx">대각선 X 증가량</param>
        /// <param name="dy">대각선 Y 증가량</param>
        private void DoDiagonal(int dx, int dy)
        {
            int k = 0;
            while (true)
            {
                Vector2Int coord = button.coordinate + new Vector2Int(k * dx, k * dy);
                if (grid.TryGetAt(coord, out BaseButton btn))
                {
                    btn.Toggle();
                    k++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
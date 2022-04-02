using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 두 방향 화살표 버튼 (직선, 대각선)
    /// </summary>
    [Preserve]
    public class TwoArrowFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes =
            {FunctionSubType.BH, FunctionSubType.BV, FunctionSubType.DBLDRU, FunctionSubType.DBLURD};

        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;

        private BaseButton button;
        private ButtonGrid grid;

        protected override void OnClickedInternal(BaseButton button)
        {
            this.button = button;
            this.grid = button.grid;

            switch (button.functionSubType)
            {
                case FunctionSubType.BH:
                    DoBothHorizontal();
                    break;
                case FunctionSubType.BV:
                    DoBothVertical();
                    break;
                case FunctionSubType.DBLDRU:
                    DoDiagonalBoth(true);
                    break;
                case FunctionSubType.DBLURD:
                    DoDiagonalBoth(false);
                    break;
            }
        }

        /// <summary>
        /// 가로 양방향 토글
        /// </summary>
        private void DoBothHorizontal()
        {
            int y = button.coordinate.y;
            for (int x = 0; x < grid.width; x++)
            {
                grid[x, y].Toggle();
            }
        }

        /// <summary>
        /// 세로 양방향 토글
        /// </summary>
        private void DoBothVertical()
        {
            int x = button.coordinate.x;
            for (int y = 0; y < grid.height; y++)
            {
                grid[x, y].Toggle();
            }
        }

        /// <summary>
        /// 대각선 양방향 토글
        /// </summary>
        /// <param name="isPositiveDir">true이면 LDRU (LeftDown-RightUp)</param>
        private void DoDiagonalBoth(bool isPositiveDir)
        {
            int k = 1;
            Vector2Int direction = isPositiveDir ? new Vector2Int(1, 1) : new Vector2Int(1, -1);

            while (true)
            {
                Vector2Int coord = button.coordinate + (direction * k);

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

            direction *= -1;
            k = 1;

            while (true)
            {
                Vector2Int coord = button.coordinate + (direction * k);

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

            button.Toggle();
        }
    }
}
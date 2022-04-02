using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Core.Function
{
    /// <summary>
    /// 네 방향 화살표 버튼 (동서남북)
    /// </summary>
    [Preserve]
    public class FourArrowFunction : BaseFunction
    {
        private readonly FunctionSubType[] supportedTypes = {FunctionSubType.BHV};
        public override FunctionSubType[] GetSupportedTypes() => supportedTypes;
        
        private BaseButton button;
        private ButtonGrid grid;

        protected override void OnClickedInternal(BaseButton button)
        {
            if (button.functionSubType == FunctionSubType.BHV)
            {
                this.button = button;
                this.grid = button.grid;

                DoBothHorizontal();
                DoBothVertical();
                button.Toggle();
            }
        }

        /// <summary>
        /// 가로 방향 토글
        /// </summary>
        public void DoBothHorizontal()
        {
            int y = button.coordinate.y;
            for (int x = 0; x < grid.width; x++)
            {
                if (x == button.coordinate.x) continue;
            
                int dx = Mathf.Abs(x - button.coordinate.x) + 1;
                grid[x, y].Toggle();
            }
        }

        /// <summary>
        /// 세로 방향 토글
        /// </summary>
        public void DoBothVertical()
        {
            int x = button.coordinate.x;
            for (int y = 0; y < grid.height; y++)
            {
                if (y == button.coordinate.y) continue;
            
                int dy = Mathf.Abs(y - button.coordinate.y) + 1;
                grid[x, y].Toggle();
            }
        }

        

    }
}
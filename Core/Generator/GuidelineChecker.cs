using Toggle.Core.Function;
using UnityEngine;

namespace Toggle.Core.Generator
{
    public partial class LevelGenerator
    {
        /// <summary>
        /// 주어진 좌표에 서브타입을 배치할 수 있는지 확인
        /// </summary>
        /// <param name="coord">배치 좌표값</param>
        /// <param name="subType">배치할 서브타입</param>
        /// <returns>배치 가능여부</returns>
        private bool CanPlace(Vector2Int coord, FunctionSubType subType)
        {
            int x = coord.x;
            int y = coord.y;

            FunctionSubType oldType = grid[x, y].functionSubType;
            grid[x, y].functionSubType = subType;

            bool result = true;

            switch (subType)
            {
                case FunctionSubType.U:
                    result = (y != grid.height - 1);
                    break;
                case FunctionSubType.D:
                    result = (y != 0);
                    break;
                case FunctionSubType.L:
                    result = (x != 0);
                    break;
                case FunctionSubType.R:
                    result = (x != grid.width - 1);
                    break;
                case FunctionSubType.DLU:
                    result = (x != 0 && y != grid.height - 1);
                    break;
                case FunctionSubType.DRU:
                    result = (x != grid.width - 1 && y != grid.height - 1);
                    break;
                case FunctionSubType.DLD:
                    result = (x != 0 && y != 0);
                    break;
                case FunctionSubType.DRD:
                    result = (x != grid.width - 1 && y != 0);
                    break;
                case FunctionSubType.DBLDRU:
                    result = (x == grid.width - 1 && y == 0) == false;
                    result &= (x == 0 && y == grid.height - 1) == false;
                    break;
                case FunctionSubType.DBLURD:
                    result = (x == 0 && y == 0) == false;
                    result &= (x == grid.width - 1 && y == grid.height - 1) == false;
                    break;
                case FunctionSubType.RC:
                case FunctionSubType.RCC:
                case FunctionSubType.SYH:
                case FunctionSubType.SYV:
                case FunctionSubType.AROUNDEIGHTSPACE:
                    bool xCond = (x >= 1 && x <= grid.width - 2);
                    bool yCond = (y >= 1 && y <= grid.height - 2);
                    result = xCond && yCond;
                    break;
            }

            if (result)
            {
                result &= CheckRule(x, y, subType);
            }

            if (result && options.mirrorType != MirrorType.None)
            {
                Vector2Int flippedCoord = GetFlippedCoordinate(x, y);
                FunctionSubType mirrorTempType = grid[flippedCoord].functionSubType;
                FunctionSubType flippedSubType = subType;
                switch (options.mirrorType)
                {
                    case MirrorType.Horizontal:
                        flippedSubType = FlipHorizontal(subType);
                        break;
                    case MirrorType.Vertical:
                        flippedSubType = FlipVertical(subType);
                        break;
                    case MirrorType.Diagonal:
                        flippedSubType = FlipHorizontal(FlipVertical(subType));
                        break;
                }

                grid[flippedCoord].functionSubType = flippedSubType;


                result &= CheckRule(flippedCoord.x, flippedCoord.y, flippedSubType);
                grid[flippedCoord].functionSubType = mirrorTempType;
            }

            if (!result)
            {
                grid[x, y].functionSubType = oldType;
            }

            return result;
        }

        /// <summary>
        /// 주어진 좌표에 서브타입을 배치할 때 가이드라인을 준수하는지 확인
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="subType">서브타입</param>
        /// <returns>가이드라인 준수여부</returns>
        private bool CheckRule(int x, int y, FunctionSubType subType)
        {
            bool result = true;
            switch (subType)
            {
                case FunctionSubType.L:
                case FunctionSubType.R:
                case FunctionSubType.BH:
                    result = CheckRuleAtRow(y);
                    break;
                case FunctionSubType.U:
                case FunctionSubType.D:
                case FunctionSubType.BV:
                    result = CheckRuleAtColumn(x);
                    break;
                case FunctionSubType.DLD:
                case FunctionSubType.DRU:
                case FunctionSubType.DBLDRU:
                    result = CheckRuleAtLDRU(x, y);
                    break;
                case FunctionSubType.DLU:
                case FunctionSubType.DRD:
                case FunctionSubType.DBLURD:
                    result = CheckRuleAtLURD(x, y);
                    break;
                case FunctionSubType.BHV:
                    result = CheckRuleFourArrow(x, y);
                    break;
            }

            return result;
        }

        /// <summary>
        /// 특정 행의 가이드라인 준수 확인
        /// </summary>
        /// <param name="y">행 좌표</param>
        /// <returns>가이드라인 준수여부</returns>
        private bool CheckRuleAtRow(int y)
        {
            bool isFirstTypeRight = grid[0, y].functionSubType == FunctionSubType.R;
            bool isLastTypeLeft = grid[grid.width - 1, y].functionSubType == FunctionSubType.L;
            if (isFirstTypeRight && isLastTypeLeft)
            {
                return false;
            }

            bool result = true;
            int twoArrowLinearCount = 0;
            for (int x = 0; x < grid.width; x++)
            {
                BaseButton otherButton = grid[x, y];
                if (otherButton.functionSubType == FunctionSubType.BH)
                {
                    twoArrowLinearCount++;
                    if (twoArrowLinearCount >= 2)
                    {
                        result = false;
                        break;
                    }
                }
            }

            /*
             * example)
             * . L R . R
             * L . . L R
             * 와 같은 케이스들을 탐지
             */

            if (isFirstTypeRight || isLastTypeLeft || twoArrowLinearCount > 0)
            {
                for (int x = 1; x < grid.width; x++)
                {
                    BaseButton leftButton = grid[x - 1, y];
                    BaseButton rightButton = grid[x, y];

                    if (leftButton.functionSubType == FunctionSubType.L &&
                        rightButton.functionSubType == FunctionSubType.R)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Rule LR violation detected.");
#endif
                        result = false;
                        break;
                    }
                }
            }

            if (twoArrowLinearCount > 0 && (isFirstTypeRight || isLastTypeLeft))
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 특정 열의 가이드라인 준수 확인
        /// </summary>
        /// <param name="x">열 좌표</param>
        /// <returns>가이드라인 준수여부</returns>
        private bool CheckRuleAtColumn(int x)
        {
            bool isFirstTypeUp = grid[x, 0].functionSubType == FunctionSubType.U;
            bool isLastTypeDown = grid[x, grid.height - 1].functionSubType == FunctionSubType.D;

            if (isFirstTypeUp && isLastTypeDown)
            {
                return false;
            }

            bool result = true;
            int twoArrowLinearCount = 0;
            for (int y = 0; y < grid.height; y++)
            {
                BaseButton otherButton = grid[x, y];
                if (otherButton.functionSubType == FunctionSubType.BV)
                {
                    twoArrowLinearCount++;
                    if (twoArrowLinearCount >= 2)
                    {
                        result = false;
                        break;
                    }
                }
            }

            /*
             * example1) 
             * . . D . .
             * . . . . .
             * . . U . .
             * . . D . .
             * . . . . .
             *
             * example2)
             * . . . . .
             * . . . . .
             * . . U . .
             * . . D . .
             * . . U . .
             *
             * 와 같은 케이스들을 탐지
             */
            if (isFirstTypeUp || isLastTypeDown || twoArrowLinearCount > 0)
            {
                for (int y = 1; y < grid.height; y++)
                {
                    BaseButton upButton = grid[x, y - 1];
                    BaseButton downButton = grid[x, y];

                    if (upButton.functionSubType == FunctionSubType.U &&
                        downButton.functionSubType == FunctionSubType.D)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Rule UD violation detected.");
#endif
                        result = false;
                        break;
                    }
                }
            }

            if (twoArrowLinearCount > 0 && (isFirstTypeUp || isLastTypeDown))
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// LeftDown-RightUp 방향 대각선 가이드라인 준수 확인
        /// </summary>
        /// <param name="x">X 자표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>가이드라인 준수여부</returns>
        private bool CheckRuleAtLDRU(int x, int y)
        {
            Vector2Int firstCoord = new Vector2Int(x, y);
            Vector2Int lastCoord = firstCoord;

            Vector2Int diagonalDir = Vector2Int.one;

            while (grid.CheckRange(firstCoord - diagonalDir))
            {
                firstCoord -= diagonalDir;
            }

            while (grid.CheckRange(lastCoord + diagonalDir))
            {
                lastCoord += diagonalDir;
            }

            int count = lastCoord.x - firstCoord.x + 1;

            bool isFirstTypeRightUp = grid[firstCoord].functionSubType == FunctionSubType.DRU;
            bool isLastTypeLeftDown = grid[lastCoord].functionSubType == FunctionSubType.DLD;

            if (isFirstTypeRightUp && isLastTypeLeftDown)
            {
                return false;
            }

            int ldruCount = 0;
            bool result = true;
            Vector2Int coord = firstCoord;

            for (int i = 0; i < count; i++)
            {
                if (grid[coord].functionSubType == FunctionSubType.DBLDRU)
                {
                    ldruCount++;
                    if (ldruCount >= 2)
                    {
                        result = false;
                        break;
                    }
                }

                coord += diagonalDir;
            }

            /*
             * . .  . . .
             * . .  . . .
             * . .  RU. .
             * . LD . . .
             * RU.  . . .
             * 와 같은 케이스들을 탐지
             */

            if (isFirstTypeRightUp || isLastTypeLeftDown || ldruCount > 0)
            {
                for (int i = 1; i < count; i++)
                {
                    BaseButton beforeButton = grid[firstCoord + (i - 1) * diagonalDir];
                    BaseButton currentButton = grid[firstCoord + i * diagonalDir];

                    if (beforeButton.functionSubType == FunctionSubType.DLD &&
                        currentButton.functionSubType == FunctionSubType.DRU)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Rule LD-RU violation detected.");
#endif
                        result = false;
                    }
                }
            }

            if (ldruCount > 0 && (isFirstTypeRightUp || isLastTypeLeftDown))
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// LeftUp-RightDown 방향 대각선 가이드라인 준수 확인
        /// </summary>
        /// <param name="x">X 자표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>가이드라인 준수여부</returns>
        private bool CheckRuleAtLURD(int x, int y)
        {
            Vector2Int firstCoord = new Vector2Int(x, y);
            Vector2Int lastCoord = new Vector2Int(x, y);

            Vector2Int diagonalDir = new Vector2Int(1, -1);
            while (grid.CheckRange(firstCoord - diagonalDir))
            {
                firstCoord -= diagonalDir;
            }

            while (grid.CheckRange(lastCoord + diagonalDir))
            {
                lastCoord += diagonalDir;
            }

            int count = lastCoord.x - firstCoord.x + 1;

            bool isFirstTypeRightDown = grid[firstCoord].functionSubType == FunctionSubType.DRD;
            bool isLastTypeLeftUp = grid[lastCoord].functionSubType == FunctionSubType.DLU;

            if (isFirstTypeRightDown && isLastTypeLeftUp)
            {
                return false;
            }

            int lurdCount = 0;
            bool result = true;
            Vector2Int coord = firstCoord;

            for (int i = 0; i < count; i++)
            {
                if (grid[coord].functionSubType == FunctionSubType.DBLURD)
                {
                    lurdCount++;
                    if (lurdCount >= 2)
                    {
                        result = false;
                        break;
                    }
                }

                coord += diagonalDir;
            }

            /*
            * RD. . . .
            * . . . . .
            * . . LU. .
            * . . . RD.
            * . . . . .
            * 와 같은 케이스들을 탐지
            */

            if (isFirstTypeRightDown || isLastTypeLeftUp || lurdCount > 0)
            {
                for (int i = 1; i < count; i++)
                {
                    BaseButton beforeButton = grid[firstCoord + (i - 1) * diagonalDir];
                    BaseButton currentButton = grid[firstCoord + i * diagonalDir];

                    if (beforeButton.functionSubType == FunctionSubType.DLD &&
                        currentButton.functionSubType == FunctionSubType.DRU)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Rule LD-RU violation detected.");
#endif
                        result = false;
                    }
                }
            }
            if (lurdCount > 0 && (isFirstTypeRightDown || isLastTypeLeftUp))
            {
                result = false;
            }

            return result;
        }


        /// <summary>
        /// 동서남북 방향 가이드라인 준수 확인
        /// </summary>
        /// <param name="x">X 자표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>가이드라인 준수여부</returns>
        private bool CheckRuleFourArrow(int x, int y)
        {
            bool hasTwoLinearArrowOnRow = false;
            bool hasTwoLinearArrowOnColumn = false;

            for (int x2 = 0; x2 < grid.width; x2++)
            {
                if (x2 == x) continue;

                if (grid[x2, y].functionSubType == FunctionSubType.BH)
                {
                    hasTwoLinearArrowOnRow = true;
                    break;
                }
            }

            for (int y2 = 0; y2 < grid.height; y2++)
            {
                if (y2 == y) continue;

                if (grid[x, y2].functionSubType == FunctionSubType.BV)
                {
                    hasTwoLinearArrowOnColumn = true;
                    break;
                }
            }

            // 두 플래그 모두 참이거나 모두 거짓이어야 하는데 그렇지 않은 경우 그냥 무시
            if (hasTwoLinearArrowOnRow ^ hasTwoLinearArrowOnColumn == false)
            {
                return true;
            }

            bool result;
            if (hasTwoLinearArrowOnRow)
            {
                // 좌우 화살표(BH)가 행에 존재할 때
                bool existsUpAndDown;
                if (y == 0)
                {
                    existsUpAndDown = (grid[x, y + 1].functionSubType == FunctionSubType.U);
                }
                else if (y == grid.height - 1)
                {
                    existsUpAndDown = (grid[x, y - 1].functionSubType == FunctionSubType.D);
                }
                else
                {
                    existsUpAndDown = (grid[x, y + 1].functionSubType == FunctionSubType.U) && (grid[x, y - 1].functionSubType == FunctionSubType.D);
                }

                result = !existsUpAndDown;
            }
            else
            {
                // 상하 화살표(BV)가 열에 존재할 때
                bool existsLeftAndRight;
                if (x == 0)
                {
                    existsLeftAndRight = (grid[x + 1, y].functionSubType == FunctionSubType.R);
                }
                else if (x == grid.width - 1)
                {
                    existsLeftAndRight = (grid[x - 1, y].functionSubType == FunctionSubType.L);
                }
                else
                {
                    existsLeftAndRight = (grid[x + 1, y].functionSubType == FunctionSubType.R) && (grid[x - 1, y].functionSubType == FunctionSubType.L);
                }

                result = !existsLeftAndRight;
            }

            if (!result)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Rule FourArrow violation detected.");
#endif
            }
            return result;
        }
    }

}
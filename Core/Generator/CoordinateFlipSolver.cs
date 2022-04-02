using Toggle.Core.Function;
using UnityEngine;

namespace Toggle.Core.Generator
{
    public partial class LevelGenerator
    {
        /// <summary>
        /// 주어진 서브타입을 가로 대칭연산해서 반환
        /// </summary>
        /// <param name="subType">서브타입</param>
        /// <returns>대칭된 서브타입</returns>
        private FunctionSubType FlipHorizontal(FunctionSubType subType)
        {
            switch (subType)
            {
                case FunctionSubType.L: return FunctionSubType.R;
                case FunctionSubType.R: return FunctionSubType.L;

                case FunctionSubType.DLD: return FunctionSubType.DRD;
                case FunctionSubType.DRD: return FunctionSubType.DLD;
                case FunctionSubType.DLU: return FunctionSubType.DRU;
                case FunctionSubType.DRU: return FunctionSubType.DLU;

                case FunctionSubType.DBLDRU: return FunctionSubType.DBLURD;
                case FunctionSubType.DBLURD: return FunctionSubType.DBLDRU;

                case FunctionSubType.RC: return FunctionSubType.RCC;
                case FunctionSubType.RCC: return FunctionSubType.RC;
            }

            return subType;
        }

        /// <summary>
        /// 주어진 서브타입을 세로 대칭연산해서 반환
        /// </summary>
        /// <param name="subType">서브타입</param>
        /// <returns>대칭된 서브타입</returns>
        private FunctionSubType FlipVertical(FunctionSubType subType)
        {
            switch (subType)
            {
                case FunctionSubType.U: return FunctionSubType.D;
                case FunctionSubType.D: return FunctionSubType.U;

                case FunctionSubType.DLD: return FunctionSubType.DLU;
                case FunctionSubType.DRD: return FunctionSubType.DRU;
                case FunctionSubType.DLU: return FunctionSubType.DLD;
                case FunctionSubType.DRU: return FunctionSubType.DRD;

                case FunctionSubType.DBLDRU: return FunctionSubType.DBLURD;
                case FunctionSubType.DBLURD: return FunctionSubType.DBLDRU;

                case FunctionSubType.RC: return FunctionSubType.RCC;
                case FunctionSubType.RCC: return FunctionSubType.RC;
            }

            return subType;
        }

        /// <summary>
        /// 주어진 좌표가 대칭 기준을 벗어나는 지 확인
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>결과 여부</returns>
        private bool CheckIfMirrorOut(int x, int y)
        {
            switch (options.mirrorType)
            {
                case MirrorType.Horizontal:
                    if (x >= grid.width / 2) return true;
                    break;
                case MirrorType.Vertical:
                    if (y >= grid.height / 2) return true;
                    break;
                case MirrorType.Diagonal:
                    if (x >= y) return true;
                    break;
            }

            return false;
        }

        /// <summary>
        /// 주어진 좌표를 MirrorType에 맞게 대칭해서 반환
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>결과 여부</returns>
        private Vector2Int GetFlippedCoordinate(int x, int y)
        {
            switch (options.mirrorType)
            {
                case MirrorType.Horizontal:
                    return new Vector2Int(grid.width - x - 1, y);
                case MirrorType.Vertical:
                    return new Vector2Int(x, grid.height - y - 1);
                case MirrorType.Diagonal:
                    return new Vector2Int(grid.width - x - 1, grid.height - y - 1);
            }

            return new Vector2Int(x, y);
        }
    }
}
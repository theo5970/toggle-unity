using System;
using System.Collections;
using Toggle.Core.Function;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Core.Generator
{
    public partial class LevelGenerator
    {
        /// <summary>
        /// 함수 typePickFunc을 통해 선택한 서브타입을 랜덤한 좌표에 배치 시도
        /// </summary>
        /// <param name="typePickFunc">서브타입을 선택하는 함수</param>
        /// <returns>배치 성공여부</returns>
        private bool PlaceButtonToRandomCoord(Func<FunctionSubType> typePickFunc)
        {
            int availableButtonCount = GetCountWithoutMirror();

            if (!GetRandomEmptyCoordinate(out Vector2Int targetCoord))
            {
                return false;
            }

            FunctionSubType targetSubType = typePickFunc();
            if (targetSubType == FunctionSubType.BHV)
            {
                fourArrowCount++;
                if (fourArrowCount > options.fourArrowPlaceLimit)
                {
                    while (targetSubType == FunctionSubType.BHV)
                    {
                        targetSubType = typePickFunc();
                    }
                    Debug.LogWarning("Four Arrow Limit");
                }
            }

            if (CanPlace(targetCoord, targetSubType))
            {
                grid[targetCoord].functionSubType = targetSubType;
                // Debug.LogFormat("Successful to place {0} to {1}", targetSubType, targetCoord);
            }
            else
            {
                BitArray visited = new BitArray(grid.width * grid.height);

                bool canReplace = true;
                while (true)
                {
                    Vector2Int otherCoord = GetRandomCoordinate();
                    int visitedFlagIndex = otherCoord.y * grid.width + otherCoord.x;

                    FunctionSubType otherSubType = grid[otherCoord].functionSubType;
                    if (visited[visitedFlagIndex] || targetCoord == otherCoord) continue;

                    targetSubType = typePickFunc();
                    if (targetSubType == FunctionSubType.BHV)
                    {
                        fourArrowCount++;
                        if (fourArrowCount > options.fourArrowPlaceLimit)
                        {
                            while (targetSubType == FunctionSubType.BHV)
                            {
                                targetSubType = typePickFunc();
                            }
                            Debug.LogWarning("Four Arrow Limit");
                        }
                    }

                    if (CanPlace(targetCoord, otherSubType) && CanPlace(otherCoord, targetSubType))
                    {
                        grid[targetCoord].functionSubType = otherSubType;
                        grid[otherCoord].functionSubType = targetSubType;
                        /* Debug.LogFormat("Replace {0} ({1}) <-> {2} ({3})", targetSubType, targetCoord, otherSubType,
                        otherCoord); */
                        break;
                    }

                    visited.Set(visitedFlagIndex, true);

                    /* Debug.LogWarningFormat("Can't replace {0} ({1}) <-> {2} ({3}) visited count: {4}", targetSubType,
                    targetCoord,
                    otherSubType, otherCoord, visited.CountSetBits());*/

                    if (visited.CountSetBits() == availableButtonCount - 1)
                    {
                        canReplace = false;
                        break;
                    }
                }

                if (!canReplace)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
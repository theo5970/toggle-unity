using System;
using System.Collections;
using System.Collections.Generic;
using Toggle.Core.Function;
using Toggle.Utils;
using UnityEngine;
using Utils;


namespace Toggle.Core.Generator
{
    public partial class LevelGenerator
    {
        private System.Random random = new System.Random((int)DateTime.Now.Ticks);

        [Serializable]
        public struct Options
        {
            public Vector2Int mapSize;
            public FunctionTypeFlags typeFlags;
            public MirrorType mirrorType;
            public int buttonCount;
            public int clickCount;
            public bool shouldPlaceEssentialType;
            public bool shouldClickEssentialType;
            public int fourArrowPlaceLimit;
        }

        public struct Result
        {
            public int placedButtonCount;
            public List<Vector2Int> clickOrders;
        }

        public ButtonGrid grid;
        public Options options;

        #region 조합 관련 기능

        private static readonly FunctionSubType[][] combinationMap =
        {
            new[] {FunctionSubType.L, FunctionSubType.D, FunctionSubType.R, FunctionSubType.U},
            new[] {FunctionSubType.DLD, FunctionSubType.DLU, FunctionSubType.DRD, FunctionSubType.DRU},
            new[] {FunctionSubType.BH, FunctionSubType.BV},
            new[] {FunctionSubType.DBLDRU, FunctionSubType.DBLURD},
            new[] {FunctionSubType.BHV},
            new[] {FunctionSubType.RC, FunctionSubType.RCC},
            new[] {FunctionSubType.SYH, FunctionSubType.SYV},
            new[] {FunctionSubType.SHL, FunctionSubType.SHR}
        };

        private readonly List<FunctionSubType[]> combinations = new List<FunctionSubType[]>();

        /// <summary>
        /// 주어진 조합 플래그에 따라 combinations 리스트에 추가
        /// </summary>
        /// <param name="flags">조합 플래그</param>
        private void SolveCombinations(FunctionTypeFlags flags)
        {
            combinations.Clear();
            if (flags.HasFlag(FunctionTypeFlags.OneArrowLinear))
            {
                combinations.Add(combinationMap[0]);
            }

            if (flags.HasFlag(FunctionTypeFlags.OneArrowDiagonal))
            {
                combinations.Add(combinationMap[1]);
            }

            if (flags.HasFlag(FunctionTypeFlags.TwoArrowLinear))
            {
                combinations.Add(combinationMap[2]);
            }

            if (flags.HasFlag(FunctionTypeFlags.TwoArrowDiagonal))
            {
                combinations.Add(combinationMap[3]);
            }

            if (flags.HasFlag(FunctionTypeFlags.FourArrow))
            {
                combinations.Add(combinationMap[4]);
            }

            if (flags.HasFlag(FunctionTypeFlags.Rotate))
            {
                combinations.Add(combinationMap[5]);
            }

            if (flags.HasFlag(FunctionTypeFlags.Symmetry))
            {
                combinations.Add(combinationMap[6]);
            }
        }


        #endregion

        #region 랜덤 좌표 기능

        /// <summary>
        /// 랜덤 좌표 반환 (단, 대칭 유형도 반영)
        /// </summary>
        /// <returns>결과 좌표</returns>
        private Vector2Int GetRandomCoordinate()
        {
            int x = 0, y = 0;
            switch (options.mirrorType)
            {
                case MirrorType.None:
                    x = random.Next(0, grid.width);
                    y = random.Next(0, grid.height);
                    break;
                case MirrorType.Horizontal:
                    x = random.Next(0, grid.width / 2);
                    y = random.Next(0, grid.height);
                    break;
                case MirrorType.Vertical:
                    x = random.Next(0, grid.width);
                    y = random.Next(0, grid.height / 2);
                    break;
                case MirrorType.Diagonal:
                    x = random.Next(0, grid.width - 1);
                    y = random.Next(x + 1, grid.height);
                    break;
            }

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 배치되지 않은 (=서브타입이 없는) 랜덤 좌표 반환
        /// </summary>
        /// <param name="coord">결과 좌표</param>
        /// <returns>배치할 수 있는 버튼의 존재유무</returns>
        private bool GetRandomEmptyCoordinate(out Vector2Int coord)
        {
            BitArray visited = new BitArray(grid.width * grid.height);
            int availableButtonCount = GetCountWithoutMirror();

            bool isGridFull = false;
            do
            {
                coord = GetRandomCoordinate();

                int visitedFlagIndex = coord.y * grid.width + coord.x;
                if (visited[visitedFlagIndex]) continue;

                visited.Set(visitedFlagIndex, true);

                if (visited.CountSetBits() == availableButtonCount - 1)
                {
                    isGridFull = true;
                    break;
                }
            } while (grid[coord].functionSubType != FunctionSubType.NOP);

            return !isGridFull;
        }

        #endregion

        /// <summary>
        /// 대칭 버튼을 제외한 버튼의 총 개수
        /// </summary>
        private int GetCountWithoutMirror()
        {
            switch (options.mirrorType)
            {
                case MirrorType.None:
                    return grid.width * grid.height;
                case MirrorType.Horizontal:
                    return (grid.width / 2) * grid.height;
                case MirrorType.Vertical:
                    return grid.width * (grid.height / 2);
                case MirrorType.Diagonal:
                    return (grid.width - 1) * grid.width / 2; // sum (1 to grid.width-1)
                default:
                    return 0;
            }
        }

        protected List<Vector2Int> orders = new List<Vector2Int>();
        private int fourArrowCount = 0;
        /// <summary>
        /// 레벨 생성
        /// </summary>
        /// <returns>테스트 코루틴에 사용할 열거자</returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual Result Generate()
        {
            fourArrowCount = 0;

            grid.Resize(options.mapSize.x, options.mapSize.y);
            ClearAndResetGrid();

            // 0. 조합 플래그로 가능한 서브타입 리스트 채워놓기
            SolveCombinations(options.typeFlags);
            orders.Clear();

            int remainingButtonCount = options.buttonCount;
            int placedButtonCount = 0;

            if (options.mirrorType != MirrorType.None)
            {
                if (grid.width != grid.height)
                {
                    throw new ArgumentException("가로 x 세로 같아야함 ㅅㄱ");
                }

                remainingButtonCount /= 2;
            }

            // 1-1. 버튼 사전 배치 (필수 조합별로 1개씩)
            if (options.shouldPlaceEssentialType)
            {
                PlaceEssentialButtons(ref remainingButtonCount, ref placedButtonCount);
            }

            // Debug.Log("1-1. 버튼 사전 배치 완료.");

            // 1-2. 버튼 랜덤 배치
            PlaceRandomButtons(ref remainingButtonCount, ref placedButtonCount);
            // Debug.Log("1-2. 버튼 랜덤 배치 완료.");

            // 2. 맵 생성순서 추가
            int remainingClickCount = options.clickCount;
            if (options.mirrorType != MirrorType.None)
            {
                remainingClickCount /= 2;
            }

            if (remainingClickCount > placedButtonCount)
            {
#if UNITY_EDITOR
                Debug.LogError($"클릭을 이만큼 할 수가 읎어요!!! 프로그래머 이양반아 ({remainingClickCount} > {options.buttonCount})");
#endif
                remainingClickCount = placedButtonCount;
            }

            // 2-1. 필수조합 버튼을 먼저 순서에 추가
            if (options.shouldClickEssentialType)
            {
                ClickEssentialButtons(ref remainingClickCount);
            }

            // Debug.Log("2-1. 필수조합 버튼을 먼저 순서에 추가 완료.");
            ClickRandomButtons(ref remainingClickCount);


            // Debug.Log("2-2. 랜덤 버튼을 순서에 추가 완료");

            // 3. 대칭 배치 (대칭 유형에 따라 기능모양 뒤집기)
            if (options.mirrorType != MirrorType.None)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    for (int x = 0; x < grid.width; x++)
                    {
                        Vector2Int flippedCoord = GetFlippedCoordinate(x, y);
                        switch (options.mirrorType)
                        {
                            case MirrorType.Horizontal:
                                if (x >= grid.width / 2)
                                {
                                    grid[x, y].functionSubType = FlipHorizontal(grid[flippedCoord].functionSubType);
                                }

                                break;
                            case MirrorType.Vertical:
                                if (y >= grid.height / 2)
                                {
                                    grid[x, y].functionSubType = FlipVertical(grid[flippedCoord].functionSubType);
                                }

                                break;
                            case MirrorType.Diagonal:
                                if (x >= y)
                                {
                                    grid[x, y].functionSubType =
                                        FlipHorizontal(FlipVertical(grid[flippedCoord].functionSubType));
                                }

                                break;
                        }
                    }
                }

                // Debug.Log("뒤집기 끝");

                int oldOrderCount = orders.Count;
                for (int i = 0; i < oldOrderCount; i++)
                {
                    Vector2Int flippedCoord = GetFlippedCoordinate(orders[i].x, orders[i].y);
                    orders.Add(flippedCoord);
                }
            }

            // 4. 맵 생성순서대로 버튼 클릭
            for (int i = 0; i < orders.Count; i++)
            {
                BaseButton button = grid[orders[i]];
                button.function.OnClicked(button);
            }

            Result result = new Result
            {
                clickOrders = new List<Vector2Int>(orders),
                placedButtonCount = placedButtonCount
            };
            return result;
        }

        private void ClearAndResetGrid()
        {
            grid.SetAllState(false);
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    grid[x, y].functionSubType = FunctionSubType.NOP;
                }
            }
        }

        private void PlaceEssentialButtons(ref int remainingButtonCount, ref int placedButtonCount)
        {
            for (int i = 0; i < combinations.Count; i++)
            {
                FunctionSubType[] currentCombination = combinations[i];


                bool placeResult =
                    PlaceButtonToRandomCoord(() => currentCombination[random.Next(0, currentCombination.Length)]);
                if (!placeResult)
                {
#if UNITY_EDITOR
                    Debug.LogError("필수버튼 배치 실패!");
#endif
                }

                remainingButtonCount--;
                placedButtonCount++;
            }
        }

        private FunctionSubType PickRandomSubType()
        {
            int combinationIndex = random.Next(0, combinations.Count);
            FunctionSubType[] currentCombination = combinations[combinationIndex];

            return currentCombination[random.Next(0, currentCombination.Length)];
        }

        private void PlaceRandomButtons(ref int remainingButtonCount, ref int placedButtonCount)
        {
            while (remainingButtonCount > 0)
            {
                bool placeResult = PlaceButtonToRandomCoord(PickRandomSubType);

                if (!placeResult)
                {
#if UNITY_EDITOR
                    Debug.LogErrorFormat("버튼 배치 실패. 아직 {0}개의 버튼이 남았으나...", remainingButtonCount);
#endif
                    break;
                }

                remainingButtonCount--;
                placedButtonCount++;
            }
        }

        private void ClickEssentialButtons(ref int remainingClickCount)
        {
            FunctionTypeFlags currentFlags = FunctionTypeFlags.None;
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    BaseButton button = grid[x, y];
                    if (CheckIfMirrorOut(x, y) ||
                        button.functionSubType == FunctionSubType.NOP) continue;

                    FunctionTypeFlags functionTypeFlags = FunctionTypeFlagsUtils.ConvertSubTypeToCombination(button.functionSubType);

                    if ((currentFlags & functionTypeFlags) == FunctionTypeFlags.None)
                    {
                        orders.Add(button.coordinate);
                        currentFlags |= functionTypeFlags;

                        remainingClickCount--;
                    }
                }
            }
        }

        private void ClickRandomButtons(ref int remainingClickCount)
        {
            while (remainingClickCount > 0)
            {
                Vector2Int randomCoord = new Vector2Int(random.Next(0, grid.width), random.Next(0, grid.height));
                BaseButton button = grid[randomCoord];

                if (CheckIfMirrorOut(randomCoord.x, randomCoord.y) ||
                    button.functionSubType == FunctionSubType.NOP) continue;

                bool isDuplicate = false;
                for (int i = orders.Count - 1; i >= 0; i--)
                {
                    BaseButton otherButton = grid[orders[i]];
                    if (TypeUtils.CheckIfOrderImportant(otherButton.functionSubType)) break;

                    if (otherButton.coordinate == button.coordinate)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    orders.Add(randomCoord);
                    remainingClickCount--;
                }
            }
        }
    }


    /// <summary>
    /// 대칭 유형
    /// </summary>
    public enum MirrorType
    {
        None,
        Horizontal,
        Vertical,
        Diagonal,
    }
}
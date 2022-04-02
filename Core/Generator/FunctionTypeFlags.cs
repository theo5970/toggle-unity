using System;
using Toggle.Core.Function;

namespace Toggle.Core.Generator
{
    /// <summary>
    /// 기능 타입 + 플래그
    /// </summary>
    [Flags]
    public enum FunctionTypeFlags
    {
        None = 0,
        OneArrowLinear = 1,
        OneArrowDiagonal = 2,
        TwoArrowLinear = 4,
        TwoArrowDiagonal = 8,
        FourArrow = 16,
        Rotate = 32,
        Symmetry = 64,
        Shift = 128,
        All = 255
    }

    public static class FunctionTypeFlagsUtils
    {

        /// <summary>
        /// 서브타입을 조합 플래그로 변환
        /// </summary>
        /// <param name="subType">서브타입</param>
        /// <returns>조합 플래그</returns>
        public static FunctionTypeFlags ConvertSubTypeToCombination(FunctionSubType subType)
        {
            switch (subType)
            {
                case FunctionSubType.U:
                case FunctionSubType.D:
                case FunctionSubType.L:
                case FunctionSubType.R:
                    return FunctionTypeFlags.OneArrowLinear;
                case FunctionSubType.DLU:
                case FunctionSubType.DRU:
                case FunctionSubType.DLD:
                case FunctionSubType.DRD:
                    return FunctionTypeFlags.OneArrowDiagonal;
                case FunctionSubType.BH:
                case FunctionSubType.BV:
                    return FunctionTypeFlags.TwoArrowLinear;
                case FunctionSubType.BHV:
                    return FunctionTypeFlags.FourArrow;
                case FunctionSubType.DBLURD:
                case FunctionSubType.DBLDRU:
                    return FunctionTypeFlags.TwoArrowDiagonal;
                case FunctionSubType.RC:
                case FunctionSubType.RCC:
                    return FunctionTypeFlags.Rotate;
                case FunctionSubType.SYH:
                case FunctionSubType.SYV:
                    return FunctionTypeFlags.Symmetry;
                case FunctionSubType.SHR:
                case FunctionSubType.SHL:
                    return FunctionTypeFlags.Shift;
                default:
                    return FunctionTypeFlags.None;
            }
        }
    }
}
using System;
using Toggle.Utils;

namespace Toggle.Core.Function
{
    public static class ConvertMap
    {
        private static Map<FunctionSubType, int> typeIntMap;

        static ConvertMap()
        {
            typeIntMap = new Map<FunctionSubType, int>();

            foreach (FunctionSubType subType in Enum.GetValues(typeof(FunctionSubType)))
            {
                typeIntMap.Add(subType, (int)subType);
            }
        }

        public static int TypeToInt(FunctionSubType subType)
        {
            return typeIntMap.Forward[subType];
        }

        public static FunctionSubType IntToType(int i)
        {
            return typeIntMap.Reverse[i];
        }
    }
}
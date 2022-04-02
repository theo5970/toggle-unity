using System.Linq;
using Toggle.Core.Function;

namespace Utils
{
    public static class TypeUtils
    {
        public static bool CheckIfOrderImportant(FunctionSubType subType)
        {
            switch (subType)
            {
                case FunctionSubType.SYH:
                case FunctionSubType.SYV:
                case FunctionSubType.RC:
                case FunctionSubType.RCC:
                case FunctionSubType.SHL:
                case FunctionSubType.SHR:
                    return true;
            }

            return false;
        }

        public static BaseFunction GetFunctionBySubType(FunctionSubType subType)
        {
            foreach (BaseFunction function in FunctionTypeMap.GetFunctions())
            {
                FunctionSubType[] supportedTypes = function.GetSupportedTypes();
                if (supportedTypes.Contains(subType))
                {
                    return function;
                }
            }

            return null;
        }
    }
}
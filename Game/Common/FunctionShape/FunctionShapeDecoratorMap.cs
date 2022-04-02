using System;
using System.Collections.Generic;
using System.Reflection;
using Toggle.Core.Function;

namespace Toggle.Game.Common.FunctionShape
{
    public static class FunctionShapeDecoratorMap
    {
        private static Dictionary<FunctionSubType, FunctionShapeDecorator> decorators;
        
        static FunctionShapeDecoratorMap()
        {
            decorators = new Dictionary<FunctionSubType, FunctionShapeDecorator>();

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            foreach (Type type in currentAssembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(FunctionShapeDecorator)))
                {
                    var decorator = Activator.CreateInstance(type) as FunctionShapeDecorator;
                    Register(decorator);
                }
            }
        }

        public static FunctionShapeDecorator Get(FunctionSubType subType)
        {
            return decorators[subType];
        }
        
        static void Register(FunctionShapeDecorator decorator)
        {
            BaseFunction function = FunctionTypeMap.Get(decorator.functionType);
            foreach (var supportedSubType in function.GetSupportedTypes())
            {
                decorators.Add(supportedSubType, decorator);
            }
        }
    }
}
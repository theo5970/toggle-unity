using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toggle.Core.Function
{
    public static class FunctionTypeMap
    {
        private static Dictionary<Type, BaseFunction> map;

        static FunctionTypeMap()
        {
            map = new Dictionary<Type, BaseFunction>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            var functionTypes = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(BaseFunction)));

            foreach (var functionType in functionTypes)
            {
                Register(Activator.CreateInstance(functionType) as BaseFunction);
            }
        }

        public static BaseFunction Get(Type type)
        {
            return map[type];
        }

        public static IEnumerable<BaseFunction> GetFunctions()
        {
            return map.Values;
        }
        
        private static void Register(BaseFunction function)
        {
            if (!map.ContainsKey(function.GetType()))
            {
                map.Add(function.GetType(), function);
            }
        }
    }
}
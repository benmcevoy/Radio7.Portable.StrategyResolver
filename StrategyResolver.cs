using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Radio7.Portable.StrategyResolver
{
    public class StrategyResolver<TStrategy>
    {
        private static Dictionary<string, TStrategy> _strategies;

        public TStrategy Resolve(string key)
        {
            if (_strategies == null)
            {
                var callingAssembly = Assembly.GetCallingAssembly();
                Scan(callingAssembly);
            }

            return ResolveImpl(key, default(TStrategy));
        }

        public TStrategy Resolve(string key, TStrategy defaultStrategy)
        {
            if (_strategies == null)
            {
                var callingAssembly = Assembly.GetCallingAssembly();
                Scan(callingAssembly);
            }

            return ResolveImpl(key, defaultStrategy);
        }

        private static TStrategy ResolveImpl(string key, TStrategy defaultStrategy)
        {
            var lowerKey = key.ToLowerInvariant();

            if (_strategies != null && _strategies.ContainsKey(lowerKey))
            {
                return _strategies[lowerKey];
            }

            return defaultStrategy;
        }

        private static void Scan(Assembly assembly)
        {
            _strategies = new Dictionary<string, TStrategy>();

            var strategyType = typeof(TStrategy);
            var types = assembly
                .GetTypes()
                .Where(t => strategyType.IsAssignableFrom(t) && t.IsClass)
                .ToList();

            foreach (var type in types)
            {
                var strategyFor = type.GetCustomAttributes(typeof(StrategyForAttribute), false).FirstOrDefault();

                if (strategyFor == null) continue;

                var strategyForAttribute = strategyFor as StrategyForAttribute;

                if (strategyForAttribute == null) continue;

                _strategies[strategyForAttribute.Key] = (TStrategy)Activator.CreateInstance(type);
            }
        }
    }
}

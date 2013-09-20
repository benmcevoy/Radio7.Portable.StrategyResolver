using System;

namespace Radio7.Portable.StrategyResolver
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StrategyForAttribute : Attribute
    {
        public StrategyForAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}

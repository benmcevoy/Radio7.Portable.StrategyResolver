Radio7.Portable.StrategyResolver
================================

http://blog.benmcevoy.com.au/resolve-strategy-by-key

Inject a StrategyResolver for the desired strategy implementations, and then resolve the correct strategy based on a key.

This sort of thing is probably best left to a DI container, using a factory to resolve. But anyways...

We can inject a StrategyResolver<TStrategy> for our strategy implementations, and then Resolve the correct strategy based on a key.

This lets use resolve the strategy through data.  For example, I've been doing a bit web scraping and manipulation, so using this technique I can resolve a strategy based on the domain of the page I am processing.

<pre><code>
	// create a resolver (or inject one)
    StrategyResolver _strategyResolver = new StrategyResolver<IMyStrategyInterface>();
	
    // use it to retrieve an implementation based on some key
    _myStrategy = _strategyResolver.Resolve(url.Host);

	// create and decorate your strategy implementation
    [StrategyFor("news.ycombinator.com")]
    public class HackerNewsStrategy : IMyStrategyInterface

</code></pre>

The resolver scans the calling assembly for implementations of TStrategy.  When any are found they are stashed away in a dictionary.

Later we can retrieve them by the key.  If nothing is found then we will return the default(TStrategy), which is probably going to be null.

Whether this is a good implementation or idea or not, I am finding it quite nice to be able to just add a new class implementing my IStrategy, decorate it and... that's it. Done.

The resolver class:
<pre><code>
    public class StrategyResolver&lt;TStrategy&gt; 
    {
        private static Dictionary&lt;string, TStrategy&gt; _strategies;

        public TStrategy Resolve(string key)
        {
            var assembly = Assembly.GetCallingAssembly();

            return ResolveImpl(key, assembly);
        }

        public TStrategy Resolve(string key, TStrategy defaultStrategy)
        {
            var assembly = Assembly.GetCallingAssembly();

            return ResolveImpl(key, assembly, defaultStrategy);
        }

        private static TStrategy ResolveImpl(string key, Assembly callingAssembly, TStrategy defaultStrategy = default(TStrategy))
        {
            if (_strategies == null) Load(callingAssembly);

            var lowerKey = key.ToLowerInvariant();

            if (_strategies != null && _strategies.ContainsKey(lowerKey))
            {
                return _strategies[lowerKey];
            }

            return defaultStrategy;
        }

        private static void Load(Assembly assembly)
        {
            _strategies = new Dictionary&lt;string, TStrategy&gt;();

            var strategyType = typeof(TStrategy);
            var types = assembly
                .GetTypes()
                .Where(t =&gt; strategyType.IsAssignableFrom(t) && t.IsClass)
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

</code></pre>

Each strategy implementation is decorated with the [StrategyFor] attribute.

<pre><code>
    [AttributeUsage(AttributeTargets.Class)]
    public class StrategyForAttribute : Attribute
    {
        public StrategyForAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }

</code></pre>

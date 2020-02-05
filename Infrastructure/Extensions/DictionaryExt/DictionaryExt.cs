using System;
using System.Collections.Generic;

namespace maxbl4.Infrastructure.Extensions.DictionaryExt
{
    public static class DictionaryExt
    {
        public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TK,TV> def = null)
        {
            if (def == null)
                def = x => default;
            return dict.TryGetValue(key, out var v) ? v : def(key);
        }
        
        public static TV GetOrAdd<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TK,TV> valueFactory)
        {
            if (dict.TryGetValue(key, out var val))
                return val;
            dict[key] = val = valueFactory(key);
            return val;
        }
        
        public static TV AddOrUpdate<TK,TV>(this IDictionary<TK, TV> dict, TK key, TV newValue, Func<TK,TV,TV> update)
        {
            if (!dict.ContainsKey(key))
                return dict[key] = newValue;
            return dict[key] = update(key, dict[key]);
        }

        public static TV UpdateOrAdd<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TV, TV> updateFunc, TV defaultValue = default)
        {
            if (!dict.TryGetValue(key, out var val))
                val = defaultValue;
            val = updateFunc(val);
            dict[key] = val;
            return val;
        }
    }
}
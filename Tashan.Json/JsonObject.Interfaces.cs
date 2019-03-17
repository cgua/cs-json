using System.Collections;
using System.Collections.Generic;

namespace Tashan.Json
{
    partial class JsonObject : IEnumerable<KeyValuePair<string, JsonObject>>
    {
        /// <summary>
        /// 判断是否包含 key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => _dict.ContainsKey( key );

        public IEnumerator<KeyValuePair<string, JsonObject>> GetEnumerator()
        {
            foreach(var p in _dict)
            {
                yield return new KeyValuePair<string, JsonObject>( p.Key, new JsonObject( p.Key, p.Value ));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    }
}

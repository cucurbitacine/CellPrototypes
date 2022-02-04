using System.Collections.Concurrent;
using UnityEngine;

namespace Prototype01.Scripts
{
    public static class PrefabList
    {
        public const string Cell = "Cell";
        public const string Food = "Food";
        
        private static readonly ConcurrentDictionary<string, Object> Map = new ConcurrentDictionary<string, Object>();

        public static T GetPrefab<T>(string key) where T : Object
        {
            if (!Map.TryGetValue(key, out var prefab))
            {
                prefab = Resources.Load<T>(key);
                Map.TryAdd(key, prefab);
            }

            return (T) prefab;
        }
    }
}
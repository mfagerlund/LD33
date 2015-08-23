using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class Extensions
    {
        public static T RandomItem<T>(this IList<T> items)
        {
            if (!items.Any())
            {
                return default(T);
            }

            return items[Random.Range(0, items.Count)];
        }
    }
}
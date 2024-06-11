namespace UniversalProductScraper.Extensions
{
    using System.Collections.Generic;

    public static class DictionaryEx
    {
        public static void AddRangeIgnoreExist(this IDictionary<string, object> target, IDictionary<string, object> elements)
        {
            foreach (var element in elements)
            {
                if (!target.ContainsKey(element.Key))
                {
                    target.Add(element.Key, element.Value);
                }
            }
        }
    }
}
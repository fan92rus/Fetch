namespace UniversalProductScraper.Extensions
{
    using System.Collections.Generic;

    public static class ExpandoEx
    {
        public static void AddRange(this IDictionary<string, object> target, IDictionary<string, object> elements)
        {
            foreach (var element in elements) target.Add(element.Key, element.Value);
        }
    }
}
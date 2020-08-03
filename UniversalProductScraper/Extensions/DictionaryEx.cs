namespace UniversalProductScraper.Extensions
{
    using System.Collections.Generic;
    using System.Dynamic;

    public static class DictionaryEx
    {
        public static ExpandoObject ToExpandoObject(this IDictionary<string, string> dictionary)
        {
            IDictionary<string, object> target = new ExpandoObject();
            foreach (var element in dictionary) target.Add(element.Key, element.Value);
            return (ExpandoObject)target;
        }
    }
}
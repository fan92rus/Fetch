namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    static class INodeEx
    {
        public static bool IsText(this IElement element)
        {
            return !string.IsNullOrWhiteSpace(element.Text().Replace("\n", "").Replace("\t", "")) && !element.Children.Any();
        }

        public static IElement GetNotVoidParent(this IElement element)
        {
            return element.GetNotVoidParent("HTML");
        }
        public static IElement GetNotVoidParent(this IElement element, string finalTag)
        {

            var final = element;
            var selector = final.GetSelector();
            var parent = element;

            while (true)
            {
                try
                {
                    var target = parent?.ParentElement;

                    if (target != null)
                    {
                        var all = target.QuerySelectorAll(selector);

                        if (all.Length < 2)
                            parent = target;
                        else
                            break;

                        if (parent.TagName == finalTag)
                            break;

                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return parent;
        }

        public static List<IElement> GetAllChildren(this IElement element)
        {
            return GetAllChildren(element, -1);
        }

        public static List<IElement> GetAllChildren(this IElement element, int level)
        {
            List<IElement> elements = new List<IElement>();

            if (level > 0 || level == -1)
                foreach (var ch in element.Children) elements.AddRange(GetAllChildren(ch, level));

            if (level != -1)
                level -= 1;

            elements.AddRange(element.Children.ToList());
            return elements;
        }

        public static IEnumerable<IElement> ExtractValueElements(this IElement doc)
        {
            return ExtractValueElements(doc, -1);
        }
        public static IEnumerable<IElement> ExtractValueElements(this IElement doc, int level)
        {
            var target = new List<IElement>();

            var els = GetAllChildren(doc, level).Where(
                x => x.IsText() || x.Attributes.Any(p => p.Name != "class")
                     && (x.TagName != "I" && x.TagName != "META" && x.TagName != "LINK" && x.TagName != "HTML"));
            return els;
        }

        public static IElement GetContainer(this IElement element)
        {
            return GetContainer(element, 2, "HTML");
        }
        public static IElement GetContainer(this IElement element, string final)
        {
            return GetContainer(element, 2, final);
        }
        public static IElement GetContainer(this IElement element, int layer, string finalElementTag)
        {
            var final = element;
            var selector = final.GetSelector();
            var parent = element;

            while (true)
            {
                try
                {
                    var target = parent?.ParentElement;
                    if (target != null)
                    {
                        var all = target.QuerySelectorAll(selector);

                        parent = target;

                        if (all.Length >= layer || parent.TagName == finalElementTag)
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return parent;
        }

        public static string GetSelector(this IElement element)
        {
            return GetSelector(element, null);
        }
        public static string GetSelector(this IElement element, string maxSelector)
        {
            if (element == null)
                return null;

            var selector = "";

            if (element.TagName != null && !element.TagName.Contains(":"))
                selector = element.TagName.ToLower();
            if (!string.IsNullOrEmpty(selector) && maxSelector != null && !selector.Contains(maxSelector) && element.ParentElement != null)
            {
                var c = element.ParentElement.QuerySelectorAll(selector).Length;
                if (c > 1 && c < 5)
                {
                    var parent = element.ParentElement;

                    var index = parent.Children.Where(_ => _.GetType() == element.GetType()).Index(element);

                    return $":nth-child({index + 1})";
                }
            }
            if (!String.IsNullOrEmpty(element.ClassName))
            {
                var parent = element.ParentElement ?? element;
                var classes = element.ClassList.Select(x => Regex.Replace(x.Trim(), ":.+", ""));

                var enumerator = classes.GetEnumerator();


                bool next;
                var count = 0;

                if (element.Children.Any())
                {
                    var childrenLength = element.Children.Length;

                    do
                    {
                        next = enumerator.MoveNext();
                        if (enumerator.Current != null)
                            selector += "." + enumerator.Current;
                        count = parent.QuerySelectorAll(selector).Length;

                        var compare = parent.QuerySelectorAll(selector).All(x => x.Children.Length == childrenLength);

                        if (compare || (count > 1 && next))
                            break;
                    }
                    while (true);

                    return selector;
                }
                else
                {
                    do
                    {
                        next = enumerator.MoveNext();
                        if (enumerator.Current != null)
                            selector += "." + enumerator.Current;
                        count = parent.QuerySelectorAll(selector).Length;
                    }
                    while (count > 1 && next);
                }
            }
            else if (element.ParentElement != null && !element.ParentElement.GetSelector().Contains("#") && !string.IsNullOrWhiteSpace(selector))
            {
                var parentSelector = element.ParentElement.GetSelector(maxSelector);
                if (maxSelector != null && !parentSelector.Contains(maxSelector))
                    return $"{parentSelector} > {selector}";
            }
            else if (!string.IsNullOrEmpty(element.Id))
                selector += $"#{element.Id}";
            else if (element.Attributes.Any())
            {
                var firstElement = element.Attributes.First();
                selector += $"[{firstElement.Name}]";
            }

            return selector;
        }
    }
}
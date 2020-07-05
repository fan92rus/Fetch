using System;

namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using AngleSharp;
    using AngleSharp.Common;
    using AngleSharp.ContentExtraction;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using MoreLinq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    using RestSharp;

    using ServiceStack;
    using ServiceStack.Text;

    enum CountType
    {
        None,
        Single,
        Multiply
    }

    enum Type
    {
        Link,
        Text,
        Image,
        Attribute,
        Container,

        Button
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            //ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            //service.HideCommandPromptWindow = true;

            //var options = new ChromeOptions();
            ////options.AddArgument("headless");

            //IWebDriver webDriver = new ChromeDriver(service, options);
            //webDriver.Navigate().GoToUrl("https://www.sparheld.de/gutscheine/hans-natur#show=10879");
            //Thread.Sleep(10000);
            //var content = webDriver.FindElement(By.TagName("html")).GetAttribute("innerHTML");
            Url target = new Url("https://www.cuponation.de/top20");
            RestClient rc = new RestClient();
            var resp = rc.Execute(new RestRequest(target));

            var config = Configuration.Default
                .WithDefaultLoader()
                .WithCss()
                .WithJs();

            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var doc = parser.ParseDocument(resp.Content);

            var head = doc.QuerySelector("head");
            var body = doc.QuerySelector("body");
            var voucher = body.QuerySelector(".v.c");
            var parent = voucher.GetNotVoidParent();
            var container = voucher.GetContainer();
            ;

            var finalNode = new InfoNode()
            {
                Element = doc.QuerySelector("html"),
                Selector = "html",
                Nodes = new List<InfoNode>()
                            {
                                                   new InfoNode()
                                                       {
                                                           Element = head,
                                                           Selector = "head",
                                                           Nodes =  ParseNodes(head.ExtractValueElements(), "HEAD").FirstOrDefault().Nodes
                                                       },
                                                   new InfoNode()
                                                       {
                                                           Element = body,
                                                           Selector = "body",
                                                           Nodes =ParseNodes(body.ExtractValueElements(), "BODY").FirstOrDefault().Nodes
                                                       }
                                                }
            };

            MakeTypes(finalNode);

            File.WriteAllText("test.json", finalNode.ToSafeJson());
            File.WriteAllText("finalNodes.json", Parse(finalNode).ToSafeJson());

            Console.WriteLine("Final!!!");
        }

        static void MakeTypes(InfoNode infoNode)
        {
            if (infoNode.Nodes.Any())
            {
                infoNode.Type = Type.Container;

                foreach (var subNode in infoNode.Nodes)
                    MakeTypes(subNode);
            }
            else
            {
                var element = infoNode.Element;

                if (element is IHtmlAnchorElement)
                {
                    infoNode.Type = Type.Link;
                }
                else if (element is IHtmlImageElement)
                {
                    infoNode.Type = Type.Image;
                }
                else if (element is IHtmlButtonElement)
                {
                    infoNode.Type = Type.Button;
                }
                else
                {
                    infoNode.Type = Type.Text;
                }
            }
        }

        static Element Parse(InfoNode info)
        {
            try
            {
                var element = new Element
                {
                    Selector = info.Selector,
                    Nodes = Nodes(info)
                };


                return element;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private static List<Node> Nodes(InfoNode info)
        {
            Console.WriteLine("Parse - " + info.Selector);
            var nodes = new List<Node>();

            var parent = info.Element.ParentElement ?? info.Element;
            var elements = parent.QuerySelectorAll(info.Selector).ToList();

            if (!elements.Any())
                elements.Add(parent);

            foreach (var e in elements)
            {
                var finalNode = new Node
                {
                    Selector = info.Selector,
                    Attributes = e.Attributes.Where(x => x.Name != "class").Select(
                                            x => new KeyValuePair<string, string>(x.Name, x.Value)),
                    Classes = e.ClassList.Select(x => x.ToString())
                };

                if (info.Type == Type.Text || info.Type == Type.Link)
                {
                    finalNode.Text = e.Text();
                    //finalNode.Text = e.GetInnerText();
                }
                finalNode.Nodes = new List<Node>();
                foreach (var node in info.Nodes)
                {
                    finalNode.Nodes.AddRange(Nodes(node));
                }

                nodes.Add(finalNode);
            }

            return nodes;
        }
        
        private static List<InfoNode> ParseNodes(IEnumerable<IElement> doc, string topElement)
        {
            var nodes = new List<InfoNode>();

            foreach (var el in doc)
            {
                if (el.ClassName != null && el.ClassName.Contains("v c"))
                {

                }

                var parent = el.GetContainer(topElement);
                var parentSelector = parent.GetSelector();
                var elementSelctor = el.GetSelector();
                var parentNode = nodes.FirstOrDefault(x => x.Selector == parentSelector);

                if (parentNode == null)
                {
                    if (parentSelector == "div.retailer-vouchers-list")
                    {

                    }
                    Console.WriteLine("Unical element  " + parentSelector);

                    var node = new InfoNode { Selector = parent.GetSelector(), Element = parent, Type = Type.Container };

                    //var children = parent.ExtractValueElements(2).Where(x => x.GetNotVoidParent().GetSelector() == parentSelector);
                    var selectors = new HashSet<string>();

                    foreach (var e in parent.ExtractValueElements(2))
                    {
                        var selector = e.GetSelector();
                        var par = e.GetNotVoidParent().GetSelector();
                        if (par == elementSelctor && !selectors.Contains(selector))
                        {
                            selectors.Add(selector);
                            Console.WriteLine($"Add new Selector {selector}");
                        }
                        else
                        {
                            Console.WriteLine($"Selector contains in collection {selector} for parent {parentSelector}");
                        }
                    }

                    foreach (var selector in selectors)
                    {
                        try
                        {
                            node.Nodes.Add(new InfoNode() { Selector = selector, Element = parent.QuerySelector(selector) });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    nodes.Add(node);
                }
                else
                {
                    Console.WriteLine("Contains element  " + parentSelector);
                }
            }

            while (nodes.Any(x => x.Element.TagName != topElement))
            {
                var nodes1 = nodes;
                var finalNodes = new List<InfoNode>();
                foreach (var x in nodes)
                {
                    var parent = x.Element.GetContainer(topElement);
                    var parentSelector = parent.GetSelector();

                    if (x.Element.TagName == topElement || x.Element.TagName == "HTML" && finalNodes.All(e => e.Selector != x.Selector))
                    {
                        var containsElements = nodes1.Where(e => e.Selector != x.Selector).Where(e => e.Element.GetContainer(topElement).GetSelector() == x.Element.GetSelector());
                        x.Nodes.AddRange(containsElements);
                        x.Nodes = x.Nodes.DistinctBy(e => e.Selector).ToList();
                        finalNodes.Add(x);
                    }
                    else if (finalNodes.All(e => e.Selector != parentSelector))
                    {
                        var node = new InfoNode()
                        {
                            Selector = parentSelector,
                            Element = parent,
                            Nodes = nodes1.Where(e => e.Element.GetContainer().GetSelector() == parentSelector).DistinctBy(e => e.Selector).ToList()
                        };

                        finalNodes.Add(node);
                    }
                    else if (parent.TagName == topElement)
                    {

                    }
                }

                nodes = finalNodes;
            }

            return nodes;
        }
    }

    class InfoNode
    {
        [JsonIgnore]
        public IElement Element { get; set; }
        public string Selector { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Type Type { get; set; }
        public List<InfoNode> Nodes { get; set; } = new List<InfoNode>();
    }

    class Node
    {
        public string Selector { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Attributes { get; set; }
        public IEnumerable<string> Classes { get; set; }
        public string Text { get; set; }
        public List<Node> Nodes { get; set; }
    }

    class Element
    {
        public string Selector { get; set; }
        public List<Node> Nodes { get; set; }
    }
    static class INodeEx
    {
        public static bool IsText(this IElement element)
        {
            return !String.IsNullOrWhiteSpace(element.Text().Replace("\n", "").Replace("\t", "")) && !element.Children.Any();
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

        public static List<IElement> GetAll(this IElement element)
        {
            return GetAll(element, -1);
        }
        public static List<IElement> GetAll(this IElement element, int level)
        {
            List<IElement> elements = new List<IElement>();

            if (level > 0 || level == -1)
                foreach (var ch in element.Children) elements.AddRange(GetAll(ch, level));

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

            var els = GetAll(doc, level).Where(
                x => x.IsText() || x.Attributes.Any(p => p.Name != "class")
                     && (x.TagName != "I" && x.TagName != "META" && x.TagName != "LINK" && x.TagName != "HTML"));
            return els;
        }

        public static IElement GetContainer(this IElement element)
        {
            return GetParent(element, 2, "HTML");
        }
        public static IElement GetContainer(this IElement element, string final)
        {
            return GetParent(element, 2, final);
        }

        private static IElement GetParent(IElement element, int child, string finalElement)
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

                        if (all.Length >= child || parent.TagName == finalElement)
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
            if (element == null)
                return null;

            var selector = "";
            if (element.TagName != null && !element.TagName.Contains(":"))
                selector = element.TagName.ToLower();

            if (!String.IsNullOrEmpty(element.ClassName))
            {
                var parent = element.ParentElement ?? element;
                var classes = element.ClassList.Select(x => Regex.Replace(x.Trim(), ":.+", ""));
                var enumerator = classes.GetEnumerator();


                bool next = false;
                int count = 0;

                if (element.Children.Any())
                {
                    var childs = element.Children.Length;

                    do
                    {
                        next = enumerator.MoveNext();
                        if (enumerator.Current != null)
                            selector += "." + enumerator.Current;
                        count = parent.QuerySelectorAll(selector).Length;

                        var compare = parent.QuerySelectorAll(selector).All(x => x.Children.Length == childs);
                        if (compare)
                            break;
                    }
                    while (count > 1 && next);

                    return selector;
                }

                do
                {
                    next = enumerator.MoveNext();
                    if (enumerator.Current != null)
                        selector += "." + enumerator.Current;
                    count = parent.QuerySelectorAll(selector).Length;
                }
                while (count > 1 && next);
            }
            else if (element.ParentElement != null && !element.ParentElement.GetSelector().Contains("#") && !string.IsNullOrWhiteSpace(selector))
            {
                var parentSelector = element.ParentElement.GetSelector();
                return $"{parentSelector} > {selector}";
            }
            else if (!String.IsNullOrEmpty(element.Id))
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

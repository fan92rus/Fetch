namespace UniversalProductScraper.Models
{
    using System;
    using AngleSharp.Dom;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using UniversalProductScraper.Extensions;

    public enum DomNodeType
    {
        Default,
        Link,
        Text,
        Image,
        Container,
        Button
    }

    class InfoNode : IEquatable<InfoNode>
    {
        public InfoNode()
        {
        }

        public InfoNode(InfoNode node)
        {
            Selector = node.Selector;
            Type = node.Type;
            Element = node.Element;
        }

        public InfoNode(IElement element)
        {
            Element = element;
            Selector = element.ParentElement != null
                           ? element.GetSelector((element.ParentElement).GetSelector())
                           : element.GetSelector();
        }


        [JsonIgnore]
        public IElement Element { get; set; }
        public string Selector { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DomNodeType Type { get; set; }

        public bool Equals(InfoNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var equals = string.Equals(Selector, other.Selector) && Type == other.Type;
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;
            return Equals((InfoNode)obj);
        }

        public override int GetHashCode() => HashCode.Combine(Selector, Type);
    }
}
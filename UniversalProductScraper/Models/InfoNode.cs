namespace UniversalProductScraper.Models
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

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
            this.Selector = node.Selector;
            this.Type = node.Type;
            this.Element = node.Element;
        }

        public InfoNode(IElement element)
        {
            this.Element = element;
            this.Selector = element.ParentElement != null
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
            return string.Equals(this.Selector, other.Selector) && this.Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((InfoNode)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Selector != null ? this.Selector.GetHashCode() : 0) * 397) ^ (int)this.Type;
            }
        }
    }
}
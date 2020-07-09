namespace UniversalProductScraper.Models
{
    using System.Collections.Generic;

    using AngleSharp.Dom;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    class InfoNode
    {
        [JsonIgnore]
        public IElement Element { get; set; }
        public string Selector { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Type Type { get; set; }
        public List<InfoNode> Nodes { get; set; } = new List<InfoNode>();
    }
}
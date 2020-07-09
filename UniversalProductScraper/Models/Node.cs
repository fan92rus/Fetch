namespace UniversalProductScraper.Models
{
    using System.Collections.Generic;

    class Node
    {
        public Node()
        {
            this.Attributes = new List<KeyValuePair<string, string>>();
            this.Classes = new List<string>();
        }
        public string Selector { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public List<string> Classes { get; set; }
        public string Text { get; set; }
        public List<Node> Nodes { get; set; }
    }
}
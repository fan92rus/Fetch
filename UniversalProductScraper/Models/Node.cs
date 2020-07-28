namespace UniversalProductScraper.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using Type = UniversalProductScraper.Type;

    public class Node
    {
        public Node()
        {
            this.Attributes = new List<KeyValuePair<string, string>>();
            //this.Classes = new List<string>();
        }

        public Type Type { get; set; }
        public string Selector { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        //public List<string> Classes { get; set; }
        public string Text { get; set; }
        public List<Node> Nodes { get; set; }

        [JsonIgnore]
        public Node ParentNode { get; set; }

        public List<Node> FindAll(Func<Node, bool> compare) => this.FindAll(compare, this);

        public List<Node> FindAll(Func<Node, bool> compare, Node node)
        {
            try
            {
                List<Node> result = new List<Node>();

                if (this.Check(compare, node))
                    result.Add(node);
                if (node.Nodes != null)
                    foreach (var childNode in node.Nodes)
                    {
                        var res = this.FindAll(compare, childNode);
                        if (res != null)
                            result.AddRange(res);
                    }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private bool Check(Func<Node, bool> compare, Node node)
        {
            try
            {
                if (node == null || compare == null)
                    return false;

                return compare.Invoke(node);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
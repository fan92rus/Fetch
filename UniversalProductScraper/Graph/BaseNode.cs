using System;
using System.Collections.Generic;

namespace UniversalProductScraper.Graph
{
    using System.Linq;
    using UniversalProductScraper.Models;

    public class BaseNode : IEquatable<BaseNode>
    {
        public DomNodeType Type { get; set; }
        public string Selector { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public string Text { get; set; }

        string Clear(string @base) => @base.Replace("-", "_").Replace(".", "_").Replace("[", "").Replace("]", "");

        public Dictionary<string, string> GetProperties()
        {
            var dList = new Dictionary<string, string>();

            foreach (var el in Attributes)
            {
                var key = Clear(el.Key);
                if (!dList.ContainsKey(key) && !string.IsNullOrWhiteSpace(el.Value))
                    dList.Add(key, el.Value);
            }

            if (!string.IsNullOrWhiteSpace(Text))
                dList.Add(Clear(Selector), Text);

            return dList;
        }

        public bool Equals(BaseNode other)
        {
            var equals = Type == other.Type && Selector == other.Selector && ((Text != null && other.Text != null && Text.Equals(other.Text, StringComparison.InvariantCultureIgnoreCase)) || Text == other.Text) && !Attributes.Except(other.Attributes).Any();
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseNode)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (Selector != null ? Selector.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}

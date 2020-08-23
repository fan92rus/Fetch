namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimhashLib;

    public readonly struct TableKey : IEquatable<TableKey>
    {
        public TableKey(string name, string containerSelector, Simhash propertyHash)
        {
            this.Name = name;
            this.ParentKey = containerSelector;
            this.PropertyHash = propertyHash;
        }
        public TableKey(Simhash propertyHash)
        {
            Name = default;
            ParentKey = default;
            this.PropertyHash = propertyHash;
        }

        public Simhash PropertyHash { get; }
        public string Name { get; }
        public string ParentKey { get; }

        private const int EqualsDistance = 20;

        public static TableKey Create(string parentSelector, string key, IDictionary<string, string> els)
        {
            var simhash = new Simhash(Simhash.HashingType.Jenkins);
            simhash.GenerateSimhash(els.Keys.ToList());
            return new TableKey(key, parentSelector, simhash);
        }


        public override bool Equals(object obj)
        {
            return obj is TableKey other && this.Equals(other);
        }

        public bool Equals(TableKey other)
        {
            var distance = this.PropertyHash.distance(other.PropertyHash);
            return distance < EqualsDistance && other.ParentKey == this.ParentKey && string.Equals(this.Name, other.Name);
        }

        public override int GetHashCode()
        {
            return $"{this.Name}{this.ParentKey}{this.PropertyHash.value}".GetHashCode(StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return this.Name;
            //if (this.Name.Contains(">"))
            //    return this.Name.Split(">").Last().Trim();
            //return this.Name.Split('.').Last();
        }
    }
}
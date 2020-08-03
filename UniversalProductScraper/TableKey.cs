namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SimhashLib;

    using UniversalProductScraper.Graph;
    
    public partial struct TableKey : IEquatable<TableKey>
    {
        public TableKey(string name, string containerSelector, Simhash propertyHash)
        {
            this.Name = name;
            this.ParentKey = containerSelector;
            this.PropertyHash = propertyHash;
        }

        public Simhash PropertyHash { get; }
        public string Name { get; }
        public string ParentKey { get; }

        private const int EqualsDistance = 15;

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
            return distance < 20 && other.ParentKey == this.ParentKey && string.Equals(this.Name, other.Name);
        }

        public override int GetHashCode()
        {
            return $"{this.Name}{this.ParentKey}{this.PropertyHash.value}".GetHashCode(StringComparison.CurrentCultureIgnoreCase);
        }


        public override string ToString()
        {
            return $"{this.ParentKey}_{this.Name}_{this.PropertyHash.value.GetHashCode()}";
        }
    }
}
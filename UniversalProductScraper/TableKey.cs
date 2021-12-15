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
            Name = name;
            ParentKey = containerSelector;
            PropertyHash = propertyHash;
        }
        public TableKey(Simhash propertyHash)
        {
            Name = default;
            ParentKey = default;
            PropertyHash = propertyHash;
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


        public override bool Equals(object obj) => obj is TableKey other && Equals(other);

        public bool Equals(TableKey other)
        {
            var distance = PropertyHash.distance(other.PropertyHash);
            return distance < EqualsDistance && other.ParentKey == ParentKey && string.Equals(Name, other.Name);
        }

        public override int GetHashCode() => $"{Name}{ParentKey}{PropertyHash.value}".GetHashCode(StringComparison.CurrentCultureIgnoreCase);

        public override string ToString() => Name;//if (this.Name.Contains(">"))//    return this.Name.Split(">").Last().Trim();//return this.Name.Split('.').Last();
    }
}
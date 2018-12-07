using System;
using System.Collections.Generic;

namespace VectorQuery.Data
{
    public class Code
    {
        internal string Predecessor;
        internal string Key;
        public string Title;
        public string Id;
        public int? Fraction;
        public DateTime? Created;
        public Dictionary<string, Code> Values;
    }
}
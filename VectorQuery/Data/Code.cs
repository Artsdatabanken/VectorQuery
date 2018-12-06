using System;
using System.Collections.Generic;

namespace VectorQuery.Data
{
    public class Code
    {
        public string Predecessor;
        public string Key;
        public string Value;
        public string Id;
        public int Fraction;
        public DateTime Created;
        public List<Code> Successors;
    }
}
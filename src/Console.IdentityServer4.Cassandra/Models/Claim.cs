using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.IdentityServer4.Cassandra.Models
{
    /*
    CREATE TYPE IF NOT EXISTS Claim (
	Type text,
	Value text,
	ValueType text
);
    */
    public class Claim
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.IdentityServer4.Cassandra.Models
{
    /*
    CREATE TYPE IF NOT EXISTS Secret (
  Description text,
  Expiration timestamp,
  Type text,
  Value text
);

    */
    public class Secret
    {
        public string Description { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}

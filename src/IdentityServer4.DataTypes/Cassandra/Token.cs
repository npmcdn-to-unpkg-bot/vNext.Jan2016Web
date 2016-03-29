using System;

namespace IdentityServer4.DataTypes.Cassandra
{
    /*
      CREATE TYPE IF NOT EXISTS Token (
        Key text,
        TokenType int,
        SubjectId text,
        ClientId text,
        JsonCode text,
        Expiry timestamp,
      );

  */

    public class Token
    {
        public virtual string Key { get; set; }
        
        public virtual TokenType TokenType { get; set; }

        public virtual string SubjectId { get; set; }

        public virtual string ClientId { get; set; }
        
        public virtual string JsonCode { get; set; }

        public virtual DateTimeOffset? Expiry { get; set; }
    }
}
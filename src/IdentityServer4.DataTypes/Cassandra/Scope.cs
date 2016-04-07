using System.Collections.Generic;

namespace IdentityServer4.DataTypes.Cassandra
{

    /*
    CREATE TYPE IF NOT EXISTS Scope(
       Enabled boolean,
       Name text,
       DisplayName text,
       Description text,
       Required boolean,
       Emphasize boolean,
       Type int,
       ScopeSecrets list<FROZEN<Secret>>,
       ScopeClaims list<FROZEN<ScopeClaim>>,
       AllowUnrestrictedIntrospection boolean,
       IncludeAllClaimsForUser boolean,
       ClaimsRule text,
       ShowInDiscoveryDocument boolean
    );
        */
    public class Scope
    {

        public virtual bool Enabled { get; set; }

        public virtual string Name { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Description { get; set; }

        public virtual bool Required { get; set; }

        public virtual bool Emphasize { get; set; }

        public virtual int Type { get; set; }

        //
        // Summary:
        //     scope secrets - only relevant for flows that require a secret
        public IEnumerable<Secret> ScopeSecrets { get; set; }

        public IEnumerable<ScopeClaim> ScopeClaims { get; set; }
        
        public virtual bool AllowUnrestrictedIntrospection { get; set; }

        public virtual bool IncludeAllClaimsForUser { get; set; }

        public virtual string ClaimsRule { get; set; }

        public virtual bool ShowInDiscoveryDocument { get; set; }
    }
}
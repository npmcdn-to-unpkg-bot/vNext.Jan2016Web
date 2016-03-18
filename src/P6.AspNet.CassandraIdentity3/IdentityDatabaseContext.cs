using System;
using System.Security.Cryptography.X509Certificates;
using p6.AspNet.Identity3.Common;
using p6.CassandraStore.DAO;
using p6.CassandraStore.Settings;

namespace P6.AspNet.CassandraIdentity3
{
    public interface IIdentityCassandraContext<TUser, TRole, TKey>
        where TRole : IdentityRole<TKey>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        ICassandraDAO CassandraDAO { get; }
    }

    public class IdentityCassandraContext : IdentityCassandraContext<IdentityUser, IdentityRole, string>
    {
        public IdentityCassandraContext(CassandraConfig cassandraConfig) : base(cassandraConfig)
        {
        }
    }

    public class IdentityCassandraContext<TUser, TRole, TKey> : IIdentityCassandraContext<TUser, TRole, TKey>
        where TRole : IdentityRole<TKey>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        private CassandraConfig _cassandraConfig { get; set; }
        public IdentityCassandraContext(CassandraConfig cassandraConfig)
        {
            _cassandraConfig = cassandraConfig;
        }

        private ICassandraDAO _cassandraDAO { get; set; }

        public ICassandraDAO CassandraDAO => _cassandraDAO ?? (_cassandraDAO = new CassandraDao(_cassandraConfig));
    }
}
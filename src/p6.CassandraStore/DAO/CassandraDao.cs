using Cassandra;
using p6.CassandraStore.Settings;

namespace p6.CassandraStore.DAO
{
    public class CassandraDao : ICassandraDAO
    {
        private  Cluster _cluster;
        private  Cluster Cluster => _cluster ?? (_cluster = Connect());

        private ISession _session;
        private ISession Session => _session ?? (_session = Cluster.Connect());

        private CassandraConfig CassandraConfig { get; }

        public CassandraDao(CassandraConfig cassandraConfig )
        {
            CassandraConfig = cassandraConfig;  
        }

        private Cluster Connect()
        {
            QueryOptions queryOptions = new QueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.One);

            Cluster cluster = Cassandra.Cluster.Builder()
                .AddContactPoints(CassandraConfig.ContactPoints)
                .WithCredentials(CassandraConfig.Credentials.UserName, CassandraConfig.Credentials.UserName)
                .WithQueryOptions(queryOptions)
                .Build();

            return cluster;
        }

        public ISession GetSession()
        {
            return Session;
        }
    }
}

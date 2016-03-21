using System;
using Cassandra;
using p6.CassandraStore.Settings;

namespace p6.CassandraStore.DAO
{
    public class CassandraDao : ICassandraDAO
    {
        private  Cluster _cluster;
        private  Cluster Cluster => _cluster ?? (_cluster = Connect());

        private ISession _session;

        private ISession Session
        {
            get
            {
                if (_session == null)
                {
                    try
                    {
                        _session = Cluster.Connect(CassandraConfig.KeySpace);
                    }
                    catch (Exception e)
                    {
                        _session = null;
                        throw;
                    }
                }
                return _session;

            }
        }

        private CassandraConfig CassandraConfig { get; }

        public CassandraDao(CassandraConfig cassandraConfig )
        {
            CassandraConfig = cassandraConfig;  
        }

        private Cluster Connect()
        {
            try
            {
                QueryOptions queryOptions = new QueryOptions()
                    .SetConsistencyLevel(ConsistencyLevel.One);
                var builder = Cassandra.Cluster.Builder();
                builder.AddContactPoints(CassandraConfig.ContactPoints);

                if (!string.IsNullOrEmpty(CassandraConfig.Credentials.UserName) &&
                    !string.IsNullOrEmpty(CassandraConfig.Credentials.Password))
                {
                    builder.WithCredentials(
                        CassandraConfig.Credentials.UserName,
                        CassandraConfig.Credentials.Password);
                }
                builder.WithQueryOptions(queryOptions);
                Cluster cluster = builder.Build();

                return cluster;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ISession GetSession()
        {
            return Session;
        }
    }
}

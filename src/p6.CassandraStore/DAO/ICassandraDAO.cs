using Cassandra;

namespace p6.CassandraStore.DAO
{
    public interface ICassandraDAO
    {
        ISession GetSession();
    }
}
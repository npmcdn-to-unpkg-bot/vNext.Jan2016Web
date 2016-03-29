using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using IdentityServer4.DataTypes.Cassandra;
using p6.CassandraStore.DAO;
using p6.CassandraStore.Settings;

namespace Console.IdentityServer4.Cassandra
{
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<Task<T>> taskFactory, bool startNewTaskForFactoryMethod = false)
            : base(startNewTaskForFactoryMethod ? () => StartNewTask(taskFactory) : taskFactory)
        {
        }

        public ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter GetAwaiter()
        {
            // Since we're using this class in the context of our library, always use ConfigureAwait(false) with the Task
            return Value.ConfigureAwait(false).GetAwaiter();
        }

        private static Task<T> StartNewTask(Func<Task<T>> taskFactory)
        {
            return Task.Factory.StartNew(taskFactory).Unwrap();
        }
    }

    public class Program
    {

        public static MemoryCache MemoryCache { get; private set; }
        private static  AsyncLazy<PreparedStatement> _findById;

        private static ISession _cassandraSession = null;
        public static ISession CassandraSession
        {
            get
            {
                try
                {
                    if (_cassandraSession == null)
                    {
                        var cc = new CassandraConfig
                        {
                            ContactPoints = new List<string> {"cassandra"},
                            Credentials = new CassandraCredentials() {Password = "", UserName = ""},
                            KeySpace = "identityserver4"
                        };
                        var dao = new CassandraDao(cc);
                        _cassandraSession = dao.GetSession();
                        _findById =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * FROM clients WHERE id = ?"));

                        _cassandraSession.UserDefinedTypes.Define(
                            UdtMap.For<Secret>(),
                            UdtMap.For<ClientClaim>(),
                            UdtMap.For<Client>()
                                .Map(a => a._AccessTokenType, "accesstokentype")
                                .Map(a => a._Flow, "flow")
                                .Map(a => a._RefreshTokenExpiration, "refreshtokenexpiration")
                                .Map(a => a._RefreshTokenUsage, "refreshtokenusage")
                            );

                    }
                }
                catch (Exception e)
                {
                    _cassandraSession = null;
                }
                return _cassandraSession;
            }
        }
        public static   void Main(string[] args)
        {
            Task.Run(async () =>
            {
                MemoryCache = new MemoryCache("Global");

                // Do any async anything you need here without worry

                var session = CassandraSession;
                while (true)
                {
                    //ac4d5da0-8295-44ff-a448-e6d4119ea3ff
                    var client = await FindByIdAsync("ac4d5da0-8295-44ff-a448-e6d4119ea3ff");
                    System.Console.WriteLine("{0}",client.ClientId);
                    System.Console.ReadLine();

                }
            }).Wait();

           
        }
        static string ToCqlString(Guid guid)
        {
            var bytes = guid.ToByteArray();
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < 16; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    sb.Append("-");
                }
                var b = bytes[i];
                sb.AppendFormat("{0:x2}", b);

            }
            return sb.ToString();
        }
        public static async Task<Client> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
 
                Guid guid = Guid.Parse(userId);
                var cguid = ToCqlString(guid);
                PreparedStatement prepared = await _findById;
                BoundStatement bound = prepared.Bind(guid);

                RowSet rows = await CassandraSession.ExecuteAsync(bound).ConfigureAwait(false);
                var row = rows.SingleOrDefault();
                var user = FromRow(row);
                return user;

            }
            catch (Exception e)
            {
                return null;
            }
        }
        private static Client FromRow(Row row)
        {
            if (row == null)
                return null;
            var id = row.GetValue<Guid>("id");
            Client cc = row.GetValue<Client>("client");
            /*
            var dd = row.GetValue<byte[]>("client");
            string jsonStr = Encoding.UTF8.GetString(dd);
            var ccd = JsonConvert.DeserializeObject<Client>(jsonStr);
            */
           
         
            return cc;
        }
    }
}

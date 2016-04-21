using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using ConsoleApp.Cassandra.Products.Models;
using Newtonsoft.Json;
using p6.CassandraStore.DAO;
using p6.CassandraStore.Settings;


namespace ConsoleApp.Cassandra.Products.DAO
{
    public class CassandraDAO
    {
        private static ISession _cassandraSession = null;

        private static AsyncLazy<PreparedStatement> _CreateProductTemplate { get; set; }
        private static AsyncLazy<PreparedStatement> _FindProductTemplateById { get; set; }
        private static AsyncLazy<PreparedStatement> _CreateProductInstance { get; set; }
        public static AsyncLazy<PreparedStatement> _FindProductInstanceById { get; set; }
        private static AsyncLazy<PreparedStatement> _CreateBubbleRecord { get; set; }
        public static AsyncLazy<PreparedStatement> _FindBubbleRecordById { get; set; }


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
                            ContactPoints = new List<string> { "cassandra" },
                            Credentials = new CassandraCredentials() { Password = "", UserName = "" },
                            KeySpace = "wideproductrow"
                        };
                        var dao = new CassandraDao(cc);
                        _cassandraSession = dao.GetSession();

                        //-----------------------------------------------
                        // PREPARED STATEMENT
                        //-----------------------------------------------
                        _CreateProductTemplate =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"producttemplates(id,type,version,document) " +
                                        @"VALUES(?,?,?,?)");
                                    return result;
                                });
                        //-----------------------------------------------
                        // PREPARED STATEMENT
                        //-----------------------------------------------
                        _FindProductTemplateById =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * FROM producttemplates WHERE id = ?"));
                        //-----------------------------------------------
                        // PREPARED STATEMENT
                        //-----------------------------------------------
                        _CreateProductInstance =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"productinstances(id,type,version,document,bubbleid) " +
                                        @"VALUES(?,?,?,?,?)");
                                    return result;
                                });
                        //-----------------------------------------------
                        // PREPARED STATEMENT
                        //-----------------------------------------------
                        _FindProductInstanceById =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * FROM productinstances WHERE id = ?"));
                        //-----------------------------------------------
                        // PREPARED STATEMENT
                        //-----------------------------------------------
                        _CreateBubbleRecord =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"bubbles(id,bubblechainid,name,deviceid,deviceidtext) " +
                                        @"VALUES(?,?,?,?,?)");
                                    return result;
                                });
                        //-----------------------------------------------
                        // PREPARED STATEMENT
                        //-----------------------------------------------
                        _FindBubbleRecordById =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * FROM bubbles WHERE id = ?"));
                    }
                }
                catch (Exception e)
                {
                    _cassandraSession = null;
                }
                return _cassandraSession;
            }
        }


        public static async Task<bool> CreateProductTemplateAsync<T>(T documentRecord,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : IProductTemplate
        {
            var session = CassandraSession;
            cancellationToken.ThrowIfCancellationRequested();

            if (documentRecord == null)
                throw new ArgumentNullException(nameof(documentRecord));
            if (documentRecord.Document == null)
                throw new ArgumentNullException(nameof(documentRecord.Document));

            PreparedStatement prepared = await _CreateProductTemplate;
            BoundStatement bound = prepared.Bind(documentRecord.MetaData.TypeId
                , documentRecord.MetaData.Type
                , documentRecord.MetaData.Version
                , documentRecord.DocumentJson);

            await session.ExecuteAsync(bound).ConfigureAwait(false);
            return true;
        }
        public static async Task<IProductTemplate> FindProductTemplateByIdAsync(Guid id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var session = CassandraSession;
                cancellationToken.ThrowIfCancellationRequested();

                PreparedStatement prepared = await _FindProductTemplateById;
                BoundStatement bound = prepared.Bind(id);

                RowSet rows = await session.ExecuteAsync(bound).ConfigureAwait(false);
                var row = rows.SingleOrDefault();
                var documentRecord = ProductTemplateFromRow(row);
                return documentRecord;

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<bool> CreateProductInstanceAsync<T>(T documentRecord,
          CancellationToken cancellationToken = default(CancellationToken))
          where T : IProductInstance
        {
            var session = CassandraSession;
            cancellationToken.ThrowIfCancellationRequested();

            if (documentRecord == null)
                throw new ArgumentNullException(nameof(documentRecord));
            if (documentRecord.Document == null)
                throw new ArgumentNullException(nameof(documentRecord.Document));

            PreparedStatement prepared = await _CreateProductInstance;
            BoundStatement bound = prepared.Bind(documentRecord.MetaData.TypeId
                , documentRecord.MetaData.Type
                , documentRecord.MetaData.Version
                , documentRecord.DocumentJson
                , documentRecord.BubbleId);

            await session.ExecuteAsync(bound).ConfigureAwait(false);
            return true;
        }
        public static async Task<IProductInstance> FindProductInstanceByIdAsync(Guid id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var session = CassandraSession;
                cancellationToken.ThrowIfCancellationRequested();

                PreparedStatement prepared = await _FindProductInstanceById;
                BoundStatement bound = prepared.Bind(id);

                RowSet rows = await session.ExecuteAsync(bound).ConfigureAwait(false);
                var row = rows.SingleOrDefault();
                var documentRecord = ProductInstanceFromRow(row);
                return documentRecord;

            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static async Task<bool> CreateBubbleRecordAsync<T>(T documentRecord,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : IBubbleRecord
        {
            var session = CassandraSession;
            cancellationToken.ThrowIfCancellationRequested();

            if (documentRecord == null)
                throw new ArgumentNullException(nameof(documentRecord));

            PreparedStatement prepared = await _CreateBubbleRecord;
            //  @"bubbles(id,bubblechainid,name,deviceid,deviceidtext) " +

            BoundStatement bound = prepared.Bind(
                documentRecord.Id
                , documentRecord.BubbleChainId
                , documentRecord.Name
                , documentRecord.DeviceId
                , documentRecord.DeviceIdText);

            await session.ExecuteAsync(bound).ConfigureAwait(false);
            return true;
        }
        public static async Task<IBubbleRecord> FindBubbleRecordByIdAsync(Guid id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var session = CassandraSession;
                cancellationToken.ThrowIfCancellationRequested();

                PreparedStatement prepared = await _FindBubbleRecordById;
                BoundStatement bound = prepared.Bind(id);

                RowSet rows = await session.ExecuteAsync(bound).ConfigureAwait(false);
                var row = rows.SingleOrDefault();
                var documentRecord = BubbleRecordFromRow(row);
                return documentRecord;

            }
            catch (Exception e)
            {
                return null;
            }
        }


        private static IProductTemplate ProductTemplateFromRow(Row row)
        {
            if (row == null)
                return null;
            var tt = row.GetValue<string>("type");
            var type = Type.GetType(tt);
            var document = row.GetValue<string>("document");
            var theObject = JsonConvert.DeserializeObject(document, type);
            var documentRecord = new ProductTemplate<object>(theObject)
            {
                DocumentMetaData = new DocumentMetaData()
                {
                    Type = type.FullName,
                    TypeId = row.GetValue<Guid>("id"),
                    Version = row.GetValue<string>("version")
                }
            };

            return documentRecord;
        }

        private static IProductInstance ProductInstanceFromRow(Row row)
        {
            if (row == null)
                return null;
            var tt = row.GetValue<string>("type");
            var type = Type.GetType(tt);
            var document = row.GetValue<string>("document");
            var theObject = JsonConvert.DeserializeObject(document, type);
            var documentRecord = new ProductInstance<object>(theObject)
            {
                DocumentMetaData = new DocumentMetaData()
                {
                    Type = type.FullName,
                    TypeId = row.GetValue<Guid>("id"),
                    Version = row.GetValue<string>("version")
                },
                BubbleId = row.GetValue<Guid>("bubbleid"),

            };

            return documentRecord;
        }
        private static IBubbleRecord BubbleRecordFromRow(Row row)
        {
            if (row == null)
                return null;
            var documentRecord = new BubbleRecord()
            {
                Id = row.GetValue<Guid>("id"),
                BubbleChainId = row.GetValue<Guid>("bubblechainid"),
                DeviceId = row.GetValue<Guid>("deviceid"),
                DeviceIdText = row.GetValue<string>("deviceidtext"),
                Name = row.GetValue<string>("name")
            };

            return documentRecord;
        }
    }

}

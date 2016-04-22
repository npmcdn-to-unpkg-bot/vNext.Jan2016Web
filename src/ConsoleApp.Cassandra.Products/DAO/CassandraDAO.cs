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

        //-----------------------------------------------
        // PREPARED STATEMENTS for ProductTemplates
        //-----------------------------------------------
        private static AsyncLazy<PreparedStatement> _CreateProductTemplateById { get; set; }
        private static AsyncLazy<PreparedStatement> _CreateProductTemplateByType { get; set; }
        private static AsyncLazy<PreparedStatement[]> _CreateProductTemplate { get; set; }

        private static AsyncLazy<PreparedStatement> _FindProductTemplateById { get; set; }
        private static AsyncLazy<PreparedStatement> _FindProductTemplateByType { get; set; }
        private static AsyncLazy<PreparedStatement> _FindProductTemplateByTypeAndVersion { get; set; }

        //-----------------------------------------------
        // PREPARED STATEMENTS for ProductInstances
        //-----------------------------------------------
        private static AsyncLazy<PreparedStatement> _CreateProductInstanceById { get; set; }
        private static AsyncLazy<PreparedStatement> _CreateProductInstanceByLabel { get; set; }
        private static AsyncLazy<PreparedStatement[]> _CreateProductInstance { get; set; }
        public static AsyncLazy<PreparedStatement> _FindProductInstanceById { get; set; }
        public static AsyncLazy<PreparedStatement> _FindProductInstanceByLabel { get; set; }

        //-----------------------------------------------
        // PREPARED STATEMENTS for Bubbles
        //-----------------------------------------------
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
                        // PREPARED STATEMENTS for ProductTemplates
                        //-----------------------------------------------
                        _CreateProductTemplateById =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"producttemplates(id,documenttype,documentversion,document) " +
                                        @"VALUES(?,?,?,?)");
                                    return result;
                                });
                        _CreateProductTemplateByType =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"producttemplates_by_type(documenttype,documentversion,id,document) " +
                                        @"VALUES(?,?,?,?)");
                                    return result;
                                });
                        // All the statements needed by the CreateAsync method
                        _CreateProductTemplate = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
                        {
                            _CreateProductTemplateById.Value,
                            _CreateProductTemplateByType.Value,
                        }));
                        _FindProductTemplateById =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * "+
                                                                        "FROM producttemplates "+
                                                                        "WHERE id = ?"));
                        _FindProductTemplateByType =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * "+
                                                                        "FROM producttemplates_by_type "+
                                                                        "WHERE documenttype = ?"));
                        _FindProductTemplateByTypeAndVersion =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * "+
                                                                        "FROM producttemplates_by_type "+
                                                                        "WHERE documenttype = ? " +
                                                                        "AND documentversion = ?"));

                        //-----------------------------------------------
                        // PREPARED STATEMENTS for ProductInstances
                        //-----------------------------------------------


                        _CreateProductInstanceById =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"productinstances(id,label,producttemplateid,documenttype,documentversion,document,bubbleid) " +
                                        @"VALUES(?,?,?,?,?,?,?)");
                                    return result;
                                });
                        _CreateProductInstanceByLabel =
                            new AsyncLazy<PreparedStatement>(
                                () =>
                                {
                                    var result = _cassandraSession.PrepareAsync(
                                        @"INSERT INTO " +
                                        @"productinstances_by_label(label,id,producttemplateid,documenttype,documentversion,document,bubbleid) " +
                                        @"VALUES(?,?,?,?,?,?,?)");
                                    return result;
                                });
                        // All the statements needed by the CreateAsync method
                        _CreateProductInstance = new AsyncLazy<PreparedStatement[]>(() => Task.WhenAll(new[]
                        {
                            _CreateProductInstanceById.Value,
                            _CreateProductInstanceByLabel.Value,
                        }));

                        _FindProductInstanceById =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * FROM productinstances WHERE id = ?"));

                        _FindProductInstanceByLabel =
                            new AsyncLazy<PreparedStatement>(
                                () => _cassandraSession.PrepareAsync("SELECT * FROM productinstances_by_label WHERE label = ?"));

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

            PreparedStatement[] prepared = await _CreateProductTemplate;

            var preparedById = prepared[0];
            var preparedByType = prepared[1];

            BoundStatement boundById = preparedById.Bind(
                  documentRecord.Id
                , documentRecord.MetaData.Type
                , documentRecord.MetaData.Version
                , documentRecord.DocumentJson);

            //@"producttemplates_by_type(documenttype,documentversion,id,document) " +
            BoundStatement boundByType = preparedByType.Bind(
                  documentRecord.MetaData.Type
                , documentRecord.MetaData.Version
                , documentRecord.Id
                , documentRecord.DocumentJson);

            var batch = new BatchStatement();
            batch.Add(boundById);
            batch.Add(boundByType);

            await session.ExecuteAsync(batch).ConfigureAwait(false);
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
        public static async Task<List<IProductTemplate>> FindProductTemplateByTypeAsync(string type,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var session = CassandraSession;
                cancellationToken.ThrowIfCancellationRequested();

                PreparedStatement prepared = await _FindProductTemplateByType;
                BoundStatement bound = prepared.Bind(type);

                RowSet rowSet = await session.ExecuteAsync(bound).ConfigureAwait(false);
                var rows = rowSet.ToArray();
                List < IProductTemplate > result = new List<IProductTemplate>();
                foreach (var row in rows)
                {
                    var documentRecord = ProductTemplateFromRow(row);
                    result.Add(documentRecord);
                }
                return result;

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<IProductTemplate> FindProductTemplateByTypeAndVersionAsync(
            string type, string version,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var session = CassandraSession;
                cancellationToken.ThrowIfCancellationRequested();

                PreparedStatement prepared = await _FindProductTemplateByTypeAndVersion;
                BoundStatement bound = prepared.Bind(type);

                RowSet rowSet = await session.ExecuteAsync(bound).ConfigureAwait(false);
                var row = rowSet.SingleOrDefault();
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

            PreparedStatement[] prepared = await _CreateProductInstance;

            var preparedById = prepared[0];
            var preparedByLabel = prepared[1];

            //@"productinstances(id,label,producttemplateid,documenttype,documentversion,document,bubbleid) " +
            BoundStatement boundById = preparedById.Bind(
                 documentRecord.Id
               , documentRecord.Label
               , documentRecord.ProductTemplateId
               , documentRecord.MetaData.Type
               , documentRecord.MetaData.Version
               , documentRecord.DocumentJson
               , documentRecord.BubbleId);

            //@"productinstances(label,id,producttemplateid,documenttype,documentversion,document,bubbleid) " +
            BoundStatement boundByLabel = preparedByLabel.Bind(
                 documentRecord.Label
               , documentRecord.Id
               , documentRecord.ProductTemplateId
               , documentRecord.MetaData.Type
               , documentRecord.MetaData.Version
               , documentRecord.DocumentJson
               , documentRecord.BubbleId);

            var batch = new BatchStatement();
            batch.Add(boundById);
            batch.Add(boundByLabel);

            await session.ExecuteAsync(batch).ConfigureAwait(false);

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
        public static async Task<List<IProductInstance>> FindProductInstanceByLabelAsync(string label,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var session = CassandraSession;
                cancellationToken.ThrowIfCancellationRequested();

                PreparedStatement prepared = await _FindProductInstanceByLabel;
                BoundStatement bound = prepared.Bind(label);

                RowSet rowSet = await session.ExecuteAsync(bound).ConfigureAwait(false);
                var rows = rowSet.ToArray();
                var result = new List<IProductInstance>();
                foreach (var row in rows)
                {
                    var documentRecord = ProductInstanceFromRow(row);
                    result.Add(documentRecord);
                }
                return result;
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
            var tt = row.GetValue<string>("documenttype");
            var type = Type.GetType(tt);
            var document = row.GetValue<string>("document");
            var theObject = JsonConvert.DeserializeObject(document, type);
            var documentRecord = new ProductTemplate<object>(row.GetValue<Guid>("id"),theObject)
            {
                DocumentMetaData = new DocumentMetaData(type.FullName, row.GetValue<string>("documentversion"))
            };

            return documentRecord;
        }

        private static IProductInstance ProductInstanceFromRow(Row row)
        {
            if (row == null)
                return null;
            var tt = row.GetValue<string>("documenttype");
            var type = Type.GetType(tt);
            var document = row.GetValue<string>("document");
            var theObject = JsonConvert.DeserializeObject(document, type);
            var documentRecord = new ProductInstance<object>(row.GetValue<Guid>("id"),theObject)
            {
                DocumentMetaData = new DocumentMetaData(type.FullName, row.GetValue<string>("documentversion")),
                BubbleId = row.GetValue<Guid>("bubbleid"),
                Label = row.GetValue<string>("label"),
                ProductTemplateId = row.GetValue<Guid>("producttemplateid"),
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

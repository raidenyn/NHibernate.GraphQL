using System.Data.Common;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.GraphQL.Tests.Dto;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    public class DatabaseFixture
    {
        private readonly static ISessionFactory SessionFactory;
        private readonly static Configuration Configuration;
        private readonly static SchemaExport SchemaExport;

        protected ISession Session { get; private set; }

        static DatabaseFixture()
        {
            Configuration = new Configuration();
            SetMappings(Configuration);

            Configuration.DataBaseIntegration(x =>
            {
                x.Dialect<SQLiteDialect>();
                x.ConnectionString = "Data Source=:memory:;Version=3;New=True;";
                x.LogFormattedSql = true;
                x.LogSqlInConsole = true;
                x.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
                x.ConnectionProvider<Connection.DriverConnectionProvider>();
                x.Driver<Driver.SQLite20Driver>();
            });

            SessionFactory = Configuration.BuildSessionFactory();

            SchemaExport = new SchemaExport(Configuration);
        }

        [SetUp]
        public void SetUp()
        {
            Session = CreateSession();
            SchemaExport.Execute(
                useStdOut: true,
                execute: true,
                justDrop: false,
                connection: Session.Connection,
                exportOutput: null);
        }

        [TearDown]
        public void TearDown()
        {
            Session.Dispose();
            Session = null;
        }

        private ISession CreateSession()
        {
            ISession openSession = SessionFactory.OpenSession();
            DbConnection connection = openSession.Connection;
            return openSession;
        }

        private static void SetMappings(Configuration configuration)
        {
            var mapper = new ModelMapper();

            mapper.AddMappings(typeof(User).Assembly.GetTypes());

            var mappings = mapper.CompileMappingForAllExplicitlyAddedEntities();

            configuration.AddMapping(mappings);

            SchemaMetadataUpdater.QuoteTableAndColumns(configuration, new SQLiteDialect());
        }
    }
}

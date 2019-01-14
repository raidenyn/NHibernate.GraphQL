using System.Data.Common;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.GraphQL.Tests.Dto;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlCommand;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    public class DatabaseFixture
    {
        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        [OneTimeSetUp]
        public void Startup()
        {
            _configuration = new Configuration();
            SetMappings(_configuration);

            _configuration.DataBaseIntegration(x =>
            {
                x.Dialect<SQLiteDialect>();
                x.ConnectionString = "Data Source=:memory:;Version=3;New=True;";
                x.LogFormattedSql = true;
                x.LogSqlInConsole = true;
                x.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
                x.ConnectionProvider<Connection.DriverConnectionProvider>();
                x.Driver<Driver.SQLite20Driver>();
            });

            _sessionFactory = _configuration.BuildSessionFactory();
        }

        protected ISession CreateSession()
        {
            ISession openSession = _sessionFactory.OpenSession();
            DbConnection connection = openSession.Connection;
            new SchemaExport(_configuration).Execute(
                useStdOut: false,
                execute: true,
                justDrop: false,
                connection: connection,
                exportOutput: null);
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

using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using SL.EntityFramework.Interface;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations.Infrastructure;

namespace SL.EntityFramework
{
    /// <summary>
    /// Провайдер хранилища данных
    /// </summary>
    public class EfContextLite : DbContext, IUoW, IDisposable
    {
        private static bool _autoSetMigrations = false;

        public DbConnection DbConnection { get; private set; }

        protected static void AutoSetMigrations<TContext, TMigrationsConfiguration>()
            where TContext : DbContext
            where TMigrationsConfiguration : DbMigrationsConfiguration<TContext>, new()
        {
            if (_autoSetMigrations) return; 
            _autoSetMigrations = true; 
            
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration>(true));
        }

        private static DbConnection CreateMsSqlConnection(string connectionString)
            => new SqlConnection(connectionString);

        private static bool CheckModelChanged { get; set; } = false;

        private static DbConnection CreatePgConnection(string connectionString)
        {
            var connection = CreateMsSqlConnection(connectionString);

            if (!CheckModelChanged)
            {
                CheckModelChanged = true;

                //var createDatabaseIfNotExists = new CreateDatabaseIfNotExists<Context>();
                //using (var context = new Context(connection, false))
                //    createDatabaseIfNotExists.InitializeDatabase(context);

                //var dropCreateDatabaseIfModelChanges = new DropCreateDatabaseIfModelChanges<Context>();
                //using (var context = new Context(connection, false))
                //    dropCreateDatabaseIfModelChanges.InitializeDatabase(context);

                //var dropCreateDatabaseAlways = new DropCreateDatabaseAlways<Context>();
                //using (var context = new Context(connection, false))
                //    dropCreateDatabaseAlways.InitializeDatabase(context);
            }

            return connection;
        }

        protected EfContextLite(DbConnection dbConnection, bool disposeConnection = true)
            : base(dbConnection, disposeConnection)
        {
            DbConnection = dbConnection;
        }

        protected string _connectionString;
        public EfContextLite(string connectionString) : this(CreatePgConnection(connectionString), true)
        {
            _connectionString = connectionString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // /*имена таблиц в соответствии с именем класса
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // */
        }
    }
}

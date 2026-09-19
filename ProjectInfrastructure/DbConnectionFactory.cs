using Microsoft.Data.SqlClient;
using System.Data;

namespace HMS_360_PMS.HMS_360_PMS.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection(string connectionName);
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public IDbConnection CreateConnection(string connectionName)
        //{
        //    var connectionString = _configuration.GetConnectionString(connectionName);

        //    if (string.IsNullOrEmpty(connectionString))
        //        throw new Exception($"Connection string '{connectionName}' not found.");

        //    return new SqlConnection(connectionString);
        //}

        //public async Task<IDbConnection> CreateConnection(string connectionName)
        //{
        //    var connectionString = _configuration.GetConnectionString(connectionName);

        //    if (string.IsNullOrWhiteSpace(connectionString))
        //        throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        //    var connection = new SqlConnection(connectionString);
        //    await connection.OpenAsync();
        //    return connection;
        //}

        public IDbConnection CreateConnection(string connectionName)
        {
            var connectionString = _configuration.GetConnectionString(connectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

            return new SqlConnection(connectionString);
        }
    }
}

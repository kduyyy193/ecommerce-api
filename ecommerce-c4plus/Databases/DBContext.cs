using System.Data;
using MySql.Data.MySqlClient;

namespace Databases
{
    public class DBContext
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionstring;

        public DBContext(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

            connectionstring = _configuration.GetConnectionString("connection") ?? throw new ArgumentNullException(nameof(_configuration), "Configuration cannot be null.");
            if (string.IsNullOrWhiteSpace(connectionstring))
            {
                throw new InvalidOperationException("Connection string 'connection' not found in configuration.");
            }
        }

        public IDbConnection CreateConnection() => new MySqlConnection(connectionstring);
    }
}
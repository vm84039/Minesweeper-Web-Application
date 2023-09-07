using MySql.Data.MySqlClient;

namespace Minesweeper_Web_Application.Services.Data
{
    public class DatabaseHelper
    {
        private readonly string connectionString; // Your MySQL database connection string

        public DatabaseHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool IsDatabaseConnected()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true; // Connection succeeded
                }
                catch (MySqlException)
                {
                    return false; // Connection failed
                }
            }
        }
    }
}

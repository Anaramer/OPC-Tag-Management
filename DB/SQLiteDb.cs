using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;

namespace MyAvaloniaApp.DB
{
    public class SQLiteDb
    {
        public SqliteConnection Connection { get; }

        public SQLiteDb()
        {
            var csBuilder = new SqliteConnectionStringBuilder();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                csBuilder.DataSource = @"DB\OPCDataManagementDB.db";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            { 
                csBuilder.DataSource = @"DB\OPCDataManagementDB.db";
            }
            Connection = new SqliteConnection(csBuilder.ConnectionString);
        }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace MyAvaloniaApp.DB
{
    public class SettingDataAccess
    {
        public string? OPCServer { get; protected set; }
        public bool AutoStart { get; protected set; }
        public string? DbUserName { get; protected set; }
        public string? DbPassword { get; protected set; }
        public string? DbDataSource { get; protected set; }
        public string? OPCHost { get; protected set; }

        private readonly SQLiteDb CF;

        public SettingDataAccess()
        {
            CF = new SQLiteDb();
            ReadData();
        }

        public void ReadData()
        {
            using (CF.Connection)
            {
                var selectCommand = new SqliteCommand("SELECT * FROM tblSetting", CF.Connection);
                CF.Connection.Open();
                var sdr = selectCommand.ExecuteReader();
                sdr.Read();

                OPCServer = HasColumn(sdr, "ServerConnection") ? sdr.GetString(sdr.GetOrdinal("ServerConnection")) : "";
                AutoStart = HasColumn(sdr, "AutoStart") && sdr.GetBoolean(sdr.GetOrdinal("AutoStart"));
                DbUserName = HasColumn(sdr, "DbUserName") ? sdr.GetString(sdr.GetOrdinal("DbUserName")) : "";
                DbPassword = HasColumn(sdr, "DbPassword") ? sdr.GetString(sdr.GetOrdinal("DbPassword")) : "";
                DbDataSource = HasColumn(sdr, "DbDataSource") ? sdr.GetString(sdr.GetOrdinal("DbDataSource")) : "";
                OPCHost = HasColumn(sdr, "OPCHost") ? sdr.GetString(sdr.GetOrdinal("OPCHost")) : "";
                CF.Connection.Close();
            }
        }

        public void SaveToDb(string serverConnection, bool autoStart, string dbUserName, string dbPassword, string dbDataSource, string host)
        {
            try
            {
                var statement = "UPDATE tblSetting " +
                                $"SET ServerConnection = '{serverConnection}', AutoStart = {(autoStart ? 1 : 0)}, " +
                                $"DbUserName = '{dbUserName}', DbPassword='{dbPassword}', DbDataSource='{dbDataSource}', OPCHost='{host}' ";

                using (CF.Connection)
                {
                    var updateCommand = new SqliteCommand(statement, CF.Connection);
                    CF.Connection.Open();
                    updateCommand.ExecuteNonQuery();
                    CF.Connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public bool HasColumn(IDataRecord r, string columnName)
        {
            try
            {
                return r.GetOrdinal(columnName) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
    }
}

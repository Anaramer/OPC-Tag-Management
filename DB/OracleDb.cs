using MyAvaloniaApp.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace MyAvaloniaApp.DB
{
    public class OracleDb
    {
        public string OracleDbUser; //= "iopc";
        public string OracleDbPassword; //= "IOPC";
        public string OracleDbDataSource; //= @"localhost/l2vax1";

        private string _conStringUser;

        public OracleDb()
        {
            GenerateConnectionString();
        }

        public void GenerateConnectionString()
        {
            _conStringUser = "User Id=" + OracleDbUser + ";Password=" + OracleDbPassword + ";Data Source=" + OracleDbDataSource + ";";
        }

        public bool TestConnection(string username, string password, string dataSource)
        {
            try
            {
                using var con = new OracleConnection("User Id=" + username + ";Password=" + password + ";Data Source=" + dataSource + ";");
                con.Open();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool TestConnection()
        {
            return TestConnection(OracleDbUser, OracleDbPassword, OracleDbDataSource);
        }

        public void InsertValue(OpcTag tag, string value)
        {
            
            try
            {
                using var con = new OracleConnection(_conStringUser);
                var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = "INSERT INTO IOPC.IOPC_TRANS_TABLE (MESSAGEID,PRODUCTIONDATE,MODULENO,ITEMNAME,VALUE)" +
                                  " VALUES('PROD',:dateParam,:MODULENO,:ITEMNAME,:VALUE)";
                cmd.Parameters.Add(new OracleParameter("dateParam", OracleDbType.Date)).Value = DateTime.Now; //.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.Parameters.Add("MODULENO", OracleDbType.NVarchar2, tag.Module, ParameterDirection.Input);
                cmd.Parameters.Add("ITEMNAME", OracleDbType.NVarchar2, tag.SaveTagName, ParameterDirection.Input);
                cmd.Parameters.Add("VALUE", OracleDbType.NVarchar2, value, ParameterDirection.Input);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                InsertLog("InsertValue, Error,"+e.Message);
            }
        }

        public ObservableCollection<OpcTag> GetAllTags(ObservableCollection<OpcTag> OldList)
        {
            ObservableCollection<OpcTag> opcTagsList = new ObservableCollection<OpcTag>();
            try
            {
                using var con = new OracleConnection(_conStringUser);
                var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = "SELECT * FROM IOPC.IOPC_TAGS_DEFINITIONS ORDER BY ID";
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var newTag = new OpcTag();

                    newTag.ID = dr.GetInt32("ID");
                    newTag.TagName = dr.GetString("TAGNAME");
                    newTag.Module = dr.GetString("MODULE");
                    newTag.DataType = DataTypeUtility.String2DataType(dr.GetString("DATATYPE"));
                    newTag.SaveTagName = dr.GetString("SAVETAGASTAGNAME");
                    newTag.Description = dr.GetSchemaTable()!.Columns.Contains("DESCRIPTION") ? dr.GetString("DESCRIPTION") : "";
                    newTag.ReadingCycle = dr.GetInt32("READCYCLEINMILLISECONDS");
                    newTag.StartReadingDateTime = dr.GetDateTime("READINMILLICYCLESSTARTDATE");
                    newTag.RunEveryDayOnce = (dr.GetInt32("READEVERYDAYDEFINED") == 1 ? true : false);
                    newTag.EveryDayAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, dr.GetInt32("READEVERYDAYATHOUR"),
                        dr.GetInt32("READEVERYDAYATMINUTES"), dr.GetInt32("READEVERYDAYATSECONDS"));

                    var oldTag = OldList?.FirstOrDefault(p => p.ID == newTag.ID);
                    if (oldTag != null)
                    {
                        newTag.LastReadingDateTime = oldTag.LastReadingDateTime;
                    }


                    opcTagsList.Add(newTag);
                }
            }
            catch (OracleException e)
            {
                Console.WriteLine(e);
                //throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            return opcTagsList;
        }

        public void SaveTag(OpcTag opcTag, bool isUpdate)
        {
            try
            {
                string statement;
                if (!isUpdate)
                {
                    statement = "INSERT INTO IOPC.IOPC_TAGS_DEFINITIONS " +
                                "VALUES(:MODULE, :TAGNAME, :DATATYPE, :READCYCLEINMILLISECONDS, :READINMILLICYCLESSTARTDATE" +
                                ", :READEVERYDAYATHOUR, :READEVERYDAYATMINUTES, :READEVERYDAYATSECONDS, :READEVERYDAYDEFINED" +
                                ", :SAVETAGASTAGNAME, :DESCRIPTION, :ID)";
                }
                else
                {
                    statement = "UPDATE IOPC.IOPC_TAGS_DEFINITIONS SET " +
                                "MODULE = :MODULE , TAGNAME = :TAGNAME , DATATYPE = :DATATYPE , READCYCLEINMILLISECONDS = :READCYCLEINMILLISECONDS , " +
                                "READINMILLICYCLESSTARTDATE = :READINMILLICYCLESSTARTDATE , READEVERYDAYATHOUR = :READEVERYDAYATHOUR , " +
                                "READEVERYDAYATMINUTES = :READEVERYDAYATMINUTES , READEVERYDAYATSECONDS = :READEVERYDAYATSECONDS , " +
                                "READEVERYDAYDEFINED = :READEVERYDAYDEFINED , SAVETAGASTAGNAME = :SAVETAGASTAGNAME ,DESCRIPTION = :DESCRIPTION "+
                                "WHERE ID = :ID";
                }

                using var con = new OracleConnection(_conStringUser);
                var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = statement;

                cmd.Parameters.Add("MODULE", OracleDbType.NVarchar2, opcTag.Module, ParameterDirection.Input);
                cmd.Parameters.Add("TAGNAME", OracleDbType.NVarchar2, opcTag.TagName, ParameterDirection.Input);
                cmd.Parameters.Add("DATATYPE", OracleDbType.NVarchar2, DataTypeUtility.ToString(opcTag.DataType), ParameterDirection.Input);
                cmd.Parameters.Add("READCYCLEINMILLISECONDS", OracleDbType.Int32, opcTag.ReadingCycle, ParameterDirection.Input);
                cmd.Parameters.Add("READINMILLICYCLESSTARTDATE", OracleDbType.Date, opcTag.StartReadingDateTime, ParameterDirection.Input);
                cmd.Parameters.Add("READEVERYDAYATHOUR", OracleDbType.Int32, opcTag.EveryDayAt.Hour, ParameterDirection.Input);
                cmd.Parameters.Add("READEVERYDAYATMINUTES", OracleDbType.Int32, opcTag.EveryDayAt.Minute, ParameterDirection.Input);
                cmd.Parameters.Add("READEVERYDAYATSECONDS", OracleDbType.Int32, opcTag.EveryDayAt.Second, ParameterDirection.Input);
                cmd.Parameters.Add("READEVERYDAYDEFINED", OracleDbType.Int32, (opcTag.RunEveryDayOnce ? 1 : 0), ParameterDirection.Input);
                cmd.Parameters.Add("SAVETAGASTAGNAME", OracleDbType.NVarchar2, opcTag.SaveTagName, ParameterDirection.Input);
                cmd.Parameters.Add("DESCRIPTION", OracleDbType.NVarchar2, opcTag.Description, ParameterDirection.Input);
                cmd.Parameters.Add("ID", OracleDbType.Int32, opcTag.ID, ParameterDirection.Input);

                var a = cmd.CommandText;
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                InsertLog("SaveTag, Error," + e.Message);
            }
        }

        public int GetNextId()
        {
            try
            {
                using var con = new OracleConnection(_conStringUser);
                var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT MAX(ID) FROM IOPC.IOPC_TAGS_DEFINITIONS";
                con.Open();
                var result = Convert.ToInt32(cmd.ExecuteScalar() ?? 1) + 1;
                con.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void RemoveTag(int OpcTagId)
        {
            try
            {
                using var con = new OracleConnection(_conStringUser);
                var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = "DELETE FROM IOPC.IOPC_TAGS_DEFINITIONS WHERE ID = :ID";
                cmd.Parameters.Add("@ID", OracleDbType.Int32, OpcTagId, ParameterDirection.Input);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public void InsertLog(string Message)
        {

            try
            {
                using var con = new OracleConnection(_conStringUser);
                var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = "INSERT INTO IOPC.IOPC_EXCEPTIONS (DATE_TIME,EXCSOURCE,EXCSTACKTRACE,EXCMESSAGE)" +
                                  " VALUES(:DATE_TIME,:EXCSOURCE,:EXCSTACKTRACE,:EXCMESSAGE)";
                cmd.Parameters.Add(new OracleParameter("DATE_TIME", OracleDbType.Date)).Value = DateTime.Now; 
                cmd.Parameters.Add("EXCSOURCE", OracleDbType.NVarchar2, "OpcNetApi", ParameterDirection.Input);
                cmd.Parameters.Add("EXCSTACKTRACE", OracleDbType.NVarchar2, "at", ParameterDirection.Input);
                cmd.Parameters.Add("EXCMESSAGE", OracleDbType.NVarchar2, Message, ParameterDirection.Input);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

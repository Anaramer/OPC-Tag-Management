using Avalonia.Controls;
using GodSharp.Opc.Da;
using GodSharp.Opc.Da.Options;
using MyAvaloniaApp.DB;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpcLabs.BaseLib.Runtime.InteropServices;
using OpcXml.Da10;
using Tag = GodSharp.Opc.Da.Tag;

namespace MyAvaloniaApp.ViewModels
{
    public class DataViewModel : ReactiveObject
    {
        private bool _isloading = false;

        private ObservableCollection<OpcTag> _opcTags;
        public ObservableCollection<OpcTag> OpcTags
        {
            get => _opcTags;
            set => this.RaiseAndSetIfChanged(ref _opcTags, value);
        }

        private string _opcHost = "";
        public string OpcHost
        {
            get => _opcHost;
            set
            {
                this.RaiseAndSetIfChanged(ref _opcHost, value);
                SaveSetting();
            }
        }

        private string _opcServer;
        public string OpcServer
        {
            get => _opcServer;
            set
            {
                this.RaiseAndSetIfChanged(ref _opcServer, value);
                SaveSetting();
            }
        }

        private bool _cycleReadingState = false;
        public bool CycleReadingState
        {
            get => _cycleReadingState;
            set => this.RaiseAndSetIfChanged(ref _cycleReadingState, value);
        }

        private bool _autoStart;
        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                this.RaiseAndSetIfChanged(ref _autoStart, value);
                SaveSetting();
            }
        }

        private bool _dbConnect = false;
        public bool DbConnect
        {
            get => _dbConnect;
            set => this.RaiseAndSetIfChanged(ref _dbConnect, value);
        }

        public ObservableCollection<DataType> DataTypes { get; } = new ObservableCollection<DataType>()
        {
            DataType.Int16,
            DataType.Int32,
            DataType.Int64,
            DataType.Float,
            DataType.Double,
            DataType.String,
            DataType.Boolean
        };

        public ListBox? ListBoxConsole;
        public DataGrid? DataGridTags;

        private readonly SettingDataAccess _settingDataAccess;
        public readonly OracleDb OracleDb;

        public DataViewModel()
        {
            _isloading = true;
            DbConnect = false;
            _settingDataAccess = new SettingDataAccess();
            OracleDb = new OracleDb();
            ReadSetting();
            ReconnectDb();
            _isloading = false;
        }

        // Try to Connect To Oracle Db & Read Tag list
        public void ReconnectDb()
        {
            OracleDb.GenerateConnectionString();
            DbConnect = OracleDb.TestConnection();
            LoadTags();

        }

        private void ReadSetting()
        {
            OpcServer = _settingDataAccess.OPCServer!;
            AutoStart = _settingDataAccess.AutoStart;
            OracleDb.OracleDbUser = _settingDataAccess.DbUserName!;
            OracleDb.OracleDbPassword = _settingDataAccess.DbPassword!;
            OracleDb.OracleDbDataSource = _settingDataAccess.DbDataSource!;
            OpcHost = _settingDataAccess.OPCHost!;
        }

        // Save Setting Data in SqlLite
        public void SaveSetting()
        {
            if (!_isloading)
            {
                _settingDataAccess.SaveToDb(OpcServer, AutoStart, OracleDb.OracleDbUser, OracleDb.OracleDbPassword, OracleDb.OracleDbDataSource,OpcHost);
            }
        }

        // Load Setting Data in SqlLite
        public void LoadTags()
        {
            OpcTags = OracleDb.GetAllTags(OpcTags);
        }

        // Insert Or Update Tag on Oracle DB
        public void SaveOpcTag(OpcTag? tag)
        {
            if(tag == null) return;
            if (tag.ID == 0)
            {
                // new tag
                tag.ID = OracleDb.GetNextId();
                OracleDb.SaveTag(tag, false);
            }
            else
            {
                // update tag
                OracleDb.SaveTag(tag, true);
            }
            
        }

        //Remove Tag From Oracle DB
        public void RemoveTag(OpcTag? tag)
        {
            if (tag == null) return;
            OracleDb.RemoveTag(tag.ID);
        }

        public bool TestDbConnection(string username, string password, string dataSource)
        {
            return OracleDb.TestConnection(username, password, dataSource);
        }

        // Add text line to App Console box
        public void AddConsole(string text)
        {
            text = $"{DateTime.Now} : {text}";
            ListBoxConsole!.Items.Add(text);
            ListBoxConsole!.SelectedIndex = ListBoxConsole!.Items.Count - 1;
            if (ListBoxConsole!.Items.Count > 500) ListBoxConsole!.Items.RemoveAt(0);
        }

        //  Add Tag info in App Console box
        public void AddConsole(OpcTag? tag)
        {
            if( tag == null) return;
            var text = $"[{tag.ID}] {tag.TagName} => {tag.LastValue}";
            AddConsole(text);
        }

        // find tag in tag list and update last value
        private void UpdateValue(string tagName, string value)
        {
            var index = OpcTags.IndexOf(OpcTags.FirstOrDefault(p => string.Equals(p.TagName, tagName, StringComparison.CurrentCultureIgnoreCase))!);
            if (index == -1) return;
            OpcTags[index].LastValue = value;
        }

        public List<Tag> GenerateTagList()
        {
            List<Tag> tags = new List<Tag>();
            foreach (var tag in OpcTags)
            {
                tags.Add(new Tag(tag.TagName, tag.ID));
            }

            return tags;
        }

        // Starting schedule cycle for read data from OPC server and store them on Oracle DB
        public void StartReadingValues()
        {
            AddConsole("Reading Starting...");
            if (!DbConnect)
            {
                CycleReadingState = false;
                AddConsole("Error : Oracle Database is not Connected!");
                AddConsole("Reading Stopped");
                return;
            }
            if (string.IsNullOrWhiteSpace(OpcServer) || string.IsNullOrEmpty(OpcServer))
            {
                CycleReadingState = false;
                AddConsole("Error : OPC Server is not found!");
                AddConsole("Reading Stopped");
                return;
            }

            try
            {
                ComManagement.Instance.Configuration.SecurityParameters.EnsureDataIntegrity = true;
                ComManagement.Instance.AssureSecurityInitialization();

                //define tags for send to OPC server
                var tagGroups = new List<GroupData>
                {
                    new()
                    {
                        Name = "default", UpdateRate = 1000, ClientHandle = 010, IsSubscribed = true,
                        Tags = GenerateTagList(),
                    }
                };

                AddConsole("Connecting to OPC Server ...");

                var server = new ServerData
                {
                    Host = OpcHost,
                    ProgId = OpcServer,
                    Groups = tagGroups
                };

                

                //Connect to OPC Server for tag list
                var opcClient = DaClientFactory.Instance.CreateOpcNetApiClient(new DaClientOptions(
                    server,
                    OnDataChangedHandler,
                    OnShoutDownHandler,
                    null,
                    null));


                opcClient.Connect();
                AddConsole("OPC Server Connected");

                CycleReadingState = true;
                AddConsole("Reading Started");
            }
            catch (Exception e)
            {
                AddConsole("Can Not Starting OPC Server - Error : " + e.Message);
                OracleDb.InsertLog("Can't Starting OPC Server + " + e.Message);
                CycleReadingState = false;
            }
        }

        // Run this method when OPC Connection ShoutDowned
        private void OnShoutDownHandler(Server arg1, string arg2)
        {
            AddConsole("Disconnect OPC Server");
        }

        // Run this method when get new update of tags value for OPC Server
        private void OnDataChangedHandler(DataChangedOutput output)
        {
            UpdateValue(output.Data.ItemName, string.Empty + output.Data.Value);
        }

        // Stopping Schedule cycle
        public void StopReadingValues()
        {
            try
            {
                CycleReadingState = false;
                AddConsole("Reading Stopped");
            }
            catch (Exception e)
            {
                AddConsole("Can NOT Stopping OPC Server - Error : " + e.Message);
                OracleDb.InsertLog("Can't Stopping OPC Server + " + e.Message);
            }
            
        }
    }
}

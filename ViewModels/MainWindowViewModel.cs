using Avalonia.Controls;
using OpcLabs.EasyOpc.DataAccess;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyAvaloniaApp.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ListBox? ListBoxConsole;
        public DataGrid? DataGridTags;

        private bool _cycleReadingState;
        public bool CycleReadingState
        {
            get => _cycleReadingState;
            set => this.RaiseAndSetIfChanged(ref _cycleReadingState, value);
        }

        public string OpcServer { get; set; } = "Matrikon.OPC.Simulation.1";

        private ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> Tags
        {
            get => _tags;
            set => this.RaiseAndSetIfChanged(ref _tags, value);
        }

        public ObservableCollection<DataType> DataTypes { get; }

        private Dictionary<int, int> handles;

        public MainWindowViewModel()
        {
            handles = new Dictionary<int, int>();
            Tags = new ObservableCollection<Tag>
            {
                new (1,"1","Random.Int1",DataType.Int32,"A1","",false,new DateTime(2023,1,1)),
                //new (2,"1","Random.String",DataType.String,"B1","",false,new DateTime(2023,1,1)),
                //new (3,"1","Random.Int4",DataType.Int32,"C1","",false,new DateTime(2023,1,1)),
            };

            DataTypes = new ObservableCollection<DataType>
            {
                DataType.Int16,
                DataType.Int32,
                DataType.Int64,
                DataType.Float,
                DataType.Double,
                DataType.String,
                DataType.Boolean
            };
        }

        private void AddConsole(int tagId,string value)
        {
            Tag tag = Tags[tagId];
            var txt = $"Tag[{tag.ID}]  {tag.TagName}  =>  {value}";
            AddConsole(txt);
        }

        public void AddConsole(string text)
        {
            text = $"{DateTime.Now} :  {text}";
            ListBoxConsole!.Items.Add(text);
            if (ListBoxConsole!.Items.Count > 10) ListBoxConsole!.Items.RemoveAt(0);
        }

        private void AddConsole(string tagName, string value)
        {
            Tag tag = Tags.FirstOrDefault(p => string.Equals(p.TagName, tagName, StringComparison.CurrentCultureIgnoreCase))!;
            var txt = $"Tag[{tag.ID}]  {tag.TagName}  =>  {value}";
            AddConsole(txt);
        }

        private void UpdateValue(string tagName, string value)
        {
            var index = Tags.IndexOf(Tags.FirstOrDefault(p => string.Equals(p.TagName, tagName, StringComparison.CurrentCultureIgnoreCase))!);
            if (index == -1) return;
            DataGridTags!.ItemsSource = Tags;
            Tags[index].LastValue = value;
            
        }

        public void StartReadingValues()
        {
            AddConsole("Starting ...");
            if (string.IsNullOrWhiteSpace(OpcServer))
            {
                CycleReadingState = false;
            }

            foreach (Tag tag in Tags)
            {
                if (handles.ContainsKey(tag.ID)) continue;
                int tagHandle = EasyDAClient.SharedInstance.SubscribeItem("", OpcServer, tag.TagName, 1000,
                    (_, eventArgs) =>
                    {
                        if (eventArgs is null) return;
                        if (!(eventArgs.Vtq is null))
                        {
                            //    Console.WriteLine("ItemID:" + eventArgs.Arguments.ItemDescriptor.ItemId);
                            //    Console.WriteLine("Value:" + eventArgs.Vtq.Value);
                            //    Console.WriteLine("Timestamp:" + eventArgs.Vtq.Timestamp);
                            //    Console.WriteLine("Quality:" + (short)eventArgs.Vtq.Quality);
                            //var value = eventArgs.Vtq.Value;
                            UpdateValue(string.Empty +eventArgs.Arguments.ItemDescriptor.ItemId, string.Empty + eventArgs.Vtq.Value);
                            AddConsole(string.Empty + eventArgs.Arguments.ItemDescriptor.ItemId, string.Empty + eventArgs.Vtq.Value);
                            //tag.LastValue = value.ToString();

                        }
                    }
                );
                handles.Add(tag.ID, tagHandle);
            }

            CycleReadingState = true;
        }

        public void StopReadingValues()
        {
            foreach (var handle in handles)
            {
                EasyDAClient.SharedInstance.UnsubscribeItem(handle.Value);
                handles.Remove(handle.Key);
            }

            CycleReadingState = false;
        }
    }
}

using System;

namespace MyAvaloniaApp.ViewModels
{
    public class OpcTag
    {
        public int ID { get; set; }
        public string Module { get; set; }
        public string TagName { get; set; }
        public DataType DataType { get; set; }
        public string SaveTagName { get; set; }
        public string? Description { get; set; }
        public DateTime StartReadingDateTime { get; set; }
        public bool RunEveryDayOnce { get; set; }
        public DateTime EveryDayAt { get; set; } // As second ; for example : 08:20:15 AM => 8*3600 + 20*60 + 15 = 30015
        public int ReadingCycle { get; set; } // As milliseconds
        public DateTime LastReadingDateTime { get; set; }
        public string LastValue { get; set; }

        public OpcTag(int id, string module, string tagName, DataType dataType, string aliasTagName, string description, bool runEveryDayOnce, DateTime startReadingDateTime)
        {
            ID = id;
            Module = module;
            TagName = tagName;
            DataType = dataType;
            SaveTagName = aliasTagName;
            Description = description;
            RunEveryDayOnce = runEveryDayOnce;
            StartReadingDateTime = startReadingDateTime;
        }

        public OpcTag() { }
    }
}

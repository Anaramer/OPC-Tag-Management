using Newtonsoft.Json.Linq;
using System;

namespace MyAvaloniaApp.ViewModels
{
    public enum DataType
    {
        Int16 = 0,
        Int32 = 1,
        Int64 = 2,
        Float = 3,
        Double = 4,
        String = 5,
        Boolean = 6,
    }

    public static class DataTypeUtility
    {
        public static int ToInt(this DataType value)
        {
            return value switch
            {
                DataType.Int16 => 0,
                DataType.Int32 => 1,
                DataType.Int64 => 2,
                DataType.Float => 3,
                DataType.Double => 4,
                DataType.String => 5,
                DataType.Boolean => 6,
                _ => 0
            };
        }

        public static string ToString(this DataType value)
        {
            return value switch
            {
                DataType.Int16 => "Int16",
                DataType.Int32 => "Int32",
                DataType.Int64 => "Int64",
                DataType.Float => "Float",
                DataType.Double => "Double",
                DataType.String => "String",
                DataType.Boolean => "Boolean",
                _ => "Int16"
            };
        }

        public static DataType String2DataType(string name)
        {
            return name switch
            {
                "Int16" => DataType.Int16,
                "Int32" => DataType.Int32,
                "Int64" => DataType.Int64,
                "Float" => DataType.Float,
                "Double" => DataType.Double,
                "String" => DataType.String,
                "Boolean" => DataType.Boolean,
                _ => DataType.Int16
            };
        }
    }
}

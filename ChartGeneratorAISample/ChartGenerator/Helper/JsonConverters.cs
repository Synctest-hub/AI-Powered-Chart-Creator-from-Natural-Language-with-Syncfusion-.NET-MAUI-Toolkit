using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartGenerator
{
    /// <summary>
    /// Converter for handling ChartTypeEnum values from string
    /// </summary>
    public class ChartTypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ChartTypeEnum) || objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string value = reader.Value.ToString().ToLower();
                
                if (value == "cartesian")
                    return ChartTypeEnum.Cartesian;
                else if (value == "circular")
                    return ChartTypeEnum.Circular;
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                int value = Convert.ToInt32(reader.Value);
                if (Enum.IsDefined(typeof(ChartTypeEnum), value))
                    return (ChartTypeEnum)value;
            }

            // Default to Cartesian if can't convert
            return ChartTypeEnum.Cartesian;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ChartTypeEnum chartType = (ChartTypeEnum)value;
            writer.WriteValue(chartType.ToString().ToLower());
        }
    }

    /// <summary>
    /// Converter for handling SeriesType values from string
    /// </summary>
    public class SeriesTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SeriesType) || objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string value = reader.Value.ToString().ToLower();
                
                switch (value)
                {
                    case "line": return SeriesType.Line;
                    case "column": return SeriesType.Column;
                    case "spline": return SeriesType.Spline;
                    case "area": return SeriesType.Area;
                    case "pie": return SeriesType.Pie;
                    case "doughnut": return SeriesType.Doughnut;
                    case "radialbar": return SeriesType.RadialBar;
                    default: return SeriesType.Column; // Default to column
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                int value = Convert.ToInt32(reader.Value);
                if (Enum.IsDefined(typeof(SeriesType), value))
                    return (SeriesType)value;
            }

            // Default to Column if can't convert
            return SeriesType.Column;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            SeriesType seriesType = (SeriesType)value;
            writer.WriteValue(seriesType.ToString().ToLower());
        }
    }
}
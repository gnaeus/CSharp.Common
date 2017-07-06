using System;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Common.Converters
{
    /// <summary>
    /// Custom value converter for passing string properties as RAW JSON values.
    /// </summary>
    public class RawJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JRaw.Create(reader).Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)value);
        }
    }
}

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Toggle.Utils
{
    [Preserve]
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            Vector2Int v = new Vector2Int
            {
                x = obj["x"].Value<int>(),
                y = obj["y"].Value<int>()
            };

            return v;
        }
    }
}
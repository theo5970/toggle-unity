using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Toggle.Core.Json
{
    [Preserve]
    public class ToggleLevelConverter : JsonConverter<ToggleLevel>
    {
        public override void WriteJson(JsonWriter writer, ToggleLevel level, JsonSerializer serializer)
        {
            string encodedText;
            using (ToggleLevelWriter levelWriter = new ToggleLevelWriter())
            {
                levelWriter.Write(level);
                encodedText = levelWriter.ToBase64();
            }

            writer.WriteValue(encodedText);
        }

        public override ToggleLevel ReadJson(JsonReader reader, Type objectType, ToggleLevel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string encodedText = reader.Value as string;
            return new ToggleLevel(ToggleLevelReader.FromBase64(encodedText));
        }
    }
}
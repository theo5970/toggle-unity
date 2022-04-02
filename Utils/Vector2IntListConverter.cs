using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

namespace Toggle.Utils
{
    public class Base64Vector2IntListConverter : JsonConverter<List<Vector2Int>>
    {
        private static Random seedRandom = new Random();
        private readonly HashAlgorithm hashAlgorithm = MD5.Create();
        
        private JsonSerializer legacySerializer;
        public Base64Vector2IntListConverter()
        {
            var legacySerializerSettings = new JsonSerializerSettings();
            legacySerializerSettings.Converters.Add(new Vector2IntConverter());
            legacySerializer = JsonSerializer.Create(legacySerializerSettings);

        }
        
        public override void WriteJson(JsonWriter writer, List<Vector2Int> value, JsonSerializer serializer)
        {
            int capacity = 4 + value.Count * sizeof(int);
            using (MemoryStream memoryStream = new MemoryStream(capacity))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    int seed = seedRandom.Next();
                    binaryWriter.Write(seed);
                    binaryWriter.Write(value.Count);

                    byte[] hash = hashAlgorithm.ComputeHash(BitConverter.GetBytes(seed));
                    for (int i = 0; i < value.Count; i++)
                    {
                        binaryWriter.Write((byte)(value[i].x ^ hash[i % hash.Length]));
                        binaryWriter.Write((byte)(value[i].y ^ hash[(i + 1) % hash.Length]));
                    }
                }

                byte[] bytes = memoryStream.ToArray();
                writer.WriteValue(Convert.ToBase64String(bytes));
            }
        }

        public override List<Vector2Int> ReadJson(JsonReader reader, Type objectType, List<Vector2Int> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            if (reader.TokenType == JsonToken.StartArray)
            {
                result = JArray.Load(reader).ToObject<List<Vector2Int>>(legacySerializer);
            }
            else
            {
                string encodedText = reader.Value as string;

                if (encodedText != null)
                {
                    byte[] bytes = Convert.FromBase64String(encodedText);

                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                        {
                            int seed = binaryReader.ReadInt32();
                            int count = binaryReader.ReadInt32();
                            
                            byte[] hash = hashAlgorithm.ComputeHash(BitConverter.GetBytes(seed));
                            
                            for (int i = 0; i < count; i++)
                            {
                                int x = binaryReader.ReadByte() ^ hash[i % hash.Length];
                                int y = binaryReader.ReadByte() ^ hash[(i + 1) % hash.Length];
                                
                                result.Add(new Vector2Int(x, y));
                            }

                        }
                    }
                }
            }
            return result;
        }
    }
}
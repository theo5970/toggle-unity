using System.Collections.Generic;
using Newtonsoft.Json;
using Toggle.Core;
using Toggle.Core.Json;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.Data
{
    public class ToggleClassicLevel
    {
        [JsonConverter(typeof(ToggleLevelConverter))]
        public ToggleLevel data;

        [JsonConverter(typeof(Base64Vector2IntListConverter))]
        public List<Vector2Int> generateOrders;

        [JsonIgnore]
        public int solverIterations { get; set; }
    }
}
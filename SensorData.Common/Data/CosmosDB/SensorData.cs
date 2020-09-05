using Newtonsoft.Json;
using System;

namespace MySensorData.Common.Data.CosmosDB
{
    public class SensorData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public string Payload { get; set; }
    }
}

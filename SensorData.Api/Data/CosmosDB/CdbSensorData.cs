using Newtonsoft.Json;
using System;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbSensorData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public string Payload { get; set; }
    }
}

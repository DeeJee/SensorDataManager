using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbNotification
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string LogLevel { get; set; }
        public string Text { get; set; }
        public DateTime? Created { get; set; }
        public string EntityType = "Notification";        
    }
}

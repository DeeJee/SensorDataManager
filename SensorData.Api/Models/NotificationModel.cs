using Newtonsoft.Json;
using System;

namespace SensorData.Api.Models
{
    public class NotificationModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string LogLevel { get; set; }
        public string Text { get; set; }
        public DateTime? Created { get; set; }
    }
}

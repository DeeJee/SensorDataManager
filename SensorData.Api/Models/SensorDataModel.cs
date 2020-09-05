using System;

namespace SensorData.Api.Models
{
    public class SensorDataModel
    {
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public string Payload { get; set; }
    }
}
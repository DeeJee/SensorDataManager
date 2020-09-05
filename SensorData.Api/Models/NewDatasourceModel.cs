using System;

namespace SensorData.Api.Models
{
    public class NewDatasourceModel
    {
        public string DeviceId { get; set; }
        public int Count { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
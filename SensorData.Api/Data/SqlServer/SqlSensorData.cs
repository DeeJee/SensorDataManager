using Newtonsoft.Json;
using System;

namespace SensorData.Api.Data.SqlServer
{
    public class SqlSensorData
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public string Payload { get; set; }
    }
}

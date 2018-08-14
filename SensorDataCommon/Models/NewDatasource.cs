using System;

namespace SensorDataCommon.Models
{
    public class NewDatasource
    {
        public string DeviceId { get; set; }
        public int Count { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
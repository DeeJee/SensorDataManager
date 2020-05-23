using System;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public partial class SensorData
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public string Payload { get; set; }
    }
}

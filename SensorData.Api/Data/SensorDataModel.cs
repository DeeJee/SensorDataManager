using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SensorData.Api.Data
{
    public partial class SensorDataModel
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public string Payload { get; set; }
    }
}

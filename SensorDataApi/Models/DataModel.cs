using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SensorDataApi.Models
{
    public class DataModel
    {
        public int Id { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string DeviceId { get; set; }
        public object Payload { get; set; }
    }
}
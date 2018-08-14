using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SensorDataApi.Models
{
    public class Datasource
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public int DataTypeId { get; set; }
        public string Description { get; set; }

    }
}
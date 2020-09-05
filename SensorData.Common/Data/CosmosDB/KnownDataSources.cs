using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySensorData.Common.Data.CosmosDB
{
    public class DataSource
    {
        public string id { get; set; }
        public string EntityType = "DataSource";
        public string DeviceId { get; set; }
        public Registration Registration { get; set; }
    }

    public class Registration
    {
        public string Description { get; set; }
        public string DatatypeId { get; set; }
    }
}

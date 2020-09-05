using System;
using System.Collections.Generic;

namespace SensorData.Api.Data.SqlServer
{
    public partial class SqlChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace SensorData.Api.Data
{
    public partial class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public bool Active { get; set; }
    }
}

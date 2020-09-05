using SensorData.Api.Data;
using SensorData.Api.Data.SqlServer;
using System;
using System.Collections.Generic;

namespace SensorData.Api.Models
{
    public class DataTypeModel
    {
        public DataTypeModel()
        {
            DataSource = new HashSet<SqlDataSource>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Properties { get; set; }

        public virtual ICollection<SqlDataSource> DataSource { get; set; }
    }
}

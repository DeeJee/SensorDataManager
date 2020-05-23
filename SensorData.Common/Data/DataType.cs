using System;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public partial class DataType
    {
        public DataType()
        {
            DataSource = new HashSet<DataSource>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Properties { get; set; }

        public virtual ICollection<DataSource> DataSource { get; set; }
    }
}

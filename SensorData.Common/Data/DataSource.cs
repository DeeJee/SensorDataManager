using System;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public partial class DataSource
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public int? ChannelId { get; set; }
        public string Description { get; set; }
        public int DataTypeId { get; set; }
        public byte[] Image { get; set; }

        public virtual DataType DataType { get; set; }
    }
}
